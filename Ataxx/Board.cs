using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ataxx
{
    class Board
    {
        static object Locker = new object();
        static int TopBoard = 0;
        static int MaxBoards = 550000;
        static List<Board> AllBoards = new List<Board>(MaxBoards);

        static Board()
        {
            foreach (int i in Enumerable.Range(0, MaxBoards))
            {
                AllBoards.Add(new Ataxx.Board(i));
            }
        }

        public static void Reset()
        {
            TopBoard = 0;
        }

        public Cell[] Cells;
        public int Index;

        public Board(int index)
        {
            Index = index;
            Cells = Enumerable.Range(0, Coord.AllCoords.Count).Select(i => new Cell(this, (byte)i)).ToArray();
        }

        public Cell this[byte row, byte col]
        {
            get { return this[new Coord(row, col)]; }
        }

        public Cell this[Coord c]
        {
            get { return Cells[Coord.Indices[c]]; }
        }

        public Board Clone()
        {
            int next;
            lock (Locker) next = TopBoard++;
            if (next >= MaxBoards)
                return null;
            var result = AllBoards[next];
            foreach (var cell in result.Cells)
            {
                cell.CloneFrom(this);
            }
            return result;
        }

        public bool Move(Move move)
        {
            var fromCoord = move.From;
            var toCoord = move.To;
            if (!Coord.Connections[fromCoord].Contains(toCoord) && !Coord.Reach[fromCoord].Contains(toCoord)) return false;
            var from = Cells[Coord.Indices[fromCoord]];
            var to = Cells[Coord.Indices[toCoord]];
            if (from.State == CellState.Empty) return false;
            if (to.State != CellState.Empty) return false;
            to.State = from.State;
            if (Coord.Reach[fromCoord].Contains(toCoord)) from.State = CellState.Empty;
            foreach (var cxn in Coord.Connections[toCoord].Select(c => Cells[Coord.Indices[c]]))
            {
                if (cxn.State != CellState.Empty) cxn.State = to.State;
            }
            return true;
        }

        public IEnumerable<Cell> GetStateCells(CellState state)
        {
            return Cells.Where(c => c.State == state);
        }

        public int GetScore(CellState team)
        {
            return Cells.Count(c => c.State == team);
        }

        public int GetDifferential(CellState team)
        {
            return GetScore(team) - GetScore(team == CellState.Human ? CellState.Computer : CellState.Human);
        }
    }
}
