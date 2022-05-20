namespace Generator.Commands
{
    using VMs;
    using System;
    using System.IO;
    using System.Windows.Input;

    public class ViewScriptCommand : ICommand
    {
        protected GeneratorVM vm;

        public ViewScriptCommand(GeneratorVM VM)
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
            try
            {
                string filePath = "temp_" + vm.SelectedCommand.Name +".txt";
                FileStream stream = File.Create(filePath);
                stream.Close();
                File.WriteAllText(filePath, vm.SelectedCommand.Script);
                System.Diagnostics.Process.Start(filePath);
                App.Log.Debug("Opening script: " + vm.SelectedCommand.Name);
            }
            catch (Exception e)
            {
                App.Log.Error("Exception caught while trying to view script: " + vm.SelectedCommand.Name);
                App.Log.Error(e.Message);
                App.Log.Error(e.StackTrace);
            }
        }
    }
}
