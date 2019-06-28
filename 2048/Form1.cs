using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;

namespace Game2048
{
    public partial class Form1 : Form
    {
        private Random rnd = new Random();
        private const int s = 4; // отвечает за масштаб [4, 10]
        private int w = 0, h = 0;
        
        private Brush[] palette = 
        {
            Brushes.LightGray,                              //0
            new SolidBrush(Color.FromArgb(255, 255, 255)),  //2
            new SolidBrush(Color.FromArgb(245, 235, 220)),  //4
            new SolidBrush(Color.FromArgb(245, 210, 180)),  //8
            new SolidBrush(Color.FromArgb(240, 170, 130)),  //16
            new SolidBrush(Color.FromArgb(250, 120, 90)),   //32
            new SolidBrush(Color.FromArgb(255, 100, 90)),   //64
            new SolidBrush(Color.FromArgb(255, 170, 80)),   //128
            new SolidBrush(Color.FromArgb(245, 200, 100)),  //256
            new SolidBrush(Color.FromArgb(250, 220, 70)),   //512
            new SolidBrush(Color.FromArgb(255, 240, 40)),   //1024
            new SolidBrush(Color.FromArgb(255, 255, 55)),   //2048
            new SolidBrush(Color.FromArgb(195, 255, 65)),   //4096
            new SolidBrush(Color.FromArgb(135, 255, 75)),   //8192
            new SolidBrush(Color.FromArgb(75, 255, 100)),   //16384
            new SolidBrush(Color.FromArgb(65, 255, 150)),   //32768
            new SolidBrush(Color.FromArgb(55, 195, 200)),   //65536
            new SolidBrush(Color.FromArgb(45, 75, 255))     //131072
        };

        private int[,] heft =
        {
            {15, 14, 13, 12},
            {8, 9, 10, 11},
            {7, 6, 5, 4},
            {0, 1, 2, 3}
        };

        private int currentMode = -1;
        private int[,] mass = new int[4, 4];
        private int score = 0;
        private CancellationTokenSource cts;
        private Form achievements;

        public Form1()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 0;
        }

        private void Painting()
        {
            Graphics g = Graphics.FromImage(pictureBox1.Image);

            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                {
                    g.FillRectangle(palette[mass[i, j] % palette.Length], s + j * (w - s) / 4, s + i * (h - s) / 4,
                        (w - s) / 4 - s, (h - s) / 4 - s);

                    if (mass[i, j] > 0)
                        g.DrawString(Math.Pow(2, mass[i, j]).ToString().PadLeft(6,' '), 
                            new Font("Tahoma", h / 20), Brushes.Black, 
                            s / 2 + j * (w - s) / 4, h / 12 + i * (h - s) / 4);
                }

            g.Dispose();

            pictureBox1.Invoke(new Action(() => pictureBox1.Refresh()));
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            w = h = pictureBox1.Width = pictureBox1.Height = 56 * s + 1;
            Size = new Size(2 * pictureBox1.Location.X + h + 15, pictureBox1.Location.Y + pictureBox1.Location.X + w + 36);
            pictureBox1.Image = new Bitmap(w, h);
            pictureBox1.BackColor = SystemColors.ControlDarkDark;
            Painting();

            textBox1.Text = "0";
            textBox2.Text = "0";
            score = 0;
            pictureBox1.Enabled = false;
        }

        
        private void Button1_Click(object sender, EventArgs e)
        {
            if (cts != null)
            {
                cts.Cancel();
                Thread.Sleep(200);
            }

            Form1_Load(this, new EventArgs());
            currentMode = comboBox1.SelectedIndex;

            mass = new int[4, 4];
            {
                int x, y;
                do
                {
                    x = rnd.Next(4);
                    y = rnd.Next(4);
                } while (mass[x, y] > 0);

                if (rnd.Next(1100) < 1000) mass[x, y] = 1;
                else mass[x, y] = 2;
            }

            switch (currentMode)
            {
                case 0:
                    {
                        int x, y;
                        do
                        {
                            x = rnd.Next(4);
                            y = rnd.Next(4);
                        } while (mass[x, y] > 0);

                        if (rnd.Next(1100) < 1000) mass[x, y] = 1;
                        else mass[x, y] = 2;
                        break;
                    }
                case 1:
                    {
                        pictureBox1.Enabled = true;
                        break;
                    }
                case 2:
                    {
                        cts = new CancellationTokenSource();
                        Task.Run(() => Autonomic(cts.Token), cts.Token);
                        break;
                    }
            }
            Painting();
        }

