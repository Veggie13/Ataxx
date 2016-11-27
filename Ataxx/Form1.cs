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
        Board _grid = new Board(-1);

        public Form1()
        {
            InitializeComponent();

            init();
        }

        void init()
        {
            _grid[0, 0].State = CellState.Human;
            _grid[6, 6].State = CellState.Human;
            _grid[0, 6].State = CellState.Computer;
            _grid[6, 0].State = CellState.Computer;
        }

        Board clone(Board grid)
        {
            return grid.Clone();
        }

        void makeMove(Board grid, int x1, int y1, int x2, int y2)
        {
            grid.Move(new Ataxx.Move((byte)x1, (byte)y1, (byte)x2, (byte)y2));
        }
        
        int score(Board grid, CellState player)
        {
            return grid.GetDifferential(player);
        }

        IEnumerable<Move> validPlays(Board grid, CellState player)
        {
            return grid.GetStateCells(player).SelectMany(c => c.ValidMoves);
        }

        Tuple<Move, int> bestMove(Board grid, CellState player, CellState opponent, int lookAhead, bool parallel = false)
        {
            var valid = validPlays(grid, player).ToList();

            Func<Move, Tuple<Move, int>> select = move => new Tuple<Move, int>(move, testMove(grid, move, player, opponent, lookAhead));
            var tests = parallel ? valid.AsParallel().Select(select).ToList() : valid.Select(select).ToList();
            return tests.OrderByDescending(t => t.Item2).FirstOrDefault();
        }

        int testMove(Board grid, Move move, CellState player, CellState opponent, int lookAhead)
        {
            var newGrid = clone(grid);
            if (newGrid == null)
                return score(grid, player);
            makeMove(newGrid, move.From.Row, move.From.Col, move.To.Row, move.To.Col);
            if (lookAhead > 0)
            {
                var opponentMove = bestMove(newGrid, opponent, player, lookAhead - 1);
                if (opponentMove == null)
                    return int.MaxValue;
                return -opponentMove.Item2;
            }

            return score(newGrid, player);
        }

        int LookAhead { get { return int.Parse(textBox1.Text); } }

        private void button1_Click(object sender, EventArgs e)
        {
            Board.Reset();
            var move = bestMove(_grid, CellState.Human, CellState.Computer, LookAhead, true);
            makeMove(_grid, move.Item1.From.Row, move.Item1.From.Col, move.Item1.To.Row, move.Item1.To.Col);

            dataGridView1.Invalidate();
            dataGridView1.Update();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.Rows.AddRange(Enumerable.Range(0, 7).Select(r => new DataGridViewRow()).ToArray());
        }

        private void dataGridView1_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            e.Value = (_grid[(byte)e.RowIndex, (byte)e.ColumnIndex].State == CellState.Computer) ? "O" : (_grid[(byte)e.RowIndex, (byte)e.ColumnIndex].State == CellState.Human) ? "X" : "";
        }

        Coord _from;
        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
                _from = new Coord(e.RowIndex, e.ColumnIndex);
            else if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                makeMove(_grid, _from.Row, _from.Col, e.RowIndex, e.ColumnIndex);
                dataGridView1.Invalidate();
                dataGridView1.Update();
            }
        }

        IEnumerable<Tuple<Move, Board>> nextGrids(Board grid, CellState player, CellState opponent, Move orig)
        {
            return validPlays(grid, player).Select(move =>
            {
                var m = orig ?? move;
                var newGrid = clone(grid);
                makeMove(newGrid, move.From.Row, move.From.Col, move.To.Row, move.To.Col);
                return new Tuple<Move, Board>(m, newGrid);
            });
        }
    }
}
