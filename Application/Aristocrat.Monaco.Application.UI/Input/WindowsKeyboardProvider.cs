namespace Aristocrat.Monaco.Application.UI.Input
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using log4net;

    public class WindowsKeyboardProvider : IVirtualKeyboardProvider
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private Process _onScreenKeyboardProcess;

        public void CloseKeyboard()
        {
            if (_onScreenKeyboardProcess == null)
            {
                Logger.Warn("CloseOnScreenKeyboard - _onScreenKeyboardProcess == null, returning");
                return;
            }

            if (_onScreenKeyboardProcess.HasExited)
            {
                Logger.Warn("CloseOnScreenKeyboard - _onScreenKeyboardProcess.HasExited, returning");
                _onScreenKeyboardProcess = null;
                return;
            }

            try
            {
                _onScreenKeyboardProcess?.Kill();
            }
            catch (Win32Exception ex)
            {
                Logger.Error($"CloseOnScreenKeyboard Win32Exception: [{ex.ErrorCode}] {ex.Message} {ex.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"CloseOnScreenKeyboard Exception: {ex.Message} {ex.InnerException?.Message}");
            }
            finally
            {
                _onScreenKeyboardProcess = null;
            }
        }

        public void OpenKeyboard(object targetControl, CultureInfo culture)
        {
            CloseKeyboard();

            if (_onScreenKeyboardProcess != null)
            {
                Logger.Warn("OpenOnScreenKeyboard - Already open, returning");
                return;
            }

            try
            {
                // Get all running TabTip processes.
                var runningTabTipProcesses = Process.GetProcessesByName("tabtip");
                if (runningTabTipProcesses.Length != 0)
                {
                    foreach (var process in runningTabTipProcesses)
                    {
                        // Has this TabTip process exited?
                        if (process.HasExited)
                        {
                            continue;
                        }

                        // No, kill it. 
                        Logger.Debug($"OpenOnScreenKeyboard - {process.ProcessName} ID {process.Id} has not exited, killing");
                        process.Kill();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"OpenOnScreenKeyboard Exception: {ex.Message} {ex.InnerException?.Message}");
                return;
            }

            string path;
            try
            {
                path = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles) + @"\microsoft shared\ink\TabTip.exe";
            }
            catch (Exception ex)
            {
                Logger.Error($"OpenOnScreenKeyboard Exception: {ex.Message} {ex.InnerException?.Message}");
                return;
            }

            try
            {
                var processStartInfo = new ProcessStartInfo(path);
                _onScreenKeyboardProcess = Process.Start(processStartInfo);
            }
            catch (Win32Exception ex)
            {
                Logger.Error($"OpenOnScreenKeyboard Win32Exception: [{ex.ErrorCode}] {ex.Message} {ex.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                Logger.Error($"OpenOnScreenKeyboard Exception: {ex.Message} {ex.InnerException?.Message}");
            }
        }
    }
}
