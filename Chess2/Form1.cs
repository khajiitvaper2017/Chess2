using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Chess2.Chess;

namespace Chess2
{
    public partial class Form1 : Form
    {
        private Bitmap boardBitmap;
        private string Console, Moves;
        private Graphics g;

        private int moveid = 1;
        private Random random;

        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            EngineName = "sf13.exe";
            ChessAppDir = Environment.CurrentDirectory;
            ChessGUIName = Process.GetCurrentProcess().MainModule?.FileName;
            var psi = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = EngineName,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            EngineProcess = Process.Start(psi);
            EngineProcess.BeginOutputReadLine();
            EngineProcess.OutputDataReceived += Output;

            cellSize = pb1.Width / 8;
            boardBitmap = new Bitmap(pb1.Width, pb1.Height);
            pb1.Image = new Bitmap(pb1.Width, pb1.Height);
            g = Graphics.FromImage(pb1.Image);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            using (var g = Graphics.FromImage(boardBitmap))
            {
                for (var x = 0; x < 8; x++)
                for (var y = 0; y < 8; y++)
                {
                    g.FillRectangle((x + y) % 2 == 0 ? Brushes.White : Brushes.MediumSeaGreen,
                        new Rectangle(x * cellSize, y * cellSize, cellSize, cellSize));
                    var letter = (char) (x + 97);
                    var drawString = letter + (7 - y + 1).ToString();

                    // Create font and brush.
                    var drawFont = new Font("Arial", 10);
                    var drawBrush = new SolidBrush(Color.Black);


                    // Set format of string.
                    var drawFormat = new StringFormat();

                    // Draw string to screen.
                    g.DrawString(drawString, drawFont, drawBrush, x * cellSize, y * cellSize);
                }
            }

            ThrowDice();
            Render();
        }

        private void ThrowDice()
        {
            Task.Factory.StartNew(async () =>
            {
                PictureBox[] pbs =
                {
                    pictureBox1, pictureBox2, pictureBox3
                };
                foreach (var pb in pbs)
                {
                    pb.Image = new Bitmap(50, 50);
                    pb.Refresh();
                }

                await Task.Delay(500);
                pictureBox1.Image = new Bitmap(SelectFigureImage(RandomFigure()), new Size(50, 50));
                pictureBox1.Refresh();
                await Task.Delay(500);
                pictureBox2.Image = new Bitmap(SelectFigureImage(RandomFigure()), new Size(50, 50));
                pictureBox2.Refresh();
                await Task.Delay(500);
                pictureBox3.Image = new Bitmap(SelectFigureImage(RandomFigure()), new Size(50, 50));
                pictureBox3.Refresh();
            });
        }

        private char RandomFigure()
        {
            var fig = '\0';
            random = new Random();
            switch (random.Next(0, 6))
            {
                case 0:
                    fig = 'P';
                    break;
                case 1:
                    fig = 'N';
                    break;
                case 2:
                    fig = 'B';
                    break;
                case 3:
                    fig = 'R';
                    break;
                case 4:
                    fig = 'Q';
                    break;
                case 5:
                    fig = 'K';
                    break;
            }

            return !Turn ? char.ToLower(fig) : fig;
        }

        private void Render()
        {
            ParseFEN();

            label1.Text = Turn ? "Ход белых" : "Ход чёрных";

            g.Clear(Color.White);
            g.DrawImage(boardBitmap, 0, 0);

            for (var i = 0; i < 8; i++)
            for (var j = 0; j < 8; j++)
            {
                var figureImage = SelectFigureImage(board[j, i]);

                g.DrawImage(figureImage, i * cellSize, j * cellSize, cellSize, cellSize);
            }

            pb1.Refresh();
            GC.Collect();
        }

        public static string getBetween(string strSource, string strStart, string strEnd)
        {
            if (!strSource.Contains(strStart) || !strSource.Contains(strEnd)) return "";
            var Start = strSource.IndexOf(strStart, 0) + strStart.Length;
            var End = strSource.IndexOf(strEnd, Start);
            return strSource.Substring(Start, End - Start);
        }