        private void Autonomic(CancellationToken ct)
        {
            while (!Fullness())
            {
                if (ct.IsCancellationRequested) break;

                int x, y;
                do
                {
                    x = rnd.Next(4);
                    y = rnd.Next(4);
                } while (mass[x, y] > 0);

                MouseEventArgs m;
                if (rnd.Next(1100) < 1000)
                    m = new MouseEventArgs(MouseButtons.Left, 1, 2 * s + y * (w - s) / 4, 2 * s + x * (h - s) / 4, 0);
                else m = new MouseEventArgs(MouseButtons.Right, 1, 2 * s + y * (w - s) / 4, 2 * s + x * (h - s) / 4, 0);

                PictureBox1_MouseClick(pictureBox1, m);
                Thread.Sleep(150);
            }
        }

        private bool Fullness()
        {
            int points = 0;
            int[,] arr = (int[,])mass.Clone();
            points += Swipe(arr, true);
            points += Swipe(arr, false);
            points += Swipe(arr, false, false);
            points += Swipe(arr, true, false);
            if (points == 0) return true;
            else return false;
        }

        private void Buttons_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) => e.IsInputKey = true;

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (comboBox1.Focused && currentMode >= 0) button1.Focus();

            int points = 0;
            if (currentMode == 0)
            {
                switch (e.KeyCode)
                {
                    case Keys.Up:
                        {
                            points = Swipe(mass, true);
                            break;
                        }
                    case Keys.Left:
                        {
                            points = Swipe(mass, false);
                            break;
                        }
                    case Keys.Right:
                        {
                            points = Swipe(mass, false, false);
                            break;
                        }
                    case Keys.Down:
                        {
                            points = Swipe(mass, true, false);
                            break;
                        }
                }
                if (points > 0)
                {
                    Painting();
                    Thread.Sleep(300);

                    textBox1.Text = (int.Parse(textBox1.Text) + 1).ToString();
                    textBox1.Refresh();
                    if (points > 1) score += points;
                    textBox2.Text = score.ToString();
                    textBox2.Refresh();

                    int x, y;
                    do
                    {
                        x = rnd.Next(4);
                        y = rnd.Next(4);
                    } while (mass[x, y] > 0);

                    if (rnd.Next(1100) < 1000) mass[x, y] = 1;
                    else mass[x, y] = 2;

                    Painting();
                    Win2048();
                }
            }
        }

        /// <summary>
        /// <para>Смахивание всех плиток игрового поля (array - массив 4х4) в одну из 4 сторон.</para>
        /// line - направление, где true - вертикальное, а false - горизонтальное;
        /// order - порядок, где true - прямой, а false - обратный.
        /// </summary>
        private int Swipe(int [,] array, bool line, bool order = true)
        {

            int primary = order ? 1 : 2,
                step = order ? 1 : -1,
                result = 0;

            for (int k = 0; k < 3; k++)
            {
                for (int i = (line ? primary : 0);
                    i >= 0 && i <= 3;
                    i += (line ? step : 1))
                {
                    for (int j = (line ? 0 : primary);
                        j >= 0 && j <= 3;
                        j += (line ? 1 : step))
                    {
                        if (array[i - (line ? step : 0), j - (line ? 0 : step)] == 0 
                            && array[i, j] > 0)
                        {
                            int tmp = array[i - (line ? step : 0), j - (line ? 0 : step)];
                            array[i - (line ? step : 0), j - (line ? 0 : step)] = array[i, j];
                            array[i, j] = tmp;
                            result = 1;
                        }
                    }
                }
            }

            for (int i = (line ? primary : 0);
                i >= 0 && i <= 3;
                i += (line ? step : 1))
            {
                for (int j = (line ? 0 : primary);
                    j >= 0 && j <= 3;
                    j += (line ? 1 : step))
                {
                    if (array[i - (line ? step : 0), j - (line ? 0 : step)] == array[i, j] 
                        && array[i, j] > 0)
                    {
                        result += (int)Math.Pow(2, ++array[i - (line ? step : 0), j - (line ? 0 : step)]);
                        array[i, j] = 0;
                    }
                }
            }

            for (int i = (line ? primary : 0);
                i >= 0 && i <= 3;
                i += (line ? step : 1))
            {
                for (int j = (line ? 0 : primary);
                    j >= 0 && j <= 3;
                    j += (line ? 1 : step))
                {
                    if (array[i - (line ? step : 0), j - (line ? 0 : step)] == 0
                        && array[i, j] > 0)
                    {
                        int tmp = array[i - (line ? step : 0), j - (line ? 0 : step)];
                        array[i - (line ? step : 0), j - (line ? 0 : step)] = array[i, j];
                        array[i, j] = tmp;
                    }
                }
            }

            if (result % 2 == 1 && result > 2) --result;
            return result;
        }

        private void PictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            int x = 0, y = 0;
            if (e.X < w - s && e.X % ((w - s) / 4) > s && e.Y < h - s && e.Y % ((h - s) / 4) > s)
            {
                x = 4 * e.X / (w - s);
                y = 4 * e.Y / (h - s);
                if (mass[y, x] == 0)
                {
                    textBox1.Invoke(new Action(() =>
                    {
                        textBox1.Text = (int.Parse(textBox1.Text) + 1).ToString();
                        textBox1.Refresh();
                    })); 

                    if (e.Button == MouseButtons.Left) mass[y, x] = 1;
                    else if (e.Button == MouseButtons.Right) mass[y, x] = 2;

                    Painting();
                    Thread.Sleep(100);

                    double max = -1;
                    int direction = 0;
                    for (int dir = 4; dir > 0; dir--)   // 4 - down, 3 - right, 2 - left, 1 - up
                    {
                        int[,] arr = (int[,])mass.Clone();
                        if (Swipe(arr, (dir % 3) % 2 == 1, (dir - 1) / 2 == 0) > 0)
                        {
                            double locmax = ExpectationMax(arr, Depth(arr));
                            if (locmax >= max)
                            {
                                max = locmax;
                                direction = dir;
                            }
                        }
                    }

                    int points = Swipe(mass, (direction % 3) % 2 == 1, (direction - 1) / 2 == 0);
                    Painting();

                    if (points > 1) score += points;

                    textBox2.Invoke(new Action(() =>
                    {
                        textBox2.Text = score.ToString();
                        textBox2.Refresh();
                    }));

                    Win2048();
                }
            }
        }

        private double ExpectationMax(int[,] array, int depth)
        {
            double evalution = 0, probably = 0;
            if (depth == 0)
            {
                for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 4; j++)
                        if (array[i, j] != 0)
                            evalution += (int)Math.Pow(2, array[i, j] + heft[i, j]);
            }
            else
            {
                for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 4; j++)
                        if (array[i, j] == 0)
                        {
                            double max = 0, probnum;
                            int direction = 0;

                            int[,] arr = (int[,])array.Clone();
                            if (rnd.Next(1100) < 1000)
                            {
                                arr[i, j] = 1;
                                probnum = 10 / 11.0;
                            }
                            else
                            {
                                arr[i, j] = 2;
                                probnum = 1 / 11.1;
                            }

                            for (int dir = 4; dir > 0; dir--)
                            {
                                int[,] arr3 = (int[,])arr.Clone();
                                if (Swipe(arr3, (dir % 3) % 2 == 1, (dir - 1) / 2 == 0) > 0)
                                {
                                    double locmax = ExpectationMax(arr3, depth - 1);
                                    if (locmax >= max)
                                    {
                                        max = locmax;
                                        direction = dir;
                                    }
                                }
                            }

                            if (direction != 0)
                                evalution += probnum * max;
                            else
                                evalution += probnum * ExpectationMax(arr, 0);
                            probably += probnum;
                        }
                if (probably != 0) evalution /= probably;
            }
            return evalution;
        }

        private int Depth(int[,] array)
        {
            int countNull = 0;
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    if (array[i, j] == 0) countNull++;
            if (countNull > 11) return 2;
            else if (countNull > 5) return 3;
            else return 4;
        }

        private void Win2048()
        {
            bool result = false;
            foreach (int now in mass)
            {
                if (now >= 11) result = true;
            }

            bool f = Fullness();
            switch (currentMode)
            {
                case 0:
                    if (result)
                    {
                        pictureBox1.BackColor = Color.Green;
                        if (f)
                        {
                            Form form2 = new Form2 { Owner = achievements, TopMost = true, Tag = currentMode };
                            form2.Show();
                            form2.Location = new Point(Location.X + 1, Location.Y + 31);
                            Enabled = false;
                            currentMode = -1;
                        }
                    }
                    else if (f)
                    {
                        currentMode = -1;
                        Graphics g = Graphics.FromImage(pictureBox1.Image);
                        g.FillRectangle(new SolidBrush(Color.FromArgb(80, Color.Yellow)), 0, 0, pictureBox1.Width, pictureBox1.Height);
                        g.DrawString("You Lose!", new Font("Tahoma", 30), new SolidBrush(Color.White), pictureBox1.Width / 2 - 90, pictureBox1.Height / 2 - 30);
                        pictureBox1.Refresh();
                    }
                    break;
                case 1:
                    if (result)
                    {
                        currentMode = -1;
                        Graphics g = Graphics.FromImage(pictureBox1.Image);
                        g.FillRectangle(new SolidBrush(Color.FromArgb(80, Color.Yellow)), 0, 0, pictureBox1.Width, pictureBox1.Height);
                        g.DrawString("You Lose!", new Font("Tahoma", 30), new SolidBrush(Color.White), pictureBox1.Width / 2 - 90, pictureBox1.Height / 2 - 30);
                        pictureBox1.Refresh();
                        pictureBox1.Enabled = false;
                    }
                    else if (f)
                    {
                        Form form2 = new Form2 { Owner = achievements, TopMost = true, Tag = currentMode };
                        form2.Show();
                        form2.Location = new Point(Location.X + 1, Location.Y + 31);
                        Enabled = false;
                        currentMode = -1;
                    }
                    break;
                case 2:
                    if (result)
                    {
                        pictureBox1.BackColor = Color.Green;
                        if (f) currentMode = -1;
                    }
                    else if (f)
                    {
                        currentMode = -1;
                        Graphics g = Graphics.FromImage(pictureBox1.Image);
                        g.FillRectangle(new SolidBrush(Color.FromArgb(80, Color.Yellow)), 0, 0, pictureBox1.Width, pictureBox1.Height);
                        g.DrawString("Lose!", new Font("Tahoma", 30), new SolidBrush(Color.White), pictureBox1.Width / 2 - 50, pictureBox1.Height / 2 - 30);
                        pictureBox1.Invoke(new Action(() => pictureBox1.Refresh()));
                    }
                    break;
            }
        }

        public int GetScore() => score;
        
        public int GetMoves() => int.Parse(textBox1.Text);

        public int MaxBest() => (int)Math.Pow(2, mass.Cast<int>().Max());

        private void Button2_Click(object sender, EventArgs e)
        {
            achievements = new Form3 { Height = this.Height };
            achievements.Show();
            achievements.Location = new Point(Location.X + Width - 8 , Location.Y);
            button2.Enabled = false;
        }

        public void EnabledButton() => button2.Enabled = true;
    }
}
