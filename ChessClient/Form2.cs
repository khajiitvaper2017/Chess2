using System;
using System.Windows.Forms;
using static Chess2.Chess;

namespace Chess2
{
    public partial class Form2 : Form
    {
        public char Figure = '\0';
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Figure = 'q';
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Figure = 'n';
            Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Figure = 'b';
            Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Figure = 'r';
            Close();
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Figure != '\0') return;
            Figure = 'q';
        }
    }
}
