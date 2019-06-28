using System;
using System.Windows.Forms;

namespace Game2048
{
    public partial class Form2 : Form
    {
        Form main = Application.OpenForms[0];

        public Form2()
        {
            InitializeComponent();
            textBox1.SetWatermark("Имя игрока");
            textBox1.Text = "";
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e) => main.Enabled = true;

        private void Buttons_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) => e.IsInputKey = true;

        private void button1_Click(object sender, EventArgs e) => Close();

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
                errorProvider1.SetError(textBox1, "Введите ваше имя");
            else
            {
                queries.Input((int)Tag, textBox1.Text, (main as Form1).GetScore(), (main as Form1).GetMoves(), (main as Form1).MaxBest());
                if ((int)Tag == 0)
                {
                    if (Owner != null) (Owner as Form3).Button1_Click(Owner, new EventArgs());
                }
                else 
                    if (Owner != null) (Owner as Form3).Button2_Click(Owner, new EventArgs());
                Close();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e) => errorProvider1.Clear();
    }
}
