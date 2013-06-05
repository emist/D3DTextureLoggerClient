using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;


namespace D3DTextureLoggerClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Program._interface.OutPutDir = Directory.GetCurrentDirectory() + "\\";
        }

        public void populateVertandPrimCount()
        {
            this.VertCountLabel.Text = Program._interface.vertCount;
            this.PrimCountLabel.Text = Program._interface.primCount;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(new ParameterizedThreadStart(Program.hook));
            t.Start(Program.exeName);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Program._interface.keys.Add(Keys.Up);
            Thread.Sleep(100);
            populateVertandPrimCount();
            totalPrims.Text = Convert.ToString(Program._interface.TotalPrims);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Program._interface.keys.Add(Keys.Down);
            Thread.Sleep(100);
            populateVertandPrimCount();
            totalPrims.Text = Convert.ToString(Program._interface.TotalPrims);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Program._interface.automatic = true;
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            Program._interface.saveprim = true;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Program._interface.clearprims = true;
            Thread.Sleep(100);
            this.PrimCountLabel.Text = "0";
            this.VertCountLabel.Text = "0";
            totalPrims.Text = Convert.ToString(Program._interface.TotalPrims);
        }

        private void UndisplayPrimButton_Click(object sender, EventArgs e)
        {
            Program._interface.display = false;
        }

    }
}
