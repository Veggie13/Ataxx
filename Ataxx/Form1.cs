using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ataxx
{
    public partial class Form1 : Form
    {
        const int N = 5500000;
        enum CellState
        {
            Empty,
            Human,
            Computer
        }

        CellState[,] _grid = new CellState[7, 7];
        CellState[][,] _grids = new CellState[N][,];

        public Form1()
        {
            InitializeComponent();

            init();
        }

        void init()
        {
            _grid[0, 0] = CellState.Human;
            _grid[6, 6] = CellState.Human;
            _grid[0, 6] = CellState.Computer;
            _grid[6, 0] = CellState.Computer;

            foreach (int i in Enumerable.Range(0, N))
            {
                _grids[i] = new CellState[7, 7];
            }
        }

        int _next = 0;
        object _locker = new object();
        CellState[,] clone(CellState[,] grid)
        {
            int next;
            lock (_locker) next = _next++;
            if (next >= N)
                return null;
            var result = _grids[next];
            Array.Copy(grid, result, 49);
            return result;
        }

        void makeMove(CellState[,] grid, int x1, int y1, int x2, int y2)
        {
            CellState orig = grid[x1, y1];
            switch (orig)
            {
                case CellState.Empty:
                    break;
                default:
                    if (Math.Abs(x2 - x1) > 1 || Math.Abs(y2 - y1) > 1)
                        grid[x1, y1] = CellState.Empty;
                    placePiece(grid, orig, x2, y2);
                    break;
            }
        }
        
        void placePiece(CellState[,] grid, CellState play, int x, int y)
        {
            CellState opponent = (play == CellState.Computer) ? CellState.Human : CellState.Computer;

            grid[x, y] = play;
            for (int xx = x - 1; xx <= x + 1; xx++)
            {
                for (int yy = y - 1; yy <= y + 1; yy++)
                {
                    if (xx < 0 || xx > 6 || yy < 0 || yy > 6)
                        continue;
                    if (grid[xx, yy] == opponent)
                        grid[xx, yy] = play;
                }
            }
        }

        int score(CellState[,] grid, CellState player)
        {
            return Enumerable.Range(0, 7).Sum(x => Enumerable.Range(0, 7).Sum(y => (grid[x, y] == CellState.Empty) ? 0 : (grid[x, y] == player) ? 1 : -1));
        }

        IEnumerable<Tuple<Point, Point>> validPlays(CellState[,] grid, CellState player)
        {
            for (int x = 0; x < 7; x++)
            {
                for (int y = 0; y < 7; y++)
                {
                    if (grid[x,y] == CellState.Empty)
                    {
                        bool done = false;
                        for (int xx = x - 2; !done && xx <= x + 2; xx++)
                        {
                            for (int yy = y - 2; yy <= y + 2; yy++)
                            {
                                if (xx < 0 || xx > 6 || yy < 0 || yy > 6)
                                    continue;
                                if (grid[xx, yy] == player)
                                {
                                    yield return new Tuple<Point, Point>(new Point(xx, yy), new Point(x, y));
                                }
                            }
                        }
                    }
                }
            }
        }

        Tuple<Point, Point, int> bestMove(CellState[,] grid, CellState player, CellState opponent, int lookAhead, bool parallel = false)
        {
            var valid = validPlays(grid, player).ToList();

            Func<Tuple<Point,Point>,Tuple<Point,Point,int>> select = move => new Tuple<Point, Point, int>(move.Item1, move.Item2, testMove(grid, move, player, opponent, lookAhead));
            var tests = parallel ? valid.AsParallel().Select(select).ToList() : valid.Select(select).ToList();
            return tests.OrderByDescending(t => t.Item3).FirstOrDefault();
        }

        int testMove(CellState[,] grid, Tuple<Point, Point> move, CellState player, CellState opponent, int lookAhead)
        {
            var newGrid = clone(grid);
            if (newGrid == null)
                return score(grid, player);
            makeMove(newGrid, move.Item1.X, move.Item1.Y, move.Item2.X, move.Item2.Y);
            if (lookAhead > 0)
            {
                var opponentMove = bestMove(newGrid, opponent, player, lookAhead - 1);
                if (opponentMove == null)
                    return int.MaxValue;
                return -opponentMove.Item3;
            }

            return score(newGrid, player);
        }

        int LookAhead { get { return int.Parse(textBox1.Text); } }

        private void button1_Click(object sender, EventArgs e)
        {
            _next = 0;
            var move = bestMove(_grid, CellState.Human, CellState.Computer, LookAhead, true);
            makeMove(_grid, move.Item1.X, move.Item1.Y, move.Item2.X, move.Item2.Y);

            dataGridView1.Invalidate();
            dataGridView1.Update();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.Rows.AddRange(Enumerable.Range(0, 7).Select(r => new DataGridViewRow()).ToArray());
        }

        private void dataGridView1_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            e.Value = (_grid[e.ColumnIndex, e.RowIndex] == CellState.Computer) ? "O" : (_grid[e.ColumnIndex, e.RowIndex] == CellState.Human) ? "X" : "";
        }

        Point _from;
        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
                _from = new Point(e.ColumnIndex, e.RowIndex);
            else if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                makeMove(_grid, _from.X, _from.Y, e.ColumnIndex, e.RowIndex);
                dataGridView1.Invalidate();
                dataGridView1.Update();
            }
        }

        IEnumerable<Tuple<Point, Point, CellState[,]>> nextGrids(CellState[,] grid, CellState player, CellState opponent, Tuple<Point, Point> orig)
        {
            return validPlays(grid, player).Select(move =>
            {
                var m = orig ?? move;
                var newGrid = clone(grid);
                makeMove(newGrid, move.Item1.X, move.Item1.Y, move.Item2.X, move.Item2.Y);
                return new Tuple<Point, Point, CellState[,]>(m.Item1, m.Item2, newGrid);
            });
        }
    }
}
