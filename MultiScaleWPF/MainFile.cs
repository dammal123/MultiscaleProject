using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiScaleWPF
{
    class MainFile
    {
        public int grainNumber { get; set; }
        public int inclusionDiameter { get; set; }
        public int windowHeight { get; set; }
        public int windowWidth { get; set; }
        private int borderWidthPx { get; set; }
        public BoundaryCondition boundaryCondition { get; set; }
        public NeighbourhoodType neighbourhoodType { get; set; }
        public EnergyType energyType { get; set; }
        public GrainShape grainShape { get; set; }
        public bool blockedConfiguration { get; set; }
        public bool stopWorkFlowFlag { get; set; }

        //MainWindow mainwindow;
        public Cell[,] cellArray;
        public Int32[,] testArray;

        private Dictionary<int,Color> colorListDict;
        public enum NeighbourhoodType
        {
            vonNeuman,
            Moore
        }
        public enum BoundaryCondition
        {
            absorbing,
            periodic
        }
        public enum EnergyType
        {
            homogenous,
            heterogenous
        }
        public enum GrainShape
        {
            Round,
            Square
        }

        public MainFile()
        {
            InitializeElements();
        }


        private Int32 GetColor(byte r, byte g, byte b)
        {
            return BitConverter.ToInt32(new byte[] { b, g, r, 0x00 }, 0);
        }

        //private int GetUiValue(string fieldName)
        //{
        //    int value;
        //    switch (fieldName)
        //    {
        //        case "widthTextBox":
        //            TryParseText(mainwindow.widthTextBox.Text,300);
        //            break;
        //        case "heightTextBox":
        //            TryParseText(mainwindow.heightTextBox.Text,300);
        //            break;
        //        case "numberGrainsTextBox":
        //            TryParseText(mainwindow.numberGrainsTextBox.Text, 5);
        //            break;
        //        case "inclusionDiameterTextBox":
        //            TryParseText(mainwindow.inclusionDiameterTextBox.Text, 2);
        //            break;
        //    }
        //    if (Int32.TryParse("", out value))
        //        return 1;
        //    return value;
        //}

        private int TryParseText(string text,int defaultValue)
        {
            if (Int32.TryParse(text, out int returnedValue))
                return returnedValue;
            else
                return defaultValue;
        }
        private void InitializeElements()
        {
            //if (mainwindow.firstSetUpDone) { pobierac wartosci jakos z ui
            grainNumber = 5;//GetUiValue("numberGrainsTextBox");
            windowHeight = 300;//GetUiValue("heightTextBox");
            windowWidth = 300;// GetUiValue("widthTextBox");
            borderWidthPx = 5;
            blockedConfiguration = false;
            cellArray = new Cell[windowWidth, windowHeight];

           // if(mainwindow.absorbingButton.IsChecked.HasValue && mainwindow.absorbingButton.IsChecked ==true)
                boundaryCondition = BoundaryCondition.absorbing;
           // else
           //     boundaryCondition = BoundaryCondition.periodic;

           // if (mainwindow.NeumanButton.IsChecked.HasValue && mainwindow.NeumanButton.IsChecked == true)
                neighbourhoodType = NeighbourhoodType.vonNeuman;
           // else
           //     neighbourhoodType = NeighbourhoodType.Moore;

           // if (mainwindow.heterogenousButton.IsChecked.HasValue && mainwindow.heterogenousButton.IsChecked == true)
                energyType = EnergyType.heterogenous;
          //  else
          //      energyType = EnergyType.homogenous;

           // if (mainwindow.squareInclusionButton.IsChecked.HasValue && mainwindow.squareInclusionButton.IsChecked == true)
                grainShape = GrainShape.Square;
          //  else
          //      grainShape = GrainShape.Round;

            testArray = new Int32[windowWidth, windowHeight];
            for (int i = 0; i < windowWidth; i++)
            {
                for (int j = 0; j < windowHeight; j++)
                {
                    testArray[i, j] = GetColor(255, 255,255);
                }
            }

            colorListDict = new Dictionary<int, Color>();

            for (int i = 0; i < windowWidth; i++)
            {
                for (int j = 0; j < windowHeight; j++)
                {
                    if (blockedConfiguration && cellArray[i, j].cellState == Cell.CellState.Inclusion)
                        continue;
                    cellArray[i, j] = new Cell();
                    cellArray[i, j].cellId = i + j * windowWidth;
                }
            }
        }

        private Color getCellIdColorFromDictionary(int cellColorId)
        {
            Color returnColorValue;

            if (colorListDict.TryGetValue(cellColorId, out returnColorValue))
                return returnColorValue;
            else
                return Color.White;
        }

        private void generateGrains()
        {
            for (int i = 1; i <= grainNumber; i++)
            {
                Random r = new Random();
                //maybe better option?
                while (true)
                {
                    int x = r.Next(windowWidth - 1);
                    int y = r.Next(windowHeight - 1);
                    if (cellArray[x, y].cellState == Cell.CellState.Empty)
                    {
                        Color grainColor;
                        do
                        {
                            grainColor = cellArray[x, y].setCellColorRandom();
                        } while (colorListDict.ContainsValue(grainColor));

                        cellArray[x, y].cellColorId = i;
                        colorListDict.Add(i, grainColor);
                        cellArray[x, y].cellColor = getCellIdColorFromDictionary(i);
                        cellArray[x, y].cellState = Cell.CellState.Grain;
                        break;
                    }
                }
            }
        }

        public void RecreateIntArray()
        {
            int testnum = 0;   
            testArray = new Int32[windowWidth,windowHeight];
            for(int i =0; i<windowWidth;i++)
            {
                for (int j = 0; j < windowHeight; j++)
                {

                    if (cellArray[i, j].cellState == Cell.CellState.Empty)
                        testArray[i, j] = GetColor(255, 255, 255);

                    if (cellArray[i, j].cellState == Cell.CellState.Grain || cellArray[i, j].cellState == Cell.CellState.Inclusion)
                    {
                        testnum++;
                        testArray[i, j] = GetColor(cellArray[i, j].cellColor.R, cellArray[i, j].cellColor.G, cellArray[i, j].cellColor.B);
                    }
                }
            }
            if(testnum == windowWidth*windowHeight)
            {
                stopWorkFlowFlag = true;
            }
        }

        public void AppWorkflow()
        {
            if (blockedConfiguration == false)
                generateGrains();

            
            FindNeighbour();
            CleanLastColors();
            RecreateIntArray();

            //5th border

            //next to think reset stop export import + GUI
        }

        private void CleanLastColors()
        {
            for(int i =0; i<windowWidth;i++)
            {
                for(int j =0; j<windowHeight;j++)
                {
                    cellArray[i, j].isNotGrown = true;
                }
            }
        }

        private void FindNeighbour()
        {
            for (int x = 0; x < windowWidth; x++)
            {
                for (int y = 0; y < windowHeight; y++)
                {
                    if (cellArray[x, y].cellState == Cell.CellState.Grain || cellArray[x, y].cellState == Cell.CellState.Inclusion)
                    {
                        continue;
                    }

                    List<int> neightbourCount = new List<int>();

                    if (neighbourhoodType == NeighbourhoodType.vonNeuman)
                    {
                        if(x > 0)
                        {
                            if (cellArray[x - 1, y].cellState == Cell.CellState.Grain && cellArray[x - 1, y].isNotGrown)
                            {
                                neightbourCount.Add(cellArray[x - 1, y].cellColorId);
                            }
                        }
                        if (y > 0)
                        {
                            
                            if (cellArray[x, y - 1].cellState == Cell.CellState.Grain && cellArray[x , y - 1].isNotGrown)
                            {
                                neightbourCount.Add(cellArray[x, y - 1].cellColorId);
                            }
                        }

                        if (y < windowHeight - 1 )
                        {
                            if (cellArray[x, y + 1].cellState == Cell.CellState.Grain && cellArray[x, y + 1].isNotGrown)
                            {
                                neightbourCount.Add(cellArray[x, y + 1].cellColorId);
                            }
                        }
                        if (x < windowWidth - 1)
                        {
                            if (cellArray[x + 1, y].cellState == Cell.CellState.Grain && cellArray[x + 1, y].isNotGrown)
                            {
                                neightbourCount.Add(cellArray[x + 1, y].cellColorId);
                            }
                        }

                    }
                    else // NeighbourhoodType.Moore
                    {
                        if (x > 0)
                        {
                            if (cellArray[x - 1, y].cellState == Cell.CellState.Grain && cellArray[x - 1, y].isNotGrown)
                            {
                                neightbourCount.Add(cellArray[x - 1, y].cellColorId);
                            }

                            if (y < windowHeight - 1)
                            {
                                if (cellArray[x - 1, y + 1].cellState == Cell.CellState.Grain && cellArray[x - 1, y + 1].isNotGrown)
                                {
                                    neightbourCount.Add(cellArray[x - 1, y + 1].cellColorId);
                                }
                            }

                            if (y > 0)
                            {
                                if (cellArray[x - 1, y - 1].cellState == Cell.CellState.Grain && cellArray[x - 1, y - 1].isNotGrown)
                                {
                                    neightbourCount.Add(cellArray[x - 1, y - 1].cellColorId);
                                }
                            }

                        }
                        if (y > 0)
                        {

                            if (cellArray[x, y - 1].cellState == Cell.CellState.Grain && cellArray[x, y - 1].isNotGrown)
                            {
                                neightbourCount.Add(cellArray[x, y - 1].cellColorId);
                            }

                            if (x > windowWidth - 1)
                            {
                                if (cellArray[x + 1, y - 1].cellState == Cell.CellState.Grain && cellArray[x + 1, y - 1].isNotGrown)
                                {
                                    neightbourCount.Add(cellArray[x + 1, y - 1].cellColorId);
                                }
                            }
                        }

                        if (y < windowHeight - 1)
                        {
                            if (cellArray[x, y + 1].cellState == Cell.CellState.Grain && cellArray[x, y + 1].isNotGrown)
                            {
                                neightbourCount.Add(cellArray[x, y + 1].cellColorId);
                            }

                            if(x < windowWidth - 1)
                            {
                                if (cellArray[x + 1, y + 1].cellState == Cell.CellState.Grain && cellArray[x + 1, y + 1].isNotGrown)
                                {
                                    neightbourCount.Add(cellArray[x + 1 , y + 1].cellColorId);
                                }
                            }
                        }

                        if (x < windowWidth - 1)
                        {
                            if (cellArray[x + 1, y].cellState == Cell.CellState.Grain && cellArray[x + 1, y].isNotGrown)
                            {
                                neightbourCount.Add(cellArray[x + 1, y].cellColorId);
                            }
                        }
                    }

                    if (neightbourCount.Count == 0)
                        continue;

                    cellArray[x, y].cellState = Cell.CellState.Grain;
                    cellArray[x, y].isNotGrown = false;
                    cellArray[x, y].cellColorId = neightbourCount.GroupBy(a => a).OrderByDescending(b => b.Count()).First().Key;
                    cellArray[x, y].cellColor = getCellIdColorFromDictionary(cellArray[x, y].cellColorId);
                }
            }

        }
    }
}
