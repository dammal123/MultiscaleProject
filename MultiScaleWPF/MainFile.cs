using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MultiScaleWPF.Enums;

namespace MultiScaleWPF
{
    class MainFile
    {
        public int grainNumber { get; set; }
        public int inclusionDiameter { get; set; }
        public int inclusionNumber { get; set; }
        public int windowHeight { get; set; }
        public int windowWidth { get; set; }
        public int borderWidthPx { get; set; }//future development
        public NeighbourhoodType neighbourhoodType { get; set; }
        public OperationType operationType { get; set; }
        public InclusionShape inclusionShape { get; set; }
        public bool dontGenerateGrainsFlag { get; set; }
        public bool stopWorkFlowFlag { get; set; }

        public int propabilityChanceToChange { get; set; }

        
        public Cell[,] cellArray;
        public Int32[,] testArray;

        public Dictionary<int,Color> colorListDict;
        
        public Int32 GetColor(byte r, byte g, byte b)
        {
            return BitConverter.ToInt32(new byte[] { b, g, r, 0x00 }, 0);
        }

        private Color getColorFromDictionary(int cellColorId)
        {
            Color returnColorValue;

            if (colorListDict.TryGetValue(cellColorId, out returnColorValue))
                return returnColorValue;
            else
                return Color.White;
        }

        public int GetCellColorIdFromDictionary(Color color)
        {
            int cellColorId = -1;
            if(colorListDict.ContainsValue(color))
            {
                foreach(KeyValuePair<int,Color> pair in colorListDict)
                {
                    if (EqualityComparer<Color>.Default.Equals(pair.Value, color))
                    {
                        cellColorId = pair.Key;
                        break;
                    }
                }
            }
            return cellColorId;
        }
        
