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
            var foreground = GetForegroundWindow();
            if (foreground == IntPtr.Zero)
                return;

            // Prefer sending to the focused child control of the foreground window (e.g. an edit box)
            IntPtr target = foreground;
            uint threadId = GetWindowThreadProcessId(foreground, out _);
            GUITHREADINFO gui = new GUITHREADINFO { cbSize = Marshal.SizeOf(typeof(GUITHREADINFO)) };
            if (threadId != 0 && GetGUIThreadInfo(threadId, ref gui) && gui.hwndFocus != IntPtr.Zero)
            {
                target = gui.hwndFocus;
            }

            Debug.Write(c);

            const uint WM_UNICHAR = 0x0109;
            const uint WM_CHAR = 0x0102;

            uint codepoint = c; // BMP character; for supplementary code points you'd need surrogate handling

            // Build a minimal lParam: repeat count = 1 (low word). Many controls don't require full scan-code details.
            IntPtr lParam = MakeLParamForChar();

            // Try WM_UNICHAR first (supports full Unicode on supporting windows),
            // fallback to WM_CHAR (UTF-16 code unit) posted to the control that actually has focus.
            if (!PostMessage(target, WM_UNICHAR, (IntPtr)codepoint, lParam))
            {
                PostMessage(target, WM_CHAR, (IntPtr)codepoint, lParam);
            }
        }

        private static IntPtr MakeLParamForChar()
        {
            // Low word: repeat count = 1
            int repeat = 1 & 0xFFFF;
            return (IntPtr)repeat;
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

        // New/added helpers to find the focused child control in the foreground thread
        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetGUIThreadInfo(uint idThread, ref GUITHREADINFO lpgui);

        [StructLayout(LayoutKind.Sequential)]
        struct GUITHREADINFO
        {
            public int cbSize;
            public uint flags;
            public IntPtr hwndActive;
            public IntPtr hwndFocus;
            public IntPtr hwndCapture;
            public IntPtr hwndMenuOwner;
            public IntPtr hwndMoveSize;
            public IntPtr hwndCaret;
            public RECT rcCaret;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
    }
}