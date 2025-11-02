using System.Runtime.InteropServices;
using System.Windows;

namespace UnicodeKeyboard
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void KeyButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as System.Windows.Controls.Button;
            if (btn == null) return;

            string? symbol = btn.Content?.ToString();
            if (string.IsNullOrEmpty(symbol)) return;

            Clipboard.SetText(symbol);

            this.Hide();

            Thread.Sleep(50);

            const byte VK_CONTROL = 0x11;
            const byte VK_V = 0x56;
            const uint KEYEVENTF_KEYUP = 0x0002;

            keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);
            keybd_event(VK_V, 0, 0, UIntPtr.Zero);
            keybd_event(VK_V, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

            this.Show();
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
    }
}