namespace Aristocrat.Monaco.Test.KeyConverter
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public class KeyboardHookStruct
    {
        public int KeyCode { get; set; }

        public int ScanCode { get; set; }

        public int KeySetting { get; set; }

        public int Time { get; set; }

        public int ExtraInfo { get; set; }
    }
}