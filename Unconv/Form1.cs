using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Unconv
{
    public partial class Form1 : Form
    {
        private int BASE = 6;

        private int UNSOLVEDLIMIT;

        private TextBox[,] board;
        private Block[] blocks;
        private int[,] puzzle;
        Random R;

        List<string> stored;
        List<string> storedfixed;
        int storedindex = 0;

        //make a struct/class for "blocks" that stores an array of points specifying where it is

        public Form1()
        {
            InitializeComponent();
            R = new Random();
            InitializeBoard();

            stored = new List<string>();
            storedfixed = new List<string>();
            StreamReader sr = new StreamReader("C:\\Users\\Devouree\\Desktop\\unconventionals.txt");
            while(!sr.EndOfStream)
            {
                stored.Add(sr.ReadLine());
            }
            sr.Close();
        }

        struct Block
        {
            public Point[] slots;

            public Block(Point[] slts)
            {
                slots = slts;
            }

            public bool ContainsSlot(int x, int y)
            {
                bool rv = false;
                for (int i = 0; i < slots.Length; i++)
                {
                    if (slots[i].X == x && slots[i].Y == y)
                    {
                        rv = true;
                        break;
                    }
                }
                return rv;
            }

            public bool ContainsValue(int val, int[,] puzzle)
            {
                bool rv = false;
                for (int i = 0; i < slots.Length; i++)
                {
                    if (puzzle[slots[i].X, slots[i].Y] == val)
                    {
                        rv = true;
                        break;
                    }
                }
                return rv;
            }

            public void Paint(Color c, TextBox[,] board)
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    board[slots[i].X, slots[i].Y].BackColor = c;
                }
            }
        }

        private void InitializeBoard()
        {
            this.Size = new Size(121 + Convert.ToInt32((84 + ((BASE - 1) * 44))), Convert.ToInt32((114 + ((BASE - 1) * 44))));

            if (board != null)
            {
                foreach (TextBox tb in board)
                {
                    tb.Dispose();
                }
            }

            board = new TextBox[BASE, BASE];
            for (int i = 0; i < BASE; i++)
            {
                for (int j = 0; j < BASE; j++)
                {
                    TextBox addition = new TextBox();
                    addition.Size = new Size(38, 38);
                    addition.MaxLength = 1;
                    if (BASE > 9) { addition.MaxLength = 2; }
                    addition.Font = new Font("Microsoft Sans Serif", Convert.ToInt32(20.25));
                    addition.TextAlign = HorizontalAlignment.Center;
                    addition.Location = new Point(23 + (j * 44), 23 + (i * 44));
                    addition.KeyDown += tb_KeyDown;
                    //addition.KeyUp += tb_KeyUp;
                    this.Controls.Add(addition);
                    board[i, j] = addition;
                }
            }

            panel1.Location = new Point(121 + Convert.ToInt32((84 + ((BASE - 1) * 44))) - (panel1.Width + 40), 23);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int bs = BASE;
            try { bs = Convert.ToInt32(textBox1.Text); }
            catch { }
            if (bs < 5 || bs > 11)
            {
                bs = BASE;
            }
            BASE = bs;
            if (sender != null) { UNSOLVEDLIMIT = BASE * BASE; }

            InitializeBoard();

            int counter = 0;

            bool notdone = true;
            while (notdone)
            {
                blocks = new Block[BASE];
                puzzle = new int[BASE, BASE];
                int[,] tracker = new int[BASE, BASE];
                counter++;
                if (RecursiveBlockGeneration(0, new Point(0, 0), tracker))
                {
                    for (int i = 1; i <= BASE; i++)
                    {
                        Point[] pts = new Point[BASE];
                        int count = 0;
                        for (int x = 0; x < BASE; x++)
                        {
                            for (int y = 0; y < BASE; y++)
                            {
                                if (tracker[x, y] == i)
                                {
                                    pts[count] = new Point(x, y);
                                    count++;
                                }
                            }
                        }
                        blocks[i - 1] = new Block(pts);
                    }

                    if (RecursiveGeneratePuzzle(1, 0))
                    {
                        notdone = false;
                        for (int i = 0; i < BASE; i++)
                        {
                            Color c = Color.FromArgb((255 / (BASE - 1)) * i, R.Next(256), 255 - ((255 / (BASE - 1)) * i));
                            blocks[i].Paint(c, board);
                        }
                    }
                }
            }
            label1.Text = counter.ToString();
            Unsolve();
            print();

            string puzz = "";
            string blox = "";
            for (int i = 0; i < BASE; i++)
            {
                for (int j = 0; j < BASE; j++)
                {
                    puzz += puzzle[i, j].ToString();

                    for (int k = 0; k < blocks.Length; k++)
                    {
                        if (blocks[k].ContainsSlot(i, j))
                        {
                            blox += k.ToString();
                        }
                    }

                }
            }
            textBox2.Text = puzz + " " + blox;
        }

        private bool RecursiveBlockGeneration(int count, Point slot, int[,] tracker)
        {
            if (count == BASE * BASE)
            {
                return true;
            }
            else if (slot.X < 0 || slot.Y < 0 || slot.X >= BASE || slot.Y >= BASE || tracker[slot.X, slot.Y] > 0)
            {
                return false;
            }
            if (Math.IEEERemainder(count, BASE) == 0)
            {
                bool failed = true;
                int firstrow = -1;
                int firstcol = -1;
                for (int i = 0; i < BASE; i++)
                {
                    for (int j = 0; j < BASE; j++)
                    {
                        if (tracker[i, j] == count / BASE)
                        {
                            if (firstrow == -1)
                            {
                                firstrow = i;
                                firstcol = j;
                            }
                            else
                            {
                                if (i != firstrow && j != firstcol)
                                {
                                    failed = false;
                                    break;
                                }
                            }
                        }
                    }
                    if (!failed) { break; }
                }
                if (failed) { return false; }

                if (count == (BASE * BASE) - BASE)
                {
                    bool failed1 = true;
                    int firstrow1 = -1;
                    int firstcol1 = -1;
                    for (int i = 0; i < BASE; i++)
                    {
                        for (int j = 0; j < BASE; j++)
                        {
                            if (tracker[i, j] == 0)
                            {
                                if (firstrow1 == -1)
                                {
                                    firstrow1 = i;
                                    firstcol1 = j;
                                }
                                else
                                {
                                    if (i != firstrow1 && j != firstcol1)
                                    {
                                        failed1 = false;
                                        break;
                                    }
                                }
                            }
                        }
                        if (!failed1) { break; }
                    }
                    if (failed1) { return false; }
                }
            }

            tracker[slot.X, slot.Y] = (count / BASE) + 1;

            List<Point> points = new List<Point>();
            points.Add(new Point(slot.X, slot.Y + 1));
            points.Add(new Point(slot.X, slot.Y - 1));
            points.Add(new Point(slot.X + 1, slot.Y));
            points.Add(new Point(slot.X - 1, slot.Y));

            for (int i = 0; i < 4; i++)
            {
                int ind = R.Next(4 - i);
                if (RecursiveBlockGeneration(count + 1, points.ElementAt(ind), tracker))
                {
                    return true;
                }
                points.Remove(points.ElementAt(ind));
            }

            tracker[slot.X, slot.Y] = 0;
            return false;
        }

        private bool RecursiveGeneratePuzzle(int value, int row)
        {
            if (value == BASE && row == BASE - 1)
            {
                for (int i = 0; i < BASE; i++)
                {
                    if (puzzle[row, i] == 0)
                    {
                        puzzle[row, i] = value;
                        return true;
                    }
                }
            }

            int error = 0;
            bool[] errors = new bool[BASE];
            int index;

            while (error < BASE)
            {
                index = R.Next(BASE);
                if (!errors[index])
                {
                    if (puzzle[row, index] == 0 && !ColContains(index, value) && !(GetBlock(row, index).ContainsValue(value, puzzle)))
                    {
                        puzzle[row, index] = value;
                        if (row == BASE - 1)
                        {
                            if (!RecursiveGeneratePuzzle(value + 1, 0))
                            {
                                puzzle[row, index] = 0;
                                errors[index] = true;
                                error++;
                            }
                            else
                            {
                                return true;
                            }
                        }
                        else
                        {
                            if (!RecursiveGeneratePuzzle(value, row + 1))
                            {
                                puzzle[row, index] = 0;
                                errors[index] = true;
                                error++;
                            }
                            else
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        error++;
                        errors[index] = true;
                    }
                }
            }
            return false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SetPuzzle();
            label1.Text = IsSolved().ToString();
        }

        private Block GetBlock(int x, int y)
        {
            for (int i = 0; i < blocks.Length; i++)
            {
                if (blocks[i].ContainsSlot(x, y))
                {
                    return blocks[i];
                }
            }
            return new Block();
        }

        private bool ColContains(int col, int value)
        {
            for (int i = 0; i < BASE; i++)
            {
                if (puzzle[i, col] == value)
                {
                    return true;
                }
            }
            return false;
        }

        private bool RowContains(int row, int value)
        {
            for (int i = 0; i < BASE; i++)
            {
                if (puzzle[row, i] == value)
                {
                    return true;
                }
            }
            return false;
        }

        private void print()
        {
            for (int i = 0; i < BASE; i++)
            {
                for (int j = 0; j < BASE; j++)
                {
                    if (puzzle[i, j] != 0)
                    {
                        board[i, j].Text = puzzle[i, j].ToString();
                    }
                }
            }
        }

        private void erase(bool[,,] pen, int row, int col)
        {
            for (int i = 0; i < BASE; i++)
            {
                pen[row, col, i] = false;
            }
        }

        private void Solve()
        {
            bool[,,] pencil = new bool[BASE, BASE, BASE];

            //make basic pencil marks
            for (int i = 0; i < BASE; i++)
            {
                for (int j = 0; j < BASE; j++)
                {
                    if (puzzle[i, j] == 0)
                    {
                        for (int k = 1; k <= BASE; k++)
                        {
                            if (!RowContains(i, k) && !ColContains(j, k) && !(GetBlock(i, j).ContainsValue(k, puzzle)))
                            {
                                pencil[i, j, k - 1] = true;
                            }
                        }
                    }
                }
            }

            bool solvedone = true;

            //0=hiddensingles
            int[] typeCounts = new int[1];

            while (solvedone)
            {
                solvedone = false;
                int typecounter = 0;

                //update basic pencil marks
                for (int i = 0; i < BASE; i++)
                {
                    for (int j = 0; j < BASE; j++)
                    {
                        if (puzzle[i, j] == 0)
                        {
                            for (int k = 1; k <= BASE; k++)
                            {
                                if (RowContains(i, k) || ColContains(j, k) || (GetBlock(i, j).ContainsValue(k, puzzle)))
                                {
                                    if (pencil[i, j, k - 1])
                                    {
                                        pencil[i, j, k - 1] = false;
                                        solvedone = true;
                                    }
                                }
                            }
                        }
                    }
                }

                //hiddensingles
                if (!solvedone)
                {
                    for (int k = 0; k < BASE; k++)
                    {
                        //go through rows
                        for (int i = 0; i < BASE; i++)
                        {
                            int count = 0;
                            int index = 0;
                            for (int j = 0; j < BASE; j++)
                            {
                                if (pencil[i, j, k])
                                {
                                    count++;
                                    index = j;
                                }
                            }
                            if (count == 1)
                            {
                                erase(pencil, i, index);
                                pencil[i, index, k] = true;
                                typeCounts[typecounter]++;
                                solvedone = true;
                            }
                        }

                        //go through columns
                        for (int j = 0; j < BASE; j++)
                        {
                            int count = 0;
                            int index = 0;
                            for (int i = 0; i < BASE; i++)
                            {
                                if (pencil[i, j, k])
                                {
                                    count++;
                                    index = i;
                                }
                            }
                            if (count == 1)
                            {
                                erase(pencil, index, j);
                                pencil[index, j, k] = true;
                                typeCounts[typecounter]++;
                                solvedone = true;
                            }
                        }

                        //go through quads
                        for (int q = 0; q < BASE; q++)
                        {
                            int count = 0;
                            Point index = new Point(0, 0);
                            for (int i = 0; i < BASE; i++)
                            {
                                if (pencil[blocks[q].slots[i].X, blocks[q].slots[i].Y, k])
                                {
                                    count++;
                                    index.X = blocks[q].slots[i].X;
                                    index.Y = blocks[q].slots[i].Y;
                                }
                            }
                            if (count == 1)
                            {
                                erase(pencil, index.X, index.Y);
                                pencil[index.X, index.Y, k] = true;
                                typeCounts[typecounter]++;
                                solvedone = true;
                            }
                        }
                    }
                }

                SolveSingles(pencil);
            }
        }

        private bool IsSolved()
        {
            for (int i = 0; i < BASE; i++)
            {
                for (int j = 0; j < BASE; j++)
                {
                    if (puzzle[i, j] < 1 || puzzle[i, j] > BASE)
                    {
                        return false;
                    }
                    int tempVal = puzzle[i, j];
                    puzzle[i, j] = 0;
                    if (RowContains(i, tempVal) || ColContains(j, tempVal) || (GetBlock(i, j).ContainsValue(tempVal, puzzle)))
                    {
                        puzzle[i, j] = tempVal;
                        return false;
                    }
                    puzzle[i, j] = tempVal;
                }
            }
            return true;
        }

        private void Unsolve()
        {
            bool notDone = true;
            int row;
            int col;
            int tries = 0;
            int unsolved = 0;
            for (int i = 0; i < BASE; i++)
            {
                for (int j = 0; j < BASE; j++)
                {
                    if (puzzle[i, j] == 0)
                    {
                        unsolved++;
                    }
                }
            }

            while (notDone && unsolved < UNSOLVEDLIMIT)
            {
                row = R.Next(BASE);
                col = R.Next(BASE);
                if (puzzle[row, col] == 0)
                {
                    for (int m = 0; m < BASE; m++)
                    {
                        for (int n = 0; n < BASE; n++)
                        {
                            int mm = row + m;
                            if (mm >= BASE) { mm -= BASE; }
                            int nn = col + n;
                            if (nn >= BASE) { nn -= BASE; }
                            if (puzzle[mm, nn] != 0)
                            {
                                row = mm;
                                col = nn;
                            }
                        }
                    }
                }

                int[,] puzcopy = new int[BASE, BASE];
                for (int t = 0; t < BASE; t++)
                {
                    for (int u = 0; u < BASE; u++)
                    {
                        puzcopy[t, u] = puzzle[t, u];
                    }
                }
                puzzle[row, col] = 0;
                Solve();
                bool good = IsSolved();
                puzzle = puzcopy;

                if (good)
                {
                    puzzle[row, col] = 0;
                    unsolved++;
                    tries = 0;
                }
                else { tries++; }

                if (tries > BASE * BASE * 2)
                {
                    notDone = false;
                }
            }
        }

        private int SolveSingles(bool[,,] pen)
        {
            int solved = 0;
            for (int i = 0; i < BASE; i++)
            {
                for (int j = 0; j < BASE; j++)
                {
                    int index = -1;
                    for (int k = 0; k < BASE; k++)
                    {
                        if (pen[i, j, k])
                        {
                            if (index == -1)
                            {
                                index = k;
                            }
                            else
                            {
                                index = -2;
                            }
                        }
                    }
                    if (index >= 0)
                    {
                        puzzle[i, j] = index + 1;
                        erase(pen, i, j);
                        solved++;
                    }
                }
            }
            return solved;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SetPuzzle();
            Solve();
            print();
        }

        private void SetPuzzle()
        {
            for (int i = 0; i < BASE; i++)
            {
                for (int j = 0; j < BASE; j++)
                {
                    int val = 0;
                    try { val = Convert.ToInt32(board[i, j].Text); }
                    catch { }
                    if (val < 1 || val > BASE)
                    {
                        val = 0;
                    }
                    puzzle[i, j] = val;
                }
            }
        }

        private void tb_KeyDown(object sender, KeyEventArgs e)
        {
            int x = -1;
            int y = -1;
            for (int i = 0; i < BASE; i++)
            {
                for (int j = 0; j < BASE; j++)
                {
                    if (board[i, j].Focused)
                    {
                        x = i;
                        y = j;
                    }
                }
            }
            if (x >= 0 && y >= 0)
            {
                if (e.KeyCode == Keys.Left && y > 0)
                {
                    board[x, y - 1].Focus();
                }
                else if (e.KeyCode == Keys.Up && x > 0)
                {
                    board[x - 1, y].Focus();
                }
                else if (e.KeyCode == Keys.Right && y < BASE - 1)
                {
                    board[x, y + 1].Focus();
                }
                else if (e.KeyCode == Keys.Down && x < BASE - 1)
                {
                    board[x + 1, y].Focus();
                }
                else if (((e.KeyCode <= Keys.D9 && e.KeyCode >= Keys.D1) || (e.KeyCode <= Keys.NumPad9 && e.KeyCode >= Keys.NumPad1) && !(board[x, y].ReadOnly)) && (BASE <= 9 || board[x, y].Text.Length >= 2))
                {
                    board[x, y].Text = "";
                }
                else if (e.KeyCode == Keys.Enter && board[x, y].Text.Equals(""))
                {
                    bool[] found = new bool[BASE];
                    int count = 0;

                    //row
                    for (int j = 0; j < BASE; j++)
                    {
                        int k = 0;
                        try { k = Convert.ToInt32(board[x, j].Text); }
                        catch { }
                        if (k > 0 && k <= BASE && !found[k - 1])
                        {
                            found[k - 1] = true;
                            count++;
                        }
                    }
                    if (count == BASE - 1)
                    {
                        for (int m = 0; m < BASE; m++)
                        {
                            if (!found[m])
                            {
                                board[x, y].Text = (m + 1).ToString();
                            }
                        }
                    }
                    else
                    {
                        //col
                        count = 0;
                        found = new bool[BASE];
                        for (int i = 0; i < BASE; i++)
                        {
                            int k = 0;
                            try { k = Convert.ToInt32(board[i, y].Text); }
                            catch { }
                            if (k > 0 && k <= BASE && !found[k - 1])
                            {
                                found[k - 1] = true;
                                count++;
                            }
                        }
                        if (count == BASE - 1)
                        {
                            for (int m = 0; m < BASE; m++)
                            {
                                if (!found[m])
                                {
                                    board[x, y].Text = (m + 1).ToString();
                                }
                            }
                        }
                        else
                        {
                            //quad
                            count = 0;
                            found = new bool[BASE];
                            Point[] slts = GetBlock(x, y).slots;
                            for (int q = 0; q < BASE; q++)
                            {
                                int i = slts[q].X;
                                int j = slts[q].Y;
                                int k = 0;
                                try { k = Convert.ToInt32(board[i, j].Text); }
                                catch { }
                                if (k > 0 && k <= BASE && !found[k - 1])
                                {
                                    found[k - 1] = true;
                                    count++;
                                }
                            }
                            if (count == BASE - 1)
                            {
                                for (int m = 0; m < BASE; m++)
                                {
                                    if (!found[m])
                                    {
                                        board[x, y].Text = (m + 1).ToString();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string s = textBox2.Text;

            try
            {
                string puzz = s.Split(' ')[0];
                string blox = s.Split(' ')[1];
                BASE = Convert.ToInt32(Math.Sqrt(puzz.Length));
                puzzle = new int[BASE, BASE];
                InitializeBoard();

                int[] counts = new int[BASE];
                Point[,] pts = new Point[BASE, BASE];
                int count = 0;
                for(int i = 0; i < BASE; i++)
                {
                    for(int j = 0; j < BASE; j++)
                    {
                        puzzle[i, j] = Convert.ToInt32(puzz.Substring(count, 1));
                        int b = Convert.ToInt32(blox.Substring(count, 1));
                        pts[b, counts[b]] = new Point(i, j);
                        counts[b]++;
                        count++;
                    }
                }

                blocks = new Block[BASE];
                for(int i = 0; i < BASE; i++)
                {
                    Point[] slts = new Point[BASE];
                    for(int m = 0; m < BASE; m++)
                    {
                        slts[m] = pts[i, m];
                    }
                    blocks[i] = new Block(slts);
                    Color c = Color.FromArgb((255 / (BASE - 1)) * i, R.Next(256), 255 - ((255 / (BASE - 1)) * i));
                    blocks[i].Paint(c, board);
                }

                print();
            }
            catch { }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            StreamWriter sw = new StreamWriter("C:\\Users\\Devouree\\Desktop\\unconventionals.txt");
            BASE = 6;
            for (int m = 0; m < 8; m++)
            {
                UNSOLVEDLIMIT = BASE * BASE;

                bool notdone = true;
                while (notdone)
                {
                    blocks = new Block[BASE];
                    puzzle = new int[BASE, BASE];
                    int[,] tracker = new int[BASE, BASE];
                    if (RecursiveBlockGeneration(0, new Point(0, 0), tracker))
                    {
                        for (int i = 1; i <= BASE; i++)
                        {
                            Point[] pts = new Point[BASE];
                            int count = 0;
                            for (int x = 0; x < BASE; x++)
                            {
                                for (int y = 0; y < BASE; y++)
                                {
                                    if (tracker[x, y] == i)
                                    {
                                        pts[count] = new Point(x, y);
                                        count++;
                                    }
                                }
                            }
                            blocks[i - 1] = new Block(pts);
                        }

                        if (RecursiveGeneratePuzzle(1, 0))
                        {
                            notdone = false;
                        }
                    }
                }
                Unsolve();

                string puzz = "";
                string blox = "";
                int unsolved = 0;
                for (int i = 0; i < BASE; i++)
                {
                    for(int j = 0; j < BASE; j++)
                    {
                        puzz += puzzle[i, j].ToString();
                        if(puzzle[i, j] == 0)
                        {
                            unsolved++;
                        }

                        for (int k = 0; k < blocks.Length; k++)
                        {
                            if (blocks[k].ContainsSlot(i, j))
                            {
                                blox += k.ToString();
                            }
                        }

                    }
                }
                sw.WriteLine(puzz + " " + blox + " " + unsolved.ToString());
            }
            sw.Close();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                string s = textBox2.Text.Split(' ')[0];
                BASE = Convert.ToInt32(Math.Sqrt(s.Length));
                UNSOLVEDLIMIT = 0;
                for (int i = 0; i < s.Length; i++)
                {
                    if (s.ElementAt(i).Equals('0'))
                    {
                        UNSOLVEDLIMIT++;
                    }
                }
            }
            catch { }

            button1_Click(null, null);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (storedindex >= stored.Count)
            {
                StreamWriter sw = new StreamWriter("C:\\Users\\Devouree\\Desktop\\unconventionals2.txt");
                foreach(string s in storedfixed)
                {
                    sw.WriteLine(s);
                }
                sw.Close();
            }
            else
            {
                string s = stored.ElementAt(storedindex);
                textBox2.Text = s;
                button4_Click(null, null);
                bool changed = false;
                bool done = false;
                while (!done)
                {
                    done = true;

                    for (int i = 0; i < BASE; i++)
                    {
                        int count = 0;
                        for (int j = 0; j < BASE; j++)
                        {
                            if (puzzle[i, j] == 0)
                            {
                                count++;
                            }
                        }
                        if (count <= 1)
                        {
                            done = false;
                            break;
                        }
                    }
                    for (int j = 0; j < BASE; j++)
                    {
                        int count = 0;
                        for (int i = 0; i < BASE; i++)
                        {
                            if (puzzle[i, j] == 0)
                            {
                                count++;
                            }
                        }
                        if (count <= 1)
                        {
                            done = false;
                            break;
                        }
                    }
                    for (int q = 0; q < BASE; q++)
                    {
                        int count = 0;
                        for (int m = 0; m < BASE; m++)
                        {
                            int i = blocks[q].slots[m].X;
                            int j = blocks[q].slots[m].Y;
                            if (puzzle[i, j] == 0)
                            {
                                count++;
                            }
                        }
                        if (count <= 1)
                        {
                            done = false;
                            break;
                        }
                    }

                    if(!done)
                    {
                        button6_Click(null, null);
                        changed = true;
                    }
                }

                storedfixed.Add(textBox2.Text);

                storedindex++;
                label1.Text = "P:" + storedindex;
                if (changed) { label1.Text += "(Y)"; }
                else { label1.Text += "(N)"; }
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            List<string> convertables = new List<string>();
            StreamReader sr1 = new StreamReader("C:\\Users\\Devouree\\Desktop\\New folder\\Puzzles\\UNCONVENTIONAL\\FIVES.txt");
            while (!sr1.EndOfStream)
            {
                convertables.Add(sr1.ReadLine());
            }
            sr1.Close();
            StreamReader sr2 = new StreamReader("C:\\Users\\Devouree\\Desktop\\New folder\\Puzzles\\UNCONVENTIONAL\\SIXES.txt");
            while (!sr2.EndOfStream)
            {
                convertables.Add(sr2.ReadLine());
            }
            sr2.Close();
            StreamReader sr3 = new StreamReader("C:\\Users\\Devouree\\Desktop\\New folder\\Puzzles\\UNCONVENTIONAL\\SEVENS.txt");
            while (!sr3.EndOfStream)
            {
                convertables.Add(sr3.ReadLine());
            }
            sr3.Close();

            StreamWriter sw = new StreamWriter("C:\\Users\\Devouree\\Desktop\\New folder\\Puzzles\\UNCONVENTIONAL\\SOLUTIONS.txt");
            foreach(string s in convertables)
            {
                textBox2.Text = s;
                button4_Click(null, null);
                button3_Click(null, null);
                string newpuzz = "";
                for(int i = 0; i < BASE; i++)
                {
                    for(int j = 0; j < BASE; j++)
                    {
                        newpuzz += puzzle[i, j].ToString();
                    }
                }
                sw.WriteLine(newpuzz + " " + s.Split(' ')[1]);
            }
            sw.Close();
        }
    }
}
