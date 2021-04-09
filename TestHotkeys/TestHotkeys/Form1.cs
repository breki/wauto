using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestHotkeys
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            
            // GlobalHotKey.RegisterHotKey("Alt + Shift + S", DoSomething);
            GlobalHotKey.RegisterHotKey("Win + A", DoSomethingElse);
        }

        private void DoSomething()
        {
            MessageBox.Show("Hello World", "Hello World");
        }

        private void DoSomethingElse()
        {
            MessageBox.Show("Notepad, eh?", "Notepad, eh?");
        }
    }
}