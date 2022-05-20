namespace Common.Utils
{
    using Common.Models;
    using log4net;
    using log4net.Core;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    public class ScriptRunner
    {
        public const string Powershell64bitPath = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe";
        private ILog Log;

        public ScriptRunner(ILog log)
        {
            Log = log;
        }

        public void CleanPartitionFormatAssignDriveLetter(USBKey key)
        {
            // the order and content of commands here may seem strange, but it is entirely intentional and necessary.

            uint diskIndex = key.DiskIndex;
            string VolumeLabel = "None";

            ScriptCommand clear = new ScriptCommand(Powershell64bitPath, new List<string>{ $"Clear-Disk -Number {diskIndex} -RemoveData -RemoveOEM -Confirm:$false" });
            RunCommand(clear, Level.Debug);

            key.ComputePartitionData(); // refresh partition data

            foreach (PartitionData PD in key.Partitions)
            {
                // Remove-Partition has parameter -PartitionNumber, which is a 1 based index, so add 1 here
                uint oneBasedIndex = PD.ZeroBasedIndex + 1;
                ScriptCommand removePartition = new ScriptCommand(Powershell64bitPath, new List<string>
                { $"Remove-Partition -DiskNumber {diskIndex} -PartitionNumber {oneBasedIndex} -confirm:$false" });

                RunCommand(removePartition, Level.Debug);
            }

            ScriptCommand initialize = new ScriptCommand(Powershell64bitPath,
                new List<string> { "Initialize-Disk -Number " + diskIndex + " -PartitionStyle GPT -Confirm:$false" });

            // if already initiailized, this does nothing, and that's ok
            RunCommand(initialize, Level.Debug);

            ScriptCommand newPartitionAndFormat = new ScriptCommand(Powershell64bitPath,
                new List<string> { $"New-Partition -DiskNumber {diskIndex} -UseMaximumSize -AssignDriveLetter | Format-Volume " +
                $"-FileSystem NTFS -NewFileSystemLabel '" + VolumeLabel + "' -Confirm:$false" });
            RunCommand(newPartitionAndFormat, Level.Debug);
        }

        public bool StopFileExplorerPopup()
        {
            ScriptCommand sc = new ScriptCommand(Powershell64bitPath, new List<string> { @"Stop-Service -Name ShellHWDetection" });
            return RunCommand(sc, Level.Debug);
        }
        public bool StartFileExplorerPopup()
        {
            ScriptCommand sc = new ScriptCommand(Powershell64bitPath, new List<string> { @"Start-Service -Name ShellHWDetection" });
            return RunCommand(sc, Level.Debug);
        }

        public bool RunCommand(ScriptCommand command, log4net.Core.Level logLevel)
        {
            try
            {
                Process p = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        Arguments = command.Arguments,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardInput = (command.Scripts != null),
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        Verb = "runas",
                        FileName = command.Executable,
                    }
                };
                p.Start();

                if (command.Scripts != null)
                {
                    foreach (string script in command.Scripts)
                        p.StandardInput.WriteLine(script);

                    p.StandardInput.Close();
                }

                // Log standard output
                string standardOutput = Environment.NewLine + "---------- Start of Process's Piped Standard Output ---------------" + Environment.NewLine;
                standardOutput += p.StandardOutput.ReadToEnd() + Environment.NewLine;
                standardOutput += "--------------- End of Process's Piped Standard Output ---------------";
                LoggingEventData outputLogEventData = new LoggingEventData
                {
                    Level = logLevel,
                    Message = standardOutput,
                    TimeStampUtc = DateTime.UtcNow
                };
                LoggingEvent outputLogEvent = new LoggingEvent(outputLogEventData);
                Log.Logger.Log(outputLogEvent);

                // Log Standard Error iff it's meaningful
                string tempSE = p.StandardError.ReadToEnd();
                if (tempSE != null && tempSE != "")
                {
                    string standardError = Environment.NewLine + "--------------- Start of Process's Piped Standard Error ---------------" + Environment.NewLine;
                    standardError += p.StandardError.ReadToEnd() + Environment.NewLine;
                    standardError += "--------------- End of Process's Piped Standard Error ---------------";
                    LoggingEventData errorLogEventData = new LoggingEventData
                    {
                        Level = logLevel,
                        Message = standardError,
                        TimeStampUtc = DateTime.UtcNow
                    };
                    LoggingEvent errorLogEvent = new LoggingEvent(errorLogEventData);
                    Log.Logger.Log(errorLogEvent);
                }

                p.WaitForExit();

                bool success = p.ExitCode == 0;

                if (!success)
                {
                    string allScripts = String.Join(Environment.NewLine, command.Scripts.Select(a => String.Join(" ", a)));
                    Log.Debug($"Script execution returned a non-zero error code. Returned: " + p.ExitCode);
                }

                return success;
            }
            catch (Exception e)
            {
                Log.Error("Exception encountered... bailing on execution...");
                Log.Error(e.Message);
                Log.Error(e.StackTrace);
            }

            return false;
        }
    }
}
