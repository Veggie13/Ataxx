using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ataxx
{
    class Move
    {
        public Coord From;
        public Coord To;

        public static Move New(string cmd)
        {
            var coords = cmd.Split(',').Select(x => int.Parse(x.Trim())).ToArray();
            return new Move((byte)coords[0], (byte)coords[1], (byte)coords[2], (byte)coords[3]);
        }

        public Move(Coord from, Coord to)
        {
            From = from;
            To = to;
        }

        public Move(byte fromRow, byte fromCol, byte toRow, byte toCol)
        {
            From = new Coord(fromRow, fromCol);
            To = new Coord(toRow, toCol);
        }

        public override string ToString()
        {
            return From.ToString() + "-" + To.ToString();
        }
    }
}
