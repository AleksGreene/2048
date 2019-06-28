using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Game2048
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            Button1_Click(this, new EventArgs());
            Width = 600;
            dataGridView1.Columns[0].HeaderText = "№";
        }

        private void Form3_FormClosed(object sender, FormClosedEventArgs e) => (Application.OpenForms[0] as Form1).EnabledButton();

        private void Form3_SizeChanged(object sender, EventArgs e) => dataGridView1.Size = new Size(Width - 38, Height - 92);

        private void DataGridView1_SizeChanged(object sender, EventArgs e) => dataGridView1.Columns[0].Width = 30;

        public void Button1_Click(object sender, EventArgs e)
        {
            outputTableAdapter.Fill(baseDataSet.Output, 0);
            button1.Focus();
            dataGridView1.Sort(dataGridView1.Columns[0], ListSortDirection.Ascending);
        }

        public void Button2_Click(object sender, EventArgs e)
        {
            outputTableAdapter.Fill(baseDataSet.Output, 1);
            button2.Focus();
        }
    }
}
