using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Chess2.Chess;

namespace Chess2
{
    public partial class Form1 : Form
    {
        private Thread receiveThread;
        private const int PORT = 8888;
        static TcpClient client;
        static NetworkStream stream;
        private Bitmap boardBitmapW, boardBitmapB;
        private string ConsoleLog, Moves;
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
            boardBitmapW = new Bitmap(pb1.Width, pb1.Height);
            boardBitmapB = new Bitmap(pb1.Width, pb1.Height);
            pb1.Image = new Bitmap(pb1.Width, pb1.Height);
            g = Graphics.FromImage(pb1.Image);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            DrawBoards();

            ThrowDice();
            Render();
        }

        private void DrawBoards()
        {
            var graphicsboard = Graphics.FromImage(boardBitmapW);
            {
                for (var x = 0; x < 8; x++)
                for (var y = 0; y < 8; y++)
                {
                    graphicsboard.FillRectangle((x + y) % 2 == 0 ? Brushes.White : Brushes.MediumSeaGreen,
                        new Rectangle(x * cellSize, y * cellSize, cellSize, cellSize));

                    var letter = (char) (x + 97);
                    var drawString = letter + (8 - y).ToString();

                    var drawFont = new Font("Arial", 10);
                    var drawBrush = new SolidBrush(Color.Black);


                    graphicsboard.DrawString(drawString, drawFont, drawBrush, x * cellSize, y * cellSize);
                }
            }
            graphicsboard = Graphics.FromImage(boardBitmapB);
            {
                for (var x = 0; x < 8; x++)
                for (var y = 0; y < 8; y++)
                {
                    graphicsboard.FillRectangle((x + y) % 2 == 0 ? Brushes.White : Brushes.MediumSeaGreen,
                        new Rectangle(x * cellSize, y * cellSize, cellSize, cellSize));

                    var letter = (char) (104 - x);
                    var drawString = letter + (y + 1).ToString();


                    var drawFont = new Font("Arial", 10);
                    var drawBrush = new SolidBrush(Color.Black);


                    graphicsboard.DrawString(drawString, drawFont, drawBrush, x * cellSize, y * cellSize);
                }
            }
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
            g.DrawImage(checkBox1.Checked ? boardBitmapW : boardBitmapB, 0, 0);

            for (var i = 0; i < 8; i++)
            for (var j = 0; j < 8; j++)
            {
                var figureImage = SelectFigureImage(checkBox1.Checked ? board[j, i] : board[7 - j, 7 - i]);

                g.DrawImage(figureImage, i * cellSize, j * cellSize, cellSize, cellSize);
            }

            pb1.Refresh();
            GC.Collect();
        }
        

        private void Output(object sender, DataReceivedEventArgs e)
        {
            ConsoleLog += "\r\n" + e.Data;
            textBox2.Text = Moves;
            if (e.Data.Contains("Fen:"))
            {
                FEN = e.Data.Remove(0, 5);
                Render();
                SendMessage();

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
            using var g = Graphics.FromImage(bitmap);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(new Bitmap(@"cp.png"), 0, 0, section, GraphicsUnit.Pixel);

            return bitmap;
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
                }
                else
                {
                    if (Turn)
                    {
                        Moves += "\r\n" + moveid + ". ";
                        moveid++;
                    }

                    Moves += textBox3.Text + " ";

                    textBox2.Text = Moves;
                    Send("position fen " + FEN + " moves " + textBox3.Text);
                    Send("d");

                    textBox3.Text = "";
                }

                e.Handled = true;
            }
        }


        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            textBox2.SelectionStart = textBox2.TextLength;
            textBox2.ScrollToCaret();
        }

        private void pb1_MouseClick(object sender, MouseEventArgs e)
        {
            if (Turn != checkBox1.Checked) return;
            
            var x = e.X / cellSize;
            var y = e.Y / cellSize;
            if (!Turn)
            {
                x = 7 - x;
                y = 7 - y;
            }

            var letter = (char) (x + 97);
            var drawString = letter + (8 - y).ToString();
            textBox3.Text += drawString;
            if (textBox3.Text.Length == 4)
            {
                if (Turn)
                {
                    if (textBox3.Text[1] == '7' && textBox3.Text[3] == '8' &&
                        board[8 - Convert.ToInt32(textBox3.Text[1].ToString()), CharToNumber(textBox3.Text[0])] == 'P')
                    {
                        var form2 = new Form2();
                        form2.ShowDialog();
                        textBox3.Text += form2.Figure;
                    }
                }
                else
                {
                    if (textBox3.Text[1] == '2' && textBox3.Text[3] == '1' &&
                        board[8 - Convert.ToInt32(textBox3.Text[1].ToString()), CharToNumber(textBox3.Text[0])] == 'p')
                    {
                        var form2 = new Form2();
                        form2.ShowDialog();
                        textBox3.Text += form2.Figure;
                    }
                }
            }

            if (textBox3.Text.Length == 4 || textBox3.Text.Length == 5)
                textBox3_KeyPress(sender, new KeyPressEventArgs((char) Keys.Enter));
        }

        private int CharToNumber(char ch)
        {
            return ch - 97;
        }

        private void Move_TextChanged(object sender, EventArgs e)
        {
            if (cbDice.Checked) ThrowDice();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ThrowDice();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            checkBox1.Enabled = false;
            textBox4.Enabled = false;
            button2.Enabled = false;
            var IPAddress = textBox4.Text;
            client = new TcpClient();
            try
            {
                client.Connect(IPAddress, PORT);  
                stream = client.GetStream();   

                string message = checkBox1.Checked?"W":"B";
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);

                receiveThread = new Thread(ReceiveMessage);
                receiveThread.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
        void SendMessage()
        {
                string message = "Fen:" + FEN;
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
            
        }
        void ReceiveMessage()
        {
            while (true)
            {
                if (Turn == checkBox1.Checked) continue;
                try
                {
                    byte[] data = new byte[64];     
                    StringBuilder builder = new StringBuilder();
                    do
                    {
                        var bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    string message = builder.ToString();
                    if (message.Contains("Fen:"))
                    {
                        FEN = message.Remove(0, 4);
                        Render();
                    }
                }
                catch
                {
                    Disconnect();
                }
            }
        }

        void Disconnect()
        {
            stream?.Close(); 
            client?.Close(); 
            Environment.Exit(0);  
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            checkBox1.Text = checkBox1.Checked ? "Белые" : "Чёрные";
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