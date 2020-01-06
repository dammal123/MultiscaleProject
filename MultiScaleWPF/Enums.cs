using System;
using System.Collections.Generic;
using System.Text;

namespace MultiScaleWPF
{
    public static class Enums
    {
        public enum NeighbourhoodType
        {
            vonNeuman,
            Moore,
            Propability
        }
        public enum OperationType
        {
            substructure,
            dualPhase
        }
        public enum InclusionShape
        {
            Round,
            Square
        }
        public enum CellState
        {
            Inclusion,
            Grain,
            Empty,
            Substructure,
            DualPhase,
            Border
        }
    }
}
