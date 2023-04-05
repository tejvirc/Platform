namespace Platform
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Windows;

    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main
    {
        private Process? _process;

        public Main()
        {
            InitializeComponent();
        }

        protected override void OnClosed(EventArgs e)
        {
            _process?.Kill(true);
        }

        private void WinHostCtrl_OnLoaded(object sender, RoutedEventArgs e)
        {
            var handle = GameBottomWindowCtrl.Handle;

            var assembly = Assembly.GetEntryAssembly();
            Debug.Assert(assembly != null);

            var binPath = Path.GetDirectoryName(assembly.Location);
            Debug.Assert(binPath != null);

            var startInfo = new ProcessStartInfo
            {
                CreateNoWindow = false,
                Arguments = handle.ToString(),
                FileName = Path.Combine(binPath, "Lobby.exe"),
                WorkingDirectory = binPath,
                UseShellExecute = false,
                ErrorDialog = false
            };

            _process = Process.Start(startInfo);
            if (_process == null)
            {
                throw new InvalidOperationException($"Failed to start game: {startInfo.FileName} {startInfo.Arguments}");
            }

            _process.EnableRaisingEvents = true;
            _process.Exited += ProcessExited;
        }

        private static void ProcessExited(object? sender, EventArgs e)
        {
            if (sender is not Process process)
            {
                return;
            }

            process.Exited -= ProcessExited;
        }
    }
}
