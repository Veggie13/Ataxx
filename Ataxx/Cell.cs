using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ataxx
{
    enum CellState
    {
        Empty,
        Human,
        Computer
    }

    class Cell
    {
        public byte Index;
        public CellState State = CellState.Empty;
        public Board Board;

        public Cell(Board board, byte index)
        {
            Board = board;
            Index = index;
        }

        public void CloneFrom(Board board)
        {
            var source = board.Cells[Index];
            this.State = source.State;
        }

        public Coord Coord
        {
            get { return Coord.AllCoords[Index]; }
        }

        public IEnumerable<Move> ValidMoves
        {
            get { return Coord.ValidDestinations.Where(c => Board[c].State == CellState.Empty).Select(c => new Move(Coord, c)); }
        }

        public override string ToString()
        {
            return (State == CellState.Empty ? "." : State == CellState.Human ? "R" : "Y") + " " + this.Coord.ToString();
        }
    }
}
