using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace scandiag
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            rk.SetValue("scandiag", Application.ExecutablePath);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            const int WM_SYSKEYDOWN = 0x0104;
            const int VK_F4 = 0x73; 

            if ((msg.Msg == WM_SYSKEYDOWN) && (keyData == (Keys.Alt | Keys.F4)))
            {
                return true; // Impede o fechamento do aplicativo
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
