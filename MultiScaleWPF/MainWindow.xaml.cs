using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MultiScaleWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Point currentPoint = new Point();
        MainFile mainFile;

        public MainWindow()
        {
            mainFile = new MainFile();
            InitializeComponent();
            PaintSurface.Width = mainFile.windowWidth;
            PaintSurface.Height = mainFile.windowHeight;
            
        }

        //public void test()
        //{
        //    try
        //    {
        //        RenderTargetBitmap renderBitmap =
        //        new RenderTargetBitmap(
        //        (int)PaintSurface.ActualWidth,
        //        (int)PaintSurface.ActualHeight,
        //        96d,
        //        96d,
        //        PixelFormats.Pbgra32);

        //        DrawingVisual drawingVisual = new DrawingVisual();
        //        using (DrawingContext drawingContext = drawingVisual.RenderOpen())
        //        {
        //            drawingContext.DrawRectangle(canvBlade1Image.Background, null, new Rect(0, 0, canvBlade1Image.ActualWidth, canvBlade1Image.ActualHeight));


        //        }
        //        renderBitmap.Render(drawingVisual);
        //        renderBitmap.Render(canvBlade1Image);
        //        using (FileStream outStream = new FileStream(@"C:\Images\Keep\Blade1test.png.", FileMode.Create))
        //        {
        //            PngBitmapEncoder encoder = new PngBitmapEncoder();
        //            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
        //            encoder.Save(outStream);
        //        }
        //    }
        //    catch (Exception E)
        //    {
        //        MessageBox.Show("Error in rendering blade 1 image... " + E);
        //        return false;
        //    }
        //}

        private void Canvas_MouseDown_1(object sender, System.Windows.Input.MouseButtonEventArgs e)
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
                mainFile.cellArray[Convert.ToInt32( currentPoint.X), Convert.ToInt32( currentPoint.Y)].cellState = Cell.CellState.Inclusion;

                mainFile.cellArray[Convert.ToInt32(currentPoint.X), Convert.ToInt32(currentPoint.Y)].cellColor = System.Drawing.Color.Black;// Color.FromRgb(0,0,0);
                //and add status for that

            }
        }

        private void NeumanButton_Checked(object sender, RoutedEventArgs e)
        {
            //cant be running
            mainFile.neighbourhoodType = MainFile.NeighbourhoodType.vonNeuman;
        }
        private void MooreButton_Checked(object sender, RoutedEventArgs e)
        {
            //cant be running
            mainFile.neighbourhoodType = MainFile.NeighbourhoodType.Moore;
        }

        private void absorbingButton_Checked(object sender, RoutedEventArgs e)
        {
            mainFile.boundaryCondition = MainFile.BoundaryCondition.absorbing;
        }
        private void periodicButton_Checked(object sender, RoutedEventArgs e)
        {

            mainFile.boundaryCondition = MainFile.BoundaryCondition.periodic;
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            //if(!mainFile.startFlag)
              //  mainFile = new MainFile();
            
            mainFile.AppWorkflow();
            mainFile.startFlag = true;

            image.Source = DrawImage(mainFile.testArray);

        }
        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            mainFile.stopFlag = true;
        }
        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            mainFile = new MainFile();
            image.Source = DrawImage(mainFile.testArray);
        }

        private void numberGrainsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Int32.TryParse(numberGrainsTextBox.Text, out int parseResult))
                mainFile.grainNumber = parseResult;
            else
                mainFile.grainNumber = 5;
        }

        private void inclusionDiameterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Int32.TryParse(inclusionDiameterTextBox.Text, out int parseResult))
                mainFile.inclusionDiameter = parseResult;
            else
                mainFile.inclusionDiameter = 5;
        }

        private void txtImport_Click(object sender, RoutedEventArgs e)
        {
            //cant be running
            mainFile.Import("txt");
        }
        private void bitmapImport_Click(object sender, RoutedEventArgs e)
        {
            //cant be running
            mainFile.Import("bitmap");
        }
        private void txtExport_Click(object sender, RoutedEventArgs e)
        {
            //cant be running
            mainFile.Export("txt");
        }
        private void bitmapExport_Click(object sender, RoutedEventArgs e)
        {
            //cant be running
            mainFile.Export("bitmap");
        }

        private void widthTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(mainFile.startFlag)
                PaintSurface.Width = mainFile.windowWidth;
        }

        private void heightTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //cant be running
            //for now like that
            if (mainFile.startFlag)
                PaintSurface.Height = mainFile.windowHeight;
        }

        public BitmapSource DrawImage(Int32[,] pixels)
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
    }
}
