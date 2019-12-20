using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MultiScaleWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Point currentPoint = new Point();
        MainFile mainFile;
        Task task;
        CancellationTokenSource wtoken;
        bool uiNotBlockedFlag = false;
        public bool firstSetUpDone = false;

        public MainWindow()
        {
            InitializeComponent();
            CreateNewMainFileInstanceDefaultValues();
            PaintSurface.Width = mainFile.windowWidth;
            PaintSurface.Height = mainFile.windowHeight;
            uiNotBlockedFlag = true;
        }

        private void Canvas_MouseDown_1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (uiNotBlockedFlag)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    currentPoint = e.GetPosition(PaintSurface);

                    Rectangle rec = new Rectangle();
                    Canvas.SetTop(rec, currentPoint.Y);
                    Canvas.SetLeft(rec, currentPoint.X);
                    rec.Width = mainFile.inclusionDiameter;
                    rec.Height = mainFile.inclusionDiameter;
                    rec.Fill = new SolidColorBrush(Colors.Black);
                    PaintSurface.Children.Add(rec);

                    //set it to not reset this cell
                    mainFile.cellArray[Convert.ToInt32(currentPoint.X), Convert.ToInt32(currentPoint.Y)].cellState = Cell.CellState.Inclusion;

                    mainFile.cellArray[Convert.ToInt32(currentPoint.X), Convert.ToInt32(currentPoint.Y)].cellColor = System.Drawing.Color.Black;// Color.FromRgb(0,0,0);
                                                                                                                                                //and add status for that
                    mainFile.cellArray[Convert.ToInt32(currentPoint.X), Convert.ToInt32(currentPoint.Y)].cellId = Convert.ToInt32(currentPoint.X) + Convert.ToInt32(currentPoint.Y) * mainFile.windowWidth;
                }
            }
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            wtoken = new CancellationTokenSource();

            task = Task.Run(async () =>  // <- marked async
            {
                while (true)
                {
                    mainFile.AppWorkflow();
                    mainFile.blockedConfiguration = true;
                    uiNotBlockedFlag = false;
                    Dispatcher.Invoke(new Action(() => {

                        IsUIBlockedReadonly(uiNotBlockedFlag);

                    }), DispatcherPriority.ContextIdle);

                    Dispatcher.Invoke(new Action(() => {

                        image.Source = DrawImage(mainFile.testArray);

                    }), DispatcherPriority.ContextIdle);
                    if (mainFile.stopWorkFlowFlag)
                    {
                        StopMethod();

                        Dispatcher.Invoke(new Action(() => {

                            inclusionAfterEndButton.IsEnabled = true;

                        }), DispatcherPriority.ContextIdle);
                    }
                    await Task.Delay(100, wtoken.Token); // <- await with cancellation
                }
            }, wtoken.Token);
        }

        private void NeumanButton_Checked(object sender, RoutedEventArgs e)
        {
            if (uiNotBlockedFlag)
                mainFile.neighbourhoodType = MainFile.NeighbourhoodType.vonNeuman;
        }
        private void MooreButton_Checked(object sender, RoutedEventArgs e)
        {
            if (uiNotBlockedFlag)
                mainFile.neighbourhoodType = MainFile.NeighbourhoodType.Moore;
        }

        private void absorbingButton_Checked(object sender, RoutedEventArgs e)
        {
            if (uiNotBlockedFlag)
                mainFile.boundaryCondition = MainFile.BoundaryCondition.absorbing;
        }
        private void periodicButton_Checked(object sender, RoutedEventArgs e)
        {
            if (uiNotBlockedFlag)
                mainFile.boundaryCondition = MainFile.BoundaryCondition.periodic;
        }
        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            StopMethod();
        }
        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            StopMethod();

            uiNotBlockedFlag = true;
            Dispatcher.Invoke(new Action(() => {
                IsUIBlockedReadonly(uiNotBlockedFlag);

            }), DispatcherPriority.ContextIdle);
            CreateNewMainFileInstanceDefaultValues();
            image.Source = DrawImage(mainFile.testArray);
        }
        private int TryParseText(string text, int defaultValue)
        {
            if (Int32.TryParse(text, out int returnedValue))
                return returnedValue;
            else
                return defaultValue;
        }

        private int GetUiValue(string fieldName)
        {
            int value = 0;
            switch (fieldName)
            {
                case "widthTextBox":
                    value = TryParseText(widthTextBox.Text, 300);
                    break;
                case "heightTextBox":
                    value = TryParseText(heightTextBox.Text, 300);
                    break;
                case "numberGrainsTextBox":
                    value = TryParseText(numberGrainsTextBox.Text, 5);
                    break;
                case "inclusionDiameterTextBox":
                    value = TryParseText(inclusionDiameterTextBox.Text, 5);
                    break; 
                case "inclusionNumberTextBox":
                    value = TryParseText(inclusionNumberTextBox.Text, 3);
                    break;
            }
            return value;
        }

        private void IsUIBlockedReadonly(bool boolSwitchValue)
        {
            widthTextBox.IsReadOnly = !boolSwitchValue;
            heightTextBox.IsReadOnly = !boolSwitchValue;
            numberGrainsTextBox.IsReadOnly = !boolSwitchValue;
            inclusionDiameterTextBox.IsReadOnly = !boolSwitchValue;
            inclusionNumberTextBox.IsReadOnly = !boolSwitchValue;

            NeumanButton.IsEnabled = boolSwitchValue;
            MooreButton.IsEnabled = boolSwitchValue;

            absorbingButton.IsEnabled = boolSwitchValue;
            periodicButton.IsEnabled = boolSwitchValue;

            squareInclusionButton.IsEnabled = boolSwitchValue;
            CircleInclusionButton.IsEnabled = boolSwitchValue;

            heterogenousButton.IsEnabled = boolSwitchValue;
            homogenousButton.IsEnabled = boolSwitchValue;

        }

        private void CreateNewMainFileInstanceDefaultValues()
        {
            mainFile = new MainFile();

            mainFile.grainNumber = GetUiValue("numberGrainsTextBox");
            mainFile.windowHeight = GetUiValue("heightTextBox");
            mainFile.windowWidth = GetUiValue("widthTextBox");
            mainFile.inclusionDiameter = GetUiValue("inclusionDiameterTextBox");
            mainFile.inclusionNumber = GetUiValue("inclusionNumberTextBox");
            mainFile.borderWidthPx = 5; //future development
            mainFile.blockedConfiguration = false;
            mainFile.cellArray = new Cell[mainFile.windowWidth, mainFile.windowHeight];

             if(absorbingButton.IsChecked.HasValue && absorbingButton.IsChecked == true)
                 mainFile.boundaryCondition = MainFile.BoundaryCondition.absorbing;
             else
                mainFile.boundaryCondition = MainFile.BoundaryCondition.periodic;

             if (NeumanButton.IsChecked.HasValue && NeumanButton.IsChecked == true)
                 mainFile.neighbourhoodType = MainFile.NeighbourhoodType.vonNeuman;
             else
                mainFile.neighbourhoodType = MainFile.NeighbourhoodType.Moore;

            if (heterogenousButton.IsChecked.HasValue && heterogenousButton.IsChecked == true)
                mainFile.energyType = MainFile.EnergyType.heterogenous;
            else
                mainFile.energyType = MainFile.EnergyType.homogenous;

            if (squareInclusionButton.IsChecked.HasValue && squareInclusionButton.IsChecked == true)
                mainFile.grainShape = MainFile.GrainShape.Square;
            else
                mainFile.grainShape = MainFile.GrainShape.Round;

            mainFile.testArray = new Int32[mainFile.windowWidth, mainFile.windowHeight];
            for (int i = 0; i < mainFile.windowWidth; i++)
            {
                for (int j = 0; j < mainFile.windowHeight; j++)
                {
                    mainFile.testArray[i, j] = mainFile.GetColor(255, 255, 255);
                }
            }

            mainFile.colorListDict = new Dictionary<int, System.Drawing.Color>();

            for (int i = 0; i < mainFile.windowWidth; i++)
            {
                for (int j = 0; j < mainFile.windowHeight; j++)
                {
                    if (mainFile.blockedConfiguration && mainFile.cellArray[i, j].cellState == Cell.CellState.Inclusion)
                        continue;
                    mainFile.cellArray[i, j] = new Cell();
                    mainFile.cellArray[i, j].cellId = i + j * mainFile.windowWidth;
                }
            }
        }

        private void numberGrainsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (uiNotBlockedFlag)
            {
                if (Int32.TryParse(numberGrainsTextBox.Text, out int parseResult))
                {
                    if (parseResult > 100)
                        parseResult = 100; //max value 100
                    if (parseResult < 1)
                        parseResult = 1;
                    mainFile.inclusionDiameter = parseResult;
                }
                else
                    mainFile.grainNumber = 5;
            }
        }

        private void inclusionDiameterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (uiNotBlockedFlag)
            {
                if (Int32.TryParse(inclusionDiameterTextBox.Text, out int parseResult))
                {
                    if (parseResult > 50)
                        parseResult = 50; //max value 50
                    if (parseResult < 1)
                        parseResult = 1;
                    mainFile.inclusionDiameter = parseResult;
                }
                else
                    mainFile.inclusionDiameter = 5;
            }
        }

        private void txtImport_Click(object sender, RoutedEventArgs e)
        {
            if (uiNotBlockedFlag)
            {
                var openFileDialog = new OpenFileDialog();

                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "All Files (*.*)|*.*";

                openFileDialog.FilterIndex = 1;
                openFileDialog.Multiselect = false;
                bool? dialogBool = openFileDialog.ShowDialog();
                if (dialogBool != null && dialogBool == true && openFileDialog.CheckFileExists)
                {                    
                    using (StreamReader file = new StreamReader(openFileDialog.FileName))
                    {
                        string line;
                        int x = 0; int y = 0; bool firstLine = true;
                        while ((line = file.ReadLine()) != null)
                        {
                            
                            string[] lineArray = line.Split(" ");
                            if (!Int32.TryParse(lineArray[0], out int value))
                                continue;

                            if (firstLine)
                            {
                                mainFile.windowWidth = Convert.ToInt32( lineArray[0]);
                                mainFile.windowHeight = Convert.ToInt32( lineArray[1]);
                                firstLine = false;
                                continue;
                            }

                            if (x == mainFile.windowWidth )
                            {
                                x = 0;
                                y++;
                            }

                            mainFile.cellArray[x, y].cellId = Convert.ToInt32( lineArray[0]);
                            mainFile.cellArray[x, y].cellColor = System.Drawing.Color.FromArgb(255, Convert.ToInt32(lineArray[1]), Convert.ToInt32(lineArray[2]), Convert.ToInt32(lineArray[3]));
                            if (mainFile.cellArray[x, y].cellColor == System.Drawing.Color.Black)
                            {
                                mainFile.cellArray[x, y].cellState = Cell.CellState.Inclusion;
                            }
                            else if(mainFile.cellArray[x, y].cellColor == System.Drawing.Color.White)
                            {
                                mainFile.cellArray[x, y].cellState = Cell.CellState.Empty;
                            }
                            else
                            {
                                mainFile.cellArray[x, y].cellState = Cell.CellState.Grain;
                            }
                            x++;
                        }
                    }
                    mainFile.RecreateIntArray();
                    image.Source = DrawImage(mainFile.testArray);
                }

            }
        }
        private void bitmapImport_Click(object sender, RoutedEventArgs e)
        {
            if (uiNotBlockedFlag)
            {
                var openFileDialog = new OpenFileDialog();

                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;";

                openFileDialog.FilterIndex = 1;
                openFileDialog.Multiselect = false;
                bool? dialogBool = openFileDialog.ShowDialog();
                if (dialogBool != null && dialogBool == true && openFileDialog.CheckFileExists)
                {  
                    //if (openFileDialog.ShowDialog().ToString().Equals("OK"))
                    
                    System.Drawing.Bitmap img = new System.Drawing.Bitmap(openFileDialog.FileName);
                    for (int x = 0; x < img.Width; x++)
                    {
                        for (int y = 0; y < img.Height;y++)
                        {
                            System.Drawing.Color pixel = img.GetPixel(x, y);

                            mainFile.cellArray[x, y].cellId = x + y * mainFile.windowWidth;
                            mainFile.cellArray[x, y].cellColor = pixel;
                            if (mainFile.cellArray[x, y].cellColor == System.Drawing.Color.Black)
                            {
                                mainFile.cellArray[x, y].cellState = Cell.CellState.Inclusion;
                            }
                            else if (mainFile.cellArray[x, y].cellColor == System.Drawing.Color.White)
                            {
                                mainFile.cellArray[x, y].cellState = Cell.CellState.Empty;
                            }
                            else
                            {
                                mainFile.cellArray[x, y].cellState = Cell.CellState.Grain;
                            }
                        }
                    }
                    mainFile.RecreateIntArray();
                    image.Source = DrawImage(mainFile.testArray);
                }

            }
        }
        private void txtExport_Click(object sender, RoutedEventArgs e)
        {
            if (uiNotBlockedFlag)
            {
                StringBuilder fileContent = new StringBuilder();

                fileContent.AppendLine("Image width and height");
                fileContent.AppendLine(String.Format("{0} {1}", mainFile.windowWidth, mainFile.windowHeight));
                fileContent.AppendLine("ID R G B");

                for (int j = 0; j < mainFile.windowHeight; j++)
                {
                    for (int i = 0; i < mainFile.windowWidth; i++)
                    {
                    
                        string linContent = String.Format("{0} {1} {2} {3}", mainFile.cellArray[i, j].cellId, mainFile.cellArray[i, j].cellColor.R, mainFile.cellArray[i, j].cellColor.G, mainFile.cellArray[i, j].cellColor.B);
                        fileContent.AppendLine(linContent);
                    }
                }
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                bool? dialogBool = dialog.ShowDialog();
                if (dialogBool != null && dialogBool == true && dialog.CheckPathExists)
                {
                    System.IO.File.WriteAllText(dialog.FileName, fileContent.ToString());
                }
            }
        }
        private void bitmapExport_Click(object sender, RoutedEventArgs e)
        {
            if (uiNotBlockedFlag)
            {
                SaveBitmapInFile();
            }
        }

        private void widthTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (uiNotBlockedFlag)
            {
                if (Int32.TryParse(widthTextBox.Text, out int parseResult))
                {
                    if (parseResult > 500)
                        parseResult = 500; //max value 500
                    if (parseResult < 100)
                        parseResult = 100;
                    mainFile.windowWidth = parseResult;
                }
                else
                    mainFile.windowWidth = 300;
                PaintSurface.Width = mainFile.windowWidth;
                CreateNewMainFileInstanceDefaultValues();

            }
        }
        private void inclusionNumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (uiNotBlockedFlag)
            {
                if (Int32.TryParse(inclusionNumberTextBox.Text, out int parseResult))
                {
                    if (parseResult > 10)
                        parseResult = 10; //max value 10
                    if (parseResult < 1)
                        parseResult = 1;
                    mainFile.inclusionNumber = parseResult;
                }
                else
                    mainFile.inclusionNumber = 3;

                CreateNewMainFileInstanceDefaultValues();
            }
        }
        private void heightTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (uiNotBlockedFlag)
            {
                if (Int32.TryParse(heightTextBox.Text, out int parseResult))
                {
                    if (parseResult > 500)
                        parseResult = 500; //max value 500
                    if (parseResult < 100)
                        parseResult = 100;
                    mainFile.windowHeight = parseResult;
                }
                else
                    mainFile.windowHeight = 300;
                PaintSurface.Height = mainFile.windowHeight;
                CreateNewMainFileInstanceDefaultValues();
            }
                
        }

        private BitmapSource DrawImage(Int32[,] pixels)
        {
            int resX = pixels.GetUpperBound(0) + 1;
            int resY = pixels.GetUpperBound(1) + 1;

            WriteableBitmap writableImg = new WriteableBitmap(resX, resY, 96, 96, PixelFormats.Bgr32, null);

            //lock the buffer
            writableImg.Lock();

            for (int i = 0; i < resX; i++)
            {
                for (int j = 0; j < resY; j++)
                {
                    IntPtr backbuffer = writableImg.BackBuffer;
                    //the buffer is a monodimensionnal array...
                    backbuffer += j * writableImg.BackBufferStride;
                    backbuffer += i * 4;
                    System.Runtime.InteropServices.Marshal.WriteInt32(backbuffer, pixels[i, j]);
                }
            }

            //specify the area to update
            writableImg.AddDirtyRect(new Int32Rect(0, 0, resX, resY));
            //release the buffer and show the image
            writableImg.Unlock();

            return writableImg;
        }
        private void StopMethod()
        {
            // CancellationTokenSource implements IDisposable.
            if(wtoken != null)
                using (wtoken)
                {
                    // Cancel.  This will cancel the task.
                    wtoken.Cancel();
                }

            // Set everything to null, since the references
            // are on the class level and keeping them around
            // is holding onto invalid state.
            //uiNotBlockedFlag = true;
            //Dispatcher.Invoke(new Action(() => {
            //    IsUIBlockedReadonly(uiNotBlockedFlag);

            //}), DispatcherPriority.ContextIdle);
            wtoken = null;
            task = null;
            mainFile.stopWorkFlowFlag = false;
        }

        private void SaveBitmapInFile()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;";
            bool? dialogBool = dialog.ShowDialog();

            if (dialogBool != null && dialogBool == true && dialog.CheckPathExists)
            {
                try
                {
                    RenderTargetBitmap rtb = new RenderTargetBitmap((int)image.RenderSize.Width,
                    (int)image.RenderSize.Height, 96d, 96d, PixelFormats.Default);
                    rtb.Render(image);

                    BitmapEncoder pngEncoder = new BmpBitmapEncoder();
                    pngEncoder.Frames.Add(BitmapFrame.Create(rtb));
                    using (Stream s = new MemoryStream())
                    {
                        pngEncoder.Save(s);
                        System.Drawing.Bitmap tempBitmap = new System.Drawing.Bitmap( s);
                        tempBitmap.Save(dialog.FileName, ImageFormat.Jpeg);
                    }
                    
                }catch(Exception ex)
                {

                }
            }
        }

        private void squareInclusionButton_Checked(object sender, RoutedEventArgs e)
        {
            if (uiNotBlockedFlag)
                mainFile.grainShape = MainFile.GrainShape.Square;
        }

        private void CircleInclusionButton_Checked(object sender, RoutedEventArgs e)
        {
            if (uiNotBlockedFlag)
                mainFile.grainShape = MainFile.GrainShape.Round;
        }

        private void heterogenousButton_Checked(object sender, RoutedEventArgs e)
        {
            if (uiNotBlockedFlag)
                mainFile.energyType = MainFile.EnergyType.heterogenous;
        }

        private void homogenousButton_Checked(object sender, RoutedEventArgs e)
        {
            if (uiNotBlockedFlag)
                mainFile.energyType = MainFile.EnergyType.homogenous;
        }

        private void inclusionAfterEndButton_Click(object sender, RoutedEventArgs e)
        {
            mainFile.FindBorderCells();

            int placedInclusionNumber = 0;

            while (placedInclusionNumber < mainFile.inclusionNumber)
            {
                Random rand = new Random();
                int i = rand.Next(0, mainFile.windowWidth - 1);
                int j = rand.Next(0, mainFile.windowHeight - 1);
                if (mainFile.cellArray[i, j].isOnBorder && mainFile.cellArray[i, j].cellState != Cell.CellState.Inclusion)
                {
                    //currentPoint = e.GetPosition(PaintSurface);

                    Rectangle rec = new Rectangle();
                    Canvas.SetTop(rec, j);
                    Canvas.SetLeft(rec, i);
                    rec.Width = mainFile.inclusionDiameter;
                    rec.Height = mainFile.inclusionDiameter;
                    rec.Fill = new SolidColorBrush(Colors.Black);
                    PaintSurface.Children.Add(rec);

                    //set it to not reset this cell
                    mainFile.cellArray[i, j].cellState = Cell.CellState.Inclusion;

                    mainFile.cellArray[i, j].cellColor = System.Drawing.Color.Black;// Color.FromRgb(0,0,0);
                                                                                    //and add status for that
                    mainFile.cellArray[i, j].cellId = i + j * mainFile.windowWidth;
                    //paint inclusion
                    placedInclusionNumber++;
                }
            }
            
        }
    }
}
