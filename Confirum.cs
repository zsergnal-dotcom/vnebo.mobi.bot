using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace vnebo.mobi.bot.Library
{
    public partial class Confirum : Form
    {
        public Confirum(string lab1,string lab2, string txtBut, string url="")
        {
            InitializeComponent();
            label1.Text = lab1;
            richTextBox1.Text = lab2;
            button1.Text = txtBut;
            button1.DialogResult = DialogResult.OK;
            pictureBox1.Load(url);
        }
        public string code
        {
            get
            {
                return textBox1.Text;
            }
        }
    }
}
