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
        private bool uiNotBlockedFlag = false;
        private bool workFlowEnded = false;
        private bool cleanSubstructures = false;
        private bool borderRemoveFlag = true;

        public MainWindow()
        {
            InitializeComponent();// tworzy wszystko potrzebne zablokowanie textchanged -> uzyto uiNotBlockedFlag
            mainFile = new MainFile(); // nowa istancja caly czas uzywana
            InitializeVariablesForMainFile(); //init potrzebnych zmiennych dla mainfile pobierane z okna defaulatowe wartosci
            PaintSurface.Width = mainFile.windowWidth;
            PaintSurface.Height = mainFile.windowHeight;
            uiNotBlockedFlag = true; // pozwalamy na UI operacje
        }

        private void Canvas_MouseDown_1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (uiNotBlockedFlag && !workFlowEnded)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    currentPoint = e.GetPosition(PaintSurface);

                    int x = Convert.ToInt32(currentPoint.X);
                    int y = Convert.ToInt32(currentPoint.Y);

                    if (squareInclusionButton != null && squareInclusionButton.IsChecked == true)
                    {
                        SquareInclusionsBefore(x,y,true);
                    }
                    else
                    {
                        RoundInclusionBefore(x,y);
                    }

                    mainFile.RecreateIntArray();
                    image.Source = DrawImage(mainFile.testArray);
                }
            }
            else if(uiNotBlockedFlag && workFlowEnded)
            {
                Substructure_Click(sender, e);
            }
        }

        private void Substructure_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            
            currentPoint = e.GetPosition(PaintSurface);
            
            int x = Convert.ToInt32(currentPoint.X);
            int y = Convert.ToInt32(currentPoint.Y);

            int dualPhaseNumberId = 100;
            int substructureNumberId = 101;
            
            if (x < 0 || y < 0 || x >= mainFile.windowWidth || y >= mainFile.windowHeight)
                return;
            
            if (mainFile.cellArray[x, y].cellState == Enums.CellState.Inclusion)
                return;
            
            int numberOfColorId  = mainFile.GetCellColorIdFromDictionary(mainFile.cellArray[x, y].cellColor);
            
            for (int i = 0; i <= mainFile.windowWidth -1; i++)
            {
                for (int j = 0; j <= mainFile.windowHeight -1; j++)
                {
                    if (mainFile.cellArray[i, j].cellColorId == numberOfColorId)
                    {
                        if (mainFile.operationType == Enums.OperationType.substructure)
                            mainFile.cellArray[i, j].cellState = Enums.CellState.Substructure;
                        else
                        {
                            mainFile.cellArray[i, j].cellState = Enums.CellState.DualPhase;
                            mainFile.cellArray[i, j].cellColor = System.Drawing.Color.FromArgb(255, 105, 180);
            
                        }
                    }
                    if(mainFile.cellArray[i, j].cellState == Enums.CellState.DualPhase)
                    {
                        mainFile.cellArray[i, j].cellColorId = dualPhaseNumberId; //dualphase cellcolorid
                        if(!mainFile.colorListDict.ContainsKey(dualPhaseNumberId))
                            mainFile.colorListDict.Add(dualPhaseNumberId, System.Drawing.Color.FromArgb(255, 105, 180));
                    }
                }
            }
            mainFile.colorListDict = new Dictionary<int, System.Drawing.Color>();
            mainFile.colorListDict.Add(substructureNumberId, mainFile.cellArray[x, y].cellColor);

            InitializeVariablesForMainFile();
            mainFile.RecreateIntArray();

            mainFile.stopWorkFlowFlag = false; uiNotBlockedFlag = true; workFlowEnded = false; mainFile.dontGenerateGrainsFlag = false;
        }

        private void SquareInclusionsBefore(int x,int y, bool onClick)
        {
            for (int num = 0; num < mainFile.inclusionDiameter; num++)
            {
                if (mainFile.inclusionDiameter == 1)
                {
                    PaintPixelAddToCellArray(x, y);
                }
                else
                {
                    if (onClick)
                    {
                        int halfOfInclusionDiameter = mainFile.inclusionDiameter / 2;
                        for (int i = Convert.ToInt32(currentPoint.X) - halfOfInclusionDiameter; i < Convert.ToInt32(currentPoint.X) + halfOfInclusionDiameter; i++)
                        {
                            for (int j = Convert.ToInt32(currentPoint.Y) - halfOfInclusionDiameter; j < Convert.ToInt32(currentPoint.Y) + halfOfInclusionDiameter; j++)
                            {
                                PaintPixelAddToCellArray(i, j);
                            }
                        }
                    }
                    else
                    {
                        int halfOfInclusionDiameter = mainFile.inclusionDiameter / 2;
                        for (int i = x - halfOfInclusionDiameter; i < x + halfOfInclusionDiameter; i++)
                        {
                            for (int j = y - halfOfInclusionDiameter; j < y + halfOfInclusionDiameter; j++)
                            {
                                PaintPixelAddToCellArray(i, j);
                            }
                        }
                    }
                }
            }
        }

        private void RoundInclusionBefore(int x, int y)
        {
            int halfOfInclusionDiameter = mainFile.inclusionDiameter / 2;
            const double PI = 3.1415926535;
            double x1, y1;
            for(int r = halfOfInclusionDiameter; r > 0; r--)
            {
                for (double angle = 0; angle < 360; angle += 0.1)
                {
                    x1 = r * Math.Cos(angle * PI / 180);

                    y1 = r * Math.Sin(angle * PI / 180);

                    PaintPixelAddToCellArray(x + (int)x1,y +(int)y1);
                }
            }
        }

        private void PaintPixelAddToCellArray( int x, int y)
        {
            if (x < 0 || y < 0 || x >= mainFile.windowWidth || y >= mainFile.windowHeight)
                return;

            if (mainFile.cellArray[x, y].cellState == Enums.CellState.Inclusion  )
                return;

            mainFile.cellArray[x, y].cellState = Enums.CellState.Inclusion;

            mainFile.cellArray[x, y].cellColor = System.Drawing.Color.Black;

            mainFile.cellArray[x, y].cellColorId = 102;
            if(mainFile.colorListDict.ContainsKey(mainFile.cellArray[x, y].cellColorId) == false)
                mainFile.colorListDict.Add(mainFile.cellArray[x, y].cellColorId, System.Drawing.Color.Black);

            mainFile.cellArray[x, y].cellId = x  + y * mainFile.windowWidth;
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            wtoken = new CancellationTokenSource();

            cleanSubstructures = false;

            task = Task.Run(async () =>  // <- marked async
            {
                while (true)
                {
                    mainFile.AppWorkflow();
                    mainFile.dontGenerateGrainsFlag = true;
                    uiNotBlockedFlag = false;


                    Dispatcher.Invoke(new Action(() => {

                        image.Source = DrawImage(mainFile.testArray);

                    }), DispatcherPriority.ContextIdle);

                    if (mainFile.stopWorkFlowFlag)
                    {
                        workFlowEnded = true;

                        uiNotBlockedFlag = true;
                        StopMethod();

                        Dispatcher.Invoke(new Action(() => {

                            inclusionAfterEndButton.IsEnabled = true;

                        }), DispatcherPriority.ContextIdle);

                        Dispatcher.Invoke(new Action(() => {

                            addBorderButton.IsEnabled = true;

                        }), DispatcherPriority.ContextIdle);
                    }
                    await Task.Delay(100, wtoken.Token); // <- await with cancellation
                }
            }, wtoken.Token);
        }

        private BitmapSource DrawImage(Int32[,] pixels)
        {
            int resX = pixels.GetUpperBound(0) + 1;
            int resY = pixels.GetUpperBound(1) + 1;

            WriteableBitmap writableImg = new WriteableBitmap(resX, resY, 96, 96, PixelFormats.Bgr32, null);

            writableImg.Lock();

            for (int i = 0; i < resX; i++)
            {
                for (int j = 0; j < resY; j++)
                {
                    IntPtr backbuffer = writableImg.BackBuffer;

                    backbuffer += j * writableImg.BackBufferStride;
                    backbuffer += i * 4;
                    System.Runtime.InteropServices.Marshal.WriteInt32(backbuffer, pixels[i, j]);
                }
            }

            writableImg.AddDirtyRect(new Int32Rect(0, 0, resX, resY));

            writableImg.Unlock();

            return writableImg;
        }
        private void StopMethod()
        {
            if (wtoken != null)
                using (wtoken)
                {
                    wtoken.Cancel();
                }

            uiNotBlockedFlag = true;

            wtoken = null;
            task = null;
            mainFile.stopWorkFlowFlag = false;
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            StopMethod();
        }
        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            StopMethod();
            cleanSubstructures = true;
            uiNotBlockedFlag = true;

            mainFile.stopWorkFlowFlag = false;

            InitializeVariablesForMainFile();
            mainFile.RecreateIntArray();
            workFlowEnded = false;
            image.Source = DrawImage(mainFile.testArray);
        }

        private void AddBorder()
        {
            
            int borderSteps = mainFile.borderWidthPx - 3;
            
            do
            {
                mainFile.FindBorderCells();
                for (int i = 0; i < mainFile.windowWidth; i++)
                {
                    for (int j = 0; j < mainFile.windowHeight; j++)
                    {
                        if (mainFile.cellArray[i, j].isOnBorder)
                        {
                            mainFile.cellArray[i, j].cellState = Enums.CellState.Border;

                            mainFile.cellArray[i, j].cellColor = System.Drawing.Color.Black;

                            mainFile.cellArray[i, j].cellColorId = 103;
                            if(mainFile.colorListDict.ContainsKey(mainFile.cellArray[i, j].cellColorId) == false)
                                mainFile.colorListDict.Add(mainFile.cellArray[i, j].cellColorId, System.Drawing.Color.Black);

                            mainFile.cellArray[i, j].isOnBorder = true;
                        }
                    }
                }

                borderSteps--;
            } while (borderSteps > 0);

            mainFile.RecreateIntArray();
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
                case "propabilityTextBox":
                    value = TryParseText(propabilityTextBox.Text, 90);
                    break;
                case "borderWidth":
                    value = TryParseText(borderWidth.Text, 3);
                    break;
            }
            return value;
        }

        private void InitializeVariablesForMainFile()
        {

            GetVariablesValues();

            for (int i = 0; i < mainFile.windowWidth; i++)
            {
                for (int j = 0; j < mainFile.windowHeight; j++)
                {
                    mainFile.testArray[i, j] = mainFile.GetColor(255, 255, 255);
                }
            }
           
            for (int i = 0; i < mainFile.windowWidth; i++)
            {
                for (int j = 0; j < mainFile.windowHeight; j++)
                {

                    if (mainFile.dontGenerateGrainsFlag && mainFile.cellArray[i, j].cellState == Enums.CellState.Inclusion)
                        continue;

                    if(workFlowEnded && cleanSubstructures == false && (mainFile.cellArray[i, j].cellState == Enums.CellState.Substructure || mainFile.cellArray[i, j].cellState == Enums.CellState.DualPhase))
                        continue;

                    if (workFlowEnded && borderRemoveFlag == false && mainFile.cellArray[i, j].cellState == Enums.CellState.Border)
                        continue;

                    mainFile.cellArray[i, j] = new Cell();
                    mainFile.cellArray[i, j].cellId = i + j * mainFile.windowWidth;
                }
            }
        }
        
        private void GetVariablesValues()
        {
            mainFile.grainNumber = GetUiValue("numberGrainsTextBox");
            mainFile.windowHeight = GetUiValue("heightTextBox");
            mainFile.windowWidth = GetUiValue("widthTextBox");
            mainFile.inclusionDiameter = GetUiValue("inclusionDiameterTextBox");
            mainFile.inclusionNumber = GetUiValue("inclusionNumberTextBox");
            mainFile.borderWidthPx = GetUiValue("borderWidth"); 

            mainFile.propabilityChanceToChange = GetUiValue("propabilityTextBox"); ;//90% na zmiane
            mainFile.dontGenerateGrainsFlag = false;

            if (workFlowEnded == false)
            {
                mainFile.cellArray = new Cell[mainFile.windowWidth, mainFile.windowHeight];
                mainFile.testArray = new Int32[mainFile.windowWidth, mainFile.windowHeight];

                mainFile.colorListDict = new Dictionary<int, System.Drawing.Color>();
            }

            if (NeumanButton.IsChecked.HasValue && NeumanButton.IsChecked == true)
                mainFile.neighbourhoodType = Enums.NeighbourhoodType.vonNeuman;
            else if (PropabilityButton.IsChecked.HasValue && PropabilityButton.IsChecked == true)
                mainFile.neighbourhoodType = Enums.NeighbourhoodType.Propability;
            else
                mainFile.neighbourhoodType = Enums.NeighbourhoodType.Moore;

            if (substructureButton.IsChecked.HasValue && substructureButton.IsChecked == true)
                mainFile.operationType = Enums.OperationType.substructure;
            else
                mainFile.operationType = Enums.OperationType.dualPhase;

            if (squareInclusionButton.IsChecked.HasValue && squareInclusionButton.IsChecked == true)
                mainFile.inclusionShape = Enums.InclusionShape.Square;
            else
                mainFile.inclusionShape = Enums.InclusionShape.Round;
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

            squareInclusionButton.IsEnabled = boolSwitchValue;
            CircleInclusionButton.IsEnabled = boolSwitchValue;
        }

        private void NeumanButton_Checked(object sender, RoutedEventArgs e)
        {
            if (uiNotBlockedFlag)
                mainFile.neighbourhoodType = Enums.NeighbourhoodType.vonNeuman;
        }
        private void MooreButton_Checked(object sender, RoutedEventArgs e)
        {
            if (uiNotBlockedFlag)
                mainFile.neighbourhoodType = Enums.NeighbourhoodType.Moore;
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
                    mainFile.grainNumber = parseResult;
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
                workFlowEnded = true;
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
                                mainFile.cellArray[x, y].cellState = Enums.CellState.Inclusion;
                            }
                            else if(mainFile.cellArray[x, y].cellColor == System.Drawing.Color.White)
                            {
                                mainFile.cellArray[x, y].cellState = Enums.CellState.Empty;
                            }
                            else
                            {
                                mainFile.cellArray[x, y].cellState = Enums.CellState.Grain;
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
                workFlowEnded = true;
                var openFileDialog = new OpenFileDialog();

                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;";

                openFileDialog.FilterIndex = 1;
                openFileDialog.Multiselect = false;
                bool? dialogBool = openFileDialog.ShowDialog();
                if (dialogBool != null && dialogBool == true && openFileDialog.CheckFileExists)
                {  
                    
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
                                mainFile.cellArray[x, y].cellState = Enums.CellState.Inclusion;
                            }
                            else if (mainFile.cellArray[x, y].cellColor == System.Drawing.Color.White)
                            {
                                mainFile.cellArray[x, y].cellState = Enums.CellState.Empty;
                            }
                            else
                            {
                                mainFile.cellArray[x, y].cellState = Enums.CellState.Grain;
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

        private void color_details_Click(object sender, RoutedEventArgs e)
        {
            if (uiNotBlockedFlag)
            {
                StringBuilder fileContent = new StringBuilder();

                fileContent.AppendLine("Image width and height");
                fileContent.AppendLine(String.Format("{0} {1}", mainFile.windowWidth, mainFile.windowHeight));
                fileContent.AppendLine("ID - Size - %");

                foreach (int colorId in mainFile.colorListDict.Keys)
                {
                    
                    int pixelNumberForColor = 0;
                    for (int j = 0; j < mainFile.windowHeight; j++)
                    {
                        for (int i = 0; i < mainFile.windowWidth; i++)
                        {
                            if (mainFile.cellArray[i, j].cellColorId == colorId)
                                pixelNumberForColor++;
                        }
                    }

                    double colourPercentage = (double)pixelNumberForColor * 100/((double)mainFile.windowWidth*(double)mainFile.windowHeight);

                    string linContent = String.Format("{0} - {1} - {2}%", colorId, pixelNumberForColor, colourPercentage);
                    fileContent.AppendLine(linContent);
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
            }
                
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
                    
                }catch(Exception )
                {

                }
            }
        }

        private void squareInclusionButton_Checked(object sender, RoutedEventArgs e)
        {
            if (uiNotBlockedFlag)
                mainFile.inclusionShape = Enums.InclusionShape.Square;
        }

        private void CircleInclusionButton_Checked(object sender, RoutedEventArgs e)
        {
            if (uiNotBlockedFlag)
                mainFile.inclusionShape = Enums.InclusionShape.Round;
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
                if (mainFile.cellArray[i, j].isOnBorder && mainFile.cellArray[i, j].cellState != Enums.CellState.Inclusion)
                {
                    
                    if (squareInclusionButton != null && squareInclusionButton.IsChecked == true)
                    {
                        SquareInclusionsBefore(i,j,false);
                    }
                    else
                    {
                        RoundInclusionBefore(i,j);
                    }
                    
                    placedInclusionNumber++;
                }
            }

            mainFile.RecreateIntArray();
            image.Source = DrawImage(mainFile.testArray);
        }

        private void PropabilityButton_Checked(object sender, RoutedEventArgs e)
        {
            if (uiNotBlockedFlag)
                mainFile.neighbourhoodType = Enums.NeighbourhoodType.Propability;
        }

        private void propabilityTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (uiNotBlockedFlag)
            {
                if (Int32.TryParse(inclusionNumberTextBox.Text, out int parseResult))
                {
                    if (parseResult > 100)
                        parseResult = 90; 
                    if (parseResult < 10)
                        parseResult = 10;
                    mainFile.inclusionNumber = parseResult;
                }
                else
                    mainFile.inclusionNumber = 90;

            }
        }

        private void dualPhaseButton_Checked(object sender, RoutedEventArgs e)
        {
            if (uiNotBlockedFlag)
                mainFile.operationType = Enums.OperationType.dualPhase;
        }

        private void substructureButton_Checked(object sender, RoutedEventArgs e)
        {
            if (uiNotBlockedFlag)
                mainFile.operationType = Enums.OperationType.substructure;
        }

        private void borderWidth_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (uiNotBlockedFlag)
            {
                if (Int32.TryParse(borderWidth.Text, out int parseResult))
                {
                    if (parseResult > 10)
                        parseResult = 10;
                    if (parseResult < 3)
                        parseResult = 3;
                    mainFile.borderWidthPx = parseResult;
                }
                else
                    mainFile.borderWidthPx = 3;

            }
        }

        private void borderCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if (uiNotBlockedFlag)
            {
                    borderRemoveFlag = true;
            }
        }
        private void borderCheckboxUnchecked(object sender, RoutedEventArgs e)
        {
            if (uiNotBlockedFlag)
            {
                    borderRemoveFlag = false;
            }
        }
        private void addBorderButton_Click(object sender, RoutedEventArgs e)
        {
            AddBorder();
        }
    }
}
