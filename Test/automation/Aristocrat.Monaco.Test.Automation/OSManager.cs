namespace Aristocrat.Monaco.Test.Automation
{
    using System.Diagnostics;

    public class OSManager
    {
        public static void ResetComputer()
        {
            var proc = new ProcessStartInfo
            {
                FileName = "cmd", WindowStyle = ProcessWindowStyle.Hidden, Arguments = "/C shutdown -r -t 0"
            };
            Process.Start(proc);
        }
    }
}