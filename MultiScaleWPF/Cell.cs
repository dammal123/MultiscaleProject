using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public enum CellState
        {
            Inclusion,
            Grain,
            Empty
        }
        public Cell()
        {
            this.cellColorId = 0;
            this.cellColor = setDefaultCellColor();
            this.isRecrystalized = false;
            this.cellEnergy = 0;
            this.cellState = CellState.Empty;
            this.isNotGrown = true;
        }
        public Color setDefaultCellColor()//int cellId)
        {
            return Color.FromArgb(250, 250, 250);
        }
        public Color setCellColorRandom()
        {
            Random r = new Random();
            this.cellColor = Color.FromArgb(r.Next(249) + 1, r.Next(249) + 1, r.Next(249) + 1);
            return this.cellColor;
        }
    }
}
