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
        private EnergyType energyType { get; set; }
        public GrainShape grainShape { get; set; }
        public bool startFlag { get; set; }
        public bool stopFlag { get; set; }

        public Cell[,] cellArray;
       // private Cell[,] cellArrayNextStep;
        //private List<Color> colorList;
        public Int32[,] testArray; //= new Int32[,];

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
            Circle,
            Square
        }

        public MainFile()
        {
            //init evverything needed
            InitializeElements();
        }


        public Int32 GetColor(byte r, byte g, byte b)
        {
            return BitConverter.ToInt32(new byte[] { b, g, r, 0x00 }, 0);
        }

        private void InitializeElements()
        {
            grainNumber = 5;
            windowHeight = 300;
            windowWidth = 300;
            borderWidthPx = 5;
            startFlag = false;
            cellArray = new Cell[windowWidth, windowHeight];
           // cellArrayNextStep = new Cell[windowWidth, windowHeight];
            boundaryCondition = BoundaryCondition.absorbing;

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
                    if (startFlag && cellArray[i, j].cellState == Cell.CellState.Inclusion)
                        continue;
                    cellArray[i, j] = new Cell();
                    cellArray[i, j].cellId = i + j * windowWidth;
                }
            }

            ///obsolete
            //for (int i = 0; i < windowWidth; i++)
            //{
            //    for (int j = 0; j < windowHeight; j++)
            //    {
            //        if (startFlag && cellArrayNextStep[i, j].cellState == Cell.CellState.Inclusion)
            //            continue;
            //        cellArrayNextStep[i, j] = new Cell();
            //        cellArrayNextStep[i, j].cellId = i + j * windowWidth;
            //    }
            //}
            ///
            neighbourhoodType = NeighbourhoodType.vonNeuman;
            energyType = EnergyType.heterogenous;
            grainShape = GrainShape.Circle;
        }

        public Color getCellIdColorFromDictionary(int cellColorId)
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

                        //cellArrayNextStep[x, y].cellColorId = i;
                        //cellArrayNextStep[x, y].cellColor = getCellIdColorFromDictionary(i);
                        //cellArrayNextStep[x, y].cellState = Cell.CellState.Grain;
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
                stopFlag = true;
            }
        }

        public void AppWorkflow()
        {
            int step = 0;
            if (startFlag == false)
                generateGrains();
          //  RewriteArray();
            //thread tutaj 
            while (step < 10)
            {

            //CREATE TWO TABLES AND REWRITE VALUES FROM ONE TO ANOTHER AND THEN IT WILL WORK BUT NOW THEY ARE THE SAME
                FindNeighbour();
                CleanLastColors();
                step++;
            }

           // RewriteNextStepArray();

            RecreateIntArray();

            //5th border

            //next to think reset stop export import + GUI
        }

        public void CleanLastColors()
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

                    //choose neighbour with bigger count in range
                    //to coś popsulo
                    cellArray[x, y].cellState = Cell.CellState.Grain;
                    cellArray[x, y].isNotGrown = false;
                    cellArray[x, y].cellColorId = neightbourCount.GroupBy(a => a).OrderByDescending(b => b.Count()).First().Key;
                    cellArray[x, y].cellColor = getCellIdColorFromDictionary(cellArray[x, y].cellColorId);
                }
            }

        }

        public void Export(string type)
        {

            //csv/text
            for (int i = 0; i < windowWidth; i++)
            {
                for (int j = 0; j < windowHeight; j++)
                {

                }
            }


            //bitmap

        }

        public void Import(string type)
        {
            //bitmap

            //csv/ txt
        }

    }
}
