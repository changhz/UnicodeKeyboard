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
                SendText(key);
        }

        private void SendKey(VirtualKeyShort key)
        {
            INPUT[] inputs = new INPUT[]
            {
                new INPUT { type = 1, U = new InputUnion { ki = new KEYBDINPUT { wVk = key } } }
            };
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        private void SendText(string text)
        {
            foreach (char c in text)
            {
                INPUT[] inputs = new INPUT[]
                {
                    new INPUT { type = 1, U = new InputUnion { ki = new KEYBDINPUT { wScan = c, dwFlags = KEYEVENTF.UNICODE } } }
                };
                SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
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
    }
}