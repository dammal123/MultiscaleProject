﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MultiScaleWPF.Enums;

namespace MultiScaleWPF
{
    public class Cell
    {
        public int cellId { get; set; }
        public int cellColorId { get; set; }
        public Color cellColor { get; set; }
        public bool isRecrystalized { get; set; }
        public int cellEnergy { get; set; }
        public CellState cellState { get; set; }
        public bool isNotGrown { get; set; }
        public bool isOnBorder { get; set; }
        
        public Cell()
        {
            this.cellColorId = 0;
            this.cellColor = setDefaultCellColor();
            this.isRecrystalized = false;
            this.cellEnergy = 0;
            this.cellState = CellState.Empty;
            this.isNotGrown = true;
            this.isOnBorder = false;
        }
        public Color setDefaultCellColor()//int cellId)
        {
            return Color.FromArgb(250, 250, 250);
        }
        public Color setCellColorRandom()
        {
            Random r = new Random();
            this.cellColor = Color.FromArgb(r.Next(249) + 1, r.Next(249) + 1, r.Next(249) + 1);
            while (this.cellColor == Color.FromArgb(255, 105, 180))//dualphase color
            {
                this.cellColor = Color.FromArgb(r.Next(249) + 1, r.Next(249) + 1, r.Next(249) + 1);
            }
            return this.cellColor;
        }
    }
}
