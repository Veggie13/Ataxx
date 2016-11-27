using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ataxx
{
    class Coord : IEquatable<Coord>
    {
        public static Dictionary<Coord, List<Coord>> Connections = new Dictionary<Coord, List<Coord>>();
        public static Dictionary<Coord, List<Coord>> Reach = new Dictionary<Coord, List<Coord>>();
        public static List<Coord> AllCoords = new List<Coord>();
        public static Dictionary<Coord, byte> Indices;

        static Coord()
        {
            for (int row = 0; row < 7; row++)
            {
                for (int col = 0; col < 7; col++)
                {
                    AllCoords.Add(new Coord(row, col));
                }
            }

            Indices = Enumerable.Range(0, AllCoords.Count).ToDictionary(i => AllCoords[i], i => (byte)i);

            foreach (var coord in AllCoords)
            {
                var cxns = new[]
                {
                    new Coord(coord.Row - 1, coord.Col - 1),
                    new Coord(coord.Row - 1, coord.Col),
                    new Coord(coord.Row - 1, coord.Col + 1),
                    new Coord(coord.Row, coord.Col - 1),
                    new Coord(coord.Row, coord.Col + 1),
                    new Coord(coord.Row + 1, coord.Col - 1),
                    new Coord(coord.Row + 1, coord.Col),
                    new Coord(coord.Row + 1, coord.Col + 1),
                };
                var rch = new[]
                {
                    new Coord(coord.Row - 2, coord.Col - 2),
                    new Coord(coord.Row - 2, coord.Col - 1),
                    new Coord(coord.Row - 2, coord.Col),
                    new Coord(coord.Row - 2, coord.Col + 1),
                    new Coord(coord.Row - 2, coord.Col + 2),
                    new Coord(coord.Row - 1, coord.Col - 2),
                    new Coord(coord.Row - 1, coord.Col + 2),
                    new Coord(coord.Row, coord.Col - 2),
                    new Coord(coord.Row, coord.Col + 2),
                    new Coord(coord.Row + 1, coord.Col - 2),
                    new Coord(coord.Row + 1, coord.Col + 2),
                    new Coord(coord.Row + 2, coord.Col - 2),
                    new Coord(coord.Row + 2, coord.Col - 1),
                    new Coord(coord.Row + 2, coord.Col),
                    new Coord(coord.Row + 2, coord.Col + 1),
                    new Coord(coord.Row + 2, coord.Col + 2),
                };

                Connections[coord] = cxns.Intersect(AllCoords).ToList();
                Reach[coord] = rch.Intersect(AllCoords).ToList();
            }
        }

        public byte Row, Col;

        public Coord(int row, int col)
        {
            Row = (byte)row;
            Col = (byte)col;
        }

        public IEnumerable<Coord> ValidDestinations
        {
            get { return Connections[this].Concat(Reach[this]); }
        }

        public bool Equals(Coord other)
        {
            return Row == other.Row && Col == other.Col;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Coord);
        }

        public override int GetHashCode()
        {
            return (Row << 8) + Col;
        }

        public override string ToString()
        {
            return string.Format("({0},{1})", Row, Col);
        }
    }
}