        private void generateGrains()
        {
            for (int i = 1; i <= grainNumber; i++)
            {
                Random r = new Random();

                while (true)
                {
                    int x = r.Next(windowWidth - 1);
                    int y = r.Next(windowHeight - 1);
                    if (cellArray[x, y].cellState == Enums.CellState.Empty)
                    {
                        Color grainColor;
                        do
                        {
                            grainColor = cellArray[x, y].setCellColorRandom();
                        } while (colorListDict.ContainsValue(grainColor));

                        cellArray[x, y].cellColorId = i;
                        colorListDict.Add(i, grainColor);
                        cellArray[x, y].cellColor = getColorFromDictionary(i);
                        cellArray[x, y].cellState = Enums.CellState.Grain;
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

                    if (cellArray[i, j].cellState == Enums.CellState.Empty)
                    {
                        testArray[i, j] = GetColor(255, 255, 255);
                    }
                    else
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
            if (dontGenerateGrainsFlag == false)
                generateGrains();

            
            FindNeighbour();
            CleanLastColors();
            RecreateIntArray();

            //5th border

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

        public void FindBorderCells()
        {
            for (int x = 0; x < windowWidth; x++)
            {
                for (int y = 0; y < windowHeight; y++)
                {

                    if (cellArray[x, y].cellState == Enums.CellState.Empty)
                        continue;

                    if(cellArray[x, y].cellState == Enums.CellState.Grain)
                    {
                        int borderNeightbourCount = 0;

                        if (x > 0)
                        {
                            if (cellArray[x - 1, y].cellState == Enums.CellState.Grain && cellArray[x - 1, y].cellColorId != cellArray[x, y].cellColorId)
                            {
                                borderNeightbourCount++;
                            }

                            if (y < windowHeight - 1)
                            {
                                if (cellArray[x - 1, y + 1].cellState == Enums.CellState.Grain && cellArray[x - 1, y + 1].cellColorId != cellArray[x, y].cellColorId)
                                {
                                    borderNeightbourCount++;
                                }
                            }

                            if (y > 0)
                            {
                                if (cellArray[x - 1, y - 1].cellState == Enums.CellState.Grain && cellArray[x - 1, y - 1].cellColorId != cellArray[x, y].cellColorId)
                                {
                                    borderNeightbourCount++;
                                }
                            }

                        }
                        if (y > 0)
                        {

                            if (cellArray[x, y - 1].cellState == Enums.CellState.Grain && cellArray[x, y - 1].cellColorId != cellArray[x, y].cellColorId)
                            {
                                borderNeightbourCount++;
                            }

                            if (x > windowWidth - 1)
                            {
                                if (cellArray[x + 1, y - 1].cellState == Enums.CellState.Grain && cellArray[x + 1, y - 1].cellColorId != cellArray[x, y].cellColorId)
                                {
                                    borderNeightbourCount++;
                                }
                            }
                        }

                        if (y < windowHeight - 1)
                        {
                            if (cellArray[x, y + 1].cellState == Enums.CellState.Grain && cellArray[x, y + 1].cellColorId != cellArray[x, y].cellColorId)
                            {
                                borderNeightbourCount++;
                            }

                            if (x < windowWidth - 1)
                            {
                                if (cellArray[x + 1, y + 1].cellState == Enums.CellState.Grain && cellArray[x + 1, y + 1].cellColorId != cellArray[x, y].cellColorId)
                                {
                                    borderNeightbourCount++;
                                }
                            }
                        }

                        if (x < windowWidth - 1)
                        {
                            if (cellArray[x + 1, y].cellState == Enums.CellState.Grain && cellArray[x + 1, y].cellColorId != cellArray[x, y].cellColorId)
                            {
                                borderNeightbourCount++;
                            }
                        }
                                

                        if (borderNeightbourCount == 0)
                            continue;

                        cellArray[x, y].isOnBorder = true;
                    }
                }
            }
        }

        private List<int> VonNeumanNeighbourIdList(int x, int y)
        {
            List<int> neightbourCount = new List<int>();

            if (x > 0)
            {
                AddNeighbour(x - 1, y, neightbourCount);
               
            }
            if (y > 0)
            {
                AddNeighbour(x, y - 1, neightbourCount);
               
            }

            if (y < windowHeight - 1)
            {
                AddNeighbour(x, y + 1, neightbourCount);
                
            }
            if (x < windowWidth - 1)
            {
                AddNeighbour(x + 1, y, neightbourCount);
                
            }
            return neightbourCount;
        }

        private List<int> MooreNeighbourIdList(int x,int y)
        {
            List<int> neightbourCount = new List<int>();

            if (x > 0)
            {
                AddNeighbour(x - 1, y, neightbourCount);
               
                if (y < windowHeight - 1)
                {
                    AddNeighbour(x - 1, y + 1, neightbourCount);
                }

                if (y > 0)
                {
                    AddNeighbour(x - 1, y - 1, neightbourCount);
                }
            }
            if (y > 0)
            {
                AddNeighbour(x, y - 1, neightbourCount);

                if (x > windowWidth - 1)
                {
                    AddNeighbour(x + 1, y - 1, neightbourCount);
                }
            }

            if (y < windowHeight - 1)
            {
                AddNeighbour(x , y + 1, neightbourCount);
               
                if (x < windowWidth - 1)
                {
                    AddNeighbour(x + 1, y + 1, neightbourCount);
                }
            }

            if (x < windowWidth - 1)
            {
                AddNeighbour(x + 1, y, neightbourCount);
            }

            return neightbourCount;
        }

        private void AddNeighbour(int x, int y, List<int> neighbourCount)
        {
            if (cellArray[x , y ].cellState == Enums.CellState.Grain && cellArray[x, y ].isNotGrown)
            {
                neighbourCount.Add(cellArray[x, y ].cellColorId);
            }
        }

        private List<int> FurtherMooreNeighbourIdList(int x, int y)
        {
            List<int> neightbourCount = new List<int>();

            if (x > 0)
            {
                if (y < windowHeight - 1)
                {
                    AddNeighbour(x -1, y +1, neightbourCount);
                }

                if (y > 0)
                {
                    AddNeighbour(x - 1, y - 1, neightbourCount);
                }
            }
            
            if (x < windowWidth - 1)
            {
                if (y < windowHeight - 1)
                {
                    AddNeighbour(x + 1, y + 1, neightbourCount);
                }

                if (y > 0)
                {
                    AddNeighbour(x + 1, y - 1, neightbourCount);
                }

            }

            return neightbourCount;
        }

        private List<int> PropabilityNeighbourhood(int x, int y)
        {
            List<int> neightbourCount = new List<int>();

            if (MooreNeighbourIdList(x, y).Count > 5)
            {
                return neightbourCount = MooreNeighbourIdList(x, y);
                
            }
            if (VonNeumanNeighbourIdList(x, y).Count > 3)
            {
                return neightbourCount = VonNeumanNeighbourIdList(x, y);

            }
            if(FurtherMooreNeighbourIdList(x,y).Count > 3)
            {
                return neightbourCount = FurtherMooreNeighbourIdList(x, y);
            }

            Random rand = new Random();

            if( rand.Next(1,100) <= propabilityChanceToChange)
            {
                return neightbourCount = MooreNeighbourIdList(x, y);
            }
            return neightbourCount;
        }

        private void FindNeighbour()
        { 
            for (int x = 0; x < windowWidth; x++)
            {
                for (int y = 0; y < windowHeight; y++)
                {
                    if (cellArray[x, y].cellState == Enums.CellState.Grain || cellArray[x, y].cellState == Enums.CellState.Inclusion || cellArray[x, y].cellState == Enums.CellState.Substructure || cellArray[x, y].cellState == Enums.CellState.DualPhase)
                    {
                        continue;
                    }

                    List<int> neightbourCount = new List<int>();

                    if (neighbourhoodType == NeighbourhoodType.vonNeuman)
                    {
                        neightbourCount = VonNeumanNeighbourIdList(x, y);

                    }
                    else if(neighbourhoodType == NeighbourhoodType.Moore)
                    {
                        neightbourCount = MooreNeighbourIdList(x,y);

                    }
                    else // NeighbourhoodType.Propability
                    {
                        // nie czyta wszystkich grainsow i zostaje tylko 5 jakims cudem
                        neightbourCount = PropabilityNeighbourhood(x, y);
                    }

                    if (neightbourCount.Count == 0)
                        continue;



                    cellArray[x, y].cellState = Enums.CellState.Grain;
                    cellArray[x, y].isNotGrown = false;
                    cellArray[x, y].cellColorId = neightbourCount.GroupBy(a => a).OrderByDescending(b => b.Count()).First().Key;
                    cellArray[x, y].cellColor = getColorFromDictionary(cellArray[x, y].cellColorId);
                }
            }

        }
    }
}
