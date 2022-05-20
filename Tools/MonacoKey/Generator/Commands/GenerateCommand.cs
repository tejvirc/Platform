namespace Generator.Commands
{
    using Common.Models;
    using Common.Utils;
    using Generator.VMs;
    using System;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public class GenerateCommand : ICommand
    {
        protected GeneratorVM vm;

        public GenerateCommand(GeneratorVM VM)
        {
            vm = VM;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            Command sc = vm.SelectedCommand;

            if (vm.EnabledUsbKeys().Count == 0)
            {
                App.Log.Info("No USBs selected...");
                return;
            }

            if (sc == null)
            {
                App.Log.Info("No command selected...");
                return;
            }

            if(vm.RsaService.SelectedKeyPair == null)
            {
                App.Log.Info("No command type selected... You must select a type to generate keys. Use 'Retail' for production ready keys.");
                return;
            }

            Task.Run(() =>
            {
                vm.GUIEnabled = false;
                App.Log.Info("Generating...");

                string summaryMessage = "\t----------------------- Results --------------------------" + Environment.NewLine +
                "\t\t\tDisk\tStatus\t\tCommmand" + Environment.NewLine;

                bool everyUsbWorked = true;

                foreach (USBKey key in vm.EnabledUsbKeys())
                {
                    if (key.Format)
                    {
                        App.Log.Info("Formatting Disk " + key.DiskIndex + "... please wait, this may take a minute or two.");
                        ScriptRunner runnah = new ScriptRunner(App.Log);
                        runnah.CleanPartitionFormatAssignDriveLetter(key);

                        // if we format, we don't have the correct partition data in memory
                        key.ComputePartitionData();
                    }

                    if (Validator.WriteCommandToUSB(sc, key, vm.RsaService))
                    {
                        summaryMessage += "\t\t\t" + key.DiskIndex + "\tSUCCESS\t\t" + sc.Name + Environment.NewLine;
                    }
                    else
                    {
                        everyUsbWorked = false;
                        summaryMessage += "\t\t\t" + key.DiskIndex + "\tFAILURE\t\t" + Environment.NewLine;
                    }
                }

                summaryMessage += "\t\t\t----------------------- Complete -----------------------";

                if (everyUsbWorked)
                    App.Log.Info(summaryMessage);
                else
                    App.Log.Error(summaryMessage);

                vm.UpdateAsync();
                vm.GUIEnabled = true;
            });
        }
    }
}