        private void Output(object sender, DataReceivedEventArgs e)
        {
            Console += "\r\n" + e.Data;
            textBox2.Text = cbMovesConsole.Checked ? Moves : Console;
            if (e.Data.Contains("Fen:"))
            {
                FEN = e.Data.Remove(0, 5);
                Render();
            }

            if (cbEval.Checked)
            {
                if (e.Data.Contains("mate"))
                    try
                    {
                        label3.Text = "#" + Convert.ToInt32(getBetween(e.Data, "mate", "nodes"));
                    }
                    catch (Exception exception)
                    {
                        // ignored
                    }
                else if (e.Data.Contains("cp"))
                    try
                    {
                        label3.Text = (Convert.ToDouble(getBetween(e.Data, "cp", "nodes")) / 100).ToString();
                    }
                    catch (Exception exception)
                    {
                        // ignored
                    }
            }

            if (e.Data.Contains("bestmove ") && cbEngine.Checked)
            {
                var move = e.Data.Split();
                Send("position fen " + FEN + " moves " + move[1]);
                if (Turn)
                {
                    Moves += "\r\n" + moveid + ". ";
                    moveid++;
                }

                Moves += move[1] + " ";
                Send("d");
                textBox3.Enabled = true;
                textBox2.Text = cbMovesConsole.Checked ? Moves : Console;
            }
        }

        private Bitmap SelectFigureImage(char enumChess)
        {
            var point = new Point();

            Enum.TryParse(enumChess.ToString(), out Figure figure);
            if (enumChess == '\0') figure = Figure.Null;
            switch (figure)
            {
                case Figure.P:
                    point.X = 1000;
                    break;
                case Figure.p:
                    point.X = 1000;
                    point.Y = 200;
                    break;
                case Figure.K:
                    break;
                case Figure.k:
                    point.Y = 200;
                    break;
                case Figure.Q:
                    point.X = 200;
                    break;
                case Figure.q:
                    point.X = 200;
                    point.Y = 200;
                    break;
                case Figure.N:
                    point.X = 600;
                    break;
                case Figure.n:
                    point.X = 600;
                    point.Y = 200;
                    break;
                case Figure.B:
                    point.X = 400;
                    break;
                case Figure.b:
                    point.X = 400;
                    point.Y = 200;
                    break;
                case Figure.R:
                    point.X = 800;
                    break;
                case Figure.r:
                    point.X = 800;
                    point.Y = 200;
                    break;
                case Figure.Null:
                    return new Bitmap(200, 200);
            }

            var section = new Rectangle(point, new Size(200, 200));
            var bitmap = new Bitmap(section.Width, section.Height);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(new Bitmap(@"cp.png"), 0, 0, section, GraphicsUnit.Pixel);
            }

            return bitmap;
        }

        private void TB1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char) Keys.Enter)
            {
                Send(textBox1.Text);
                e.Handled = true;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            EngineProcess.Close();
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char) Keys.Enter)
            {
                if (textBox3.Text == "")
                {
                    if (cbEngine.Checked || cbEval.Checked)
                    {
                        Send("go depth " + DEPTH);
                        textBox3.Enabled = false;
                    }
                }
                else
                {
                    if (Turn)
                    {
                        textBox2.Text += "\r\n" + moveid + ". ";
                        moveid++;
                    }

                    Moves += textBox3.Text + " ";

                    textBox2.Text = cbMovesConsole.Checked ? Moves : Console;
                    Send("position fen " + FEN + " moves " + textBox3.Text);
                    Send("d");
                    if (cbEngine.Checked || cbEval.Checked)
                    {
                        Send("go depth " + DEPTH);
                        if (cbEngine.Checked) textBox3.Enabled = false;
                    }

                    textBox3.Text = "";
                }

                e.Handled = true;
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            DEPTH = trackBar1.Value;
            label2.Text = "Глубина: " + DEPTH;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            textBox2.Text = cbMovesConsole.Checked ? Moves : Console;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            textBox2.SelectionStart = textBox2.TextLength;
            textBox2.ScrollToCaret();
        }

        private void pb1_MouseClick(object sender, MouseEventArgs e)
        {
            var x = e.X / cellSize;
            var y = e.Y / cellSize;
            var letter = (char) (x + 97);
            var drawString = letter + (7 - y + 1).ToString();
            textBox3.Text += drawString;
            if (textBox3.Text.Length == 4) textBox3_KeyPress(sender, new KeyPressEventArgs((char) Keys.Enter));
        }

        private void label1_TextChanged(object sender, EventArgs e)
        {
            ThrowDice();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ThrowDice();
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            panel1.Visible = cbDice.Checked;
        }

        private void textBox3_Layout(object sender, LayoutEventArgs e)
        {
            textBox3.Focus();
        }
    }
}