using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace VirtualKeyboardOverlay
{
    public partial class MainWindow : Window
    {
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var hwnd = new WindowInteropHelper(this).Handle;
            int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            exStyle |= WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW;
            SetWindowLong(hwnd, GWL_EXSTYLE, exStyle);
        }

        const int GWL_EXSTYLE = -20;
        const int WS_EX_NOACTIVATE = 0x08000000;
        const int WS_EX_TOOLWINDOW = 0x00000080;

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public MainWindow()
        {
            InitializeComponent();
        }

        private void KeyButton_Click(object sender, RoutedEventArgs e)
        {
            var key = (sender as System.Windows.Controls.Button)?.Content.ToString();
            if (key == "Ctrl")
                SendKey(VirtualKeyShort.CONTROL);
            else if (key != null)
                SendChar(key[0]);
        }

        private void SendKey(VirtualKeyShort key)
        {
            INPUT[] inputs = new INPUT[]
            {
                new INPUT { type = 1, U = new InputUnion { ki = new KEYBDINPUT { wVk = key } } }
            };
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        private void SendChar(char c)
        {
            var hwnd = GetForegroundWindow();
            if (hwnd == IntPtr.Zero) 
                return;

            Debug.Write(c);

            const uint WM_UNICHAR = 0x0109;
            const uint WM_CHAR = 0x0102;

            // Try WM_UNICHAR first (allows full Unicode code points on supporting windows),
            // fallback to WM_CHAR (UTF-16 code unit).
            if (!PostMessage(hwnd, WM_UNICHAR, (IntPtr)c, IntPtr.Zero))
            {
                PostMessage(hwnd, WM_CHAR, (IntPtr)c, IntPtr.Zero);
            }
        }

        [DllImport("user32.dll")]
        static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            public uint type;
            public InputUnion U;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct InputUnion
        {
            [FieldOffset(0)] public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            public VirtualKeyShort wVk;
            public ushort wScan;
            public KEYEVENTF dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        enum VirtualKeyShort : ushort
        {
            CONTROL = 0x11,
            MENU = 0x12,
            DELETE = 0x2E
        }

        [Flags]
        enum KEYEVENTF : uint
        {
            UNICODE = 0x0004
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
    }
}