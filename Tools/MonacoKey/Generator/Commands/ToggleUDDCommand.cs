namespace Generator.Commands
{
    using Generator.VMs;
    using System;
    using System.Collections.Generic;
    using System.Windows.Input;

    public class ToggleUDDCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private GeneratorVM VM;
        public ToggleUDDCommand(GeneratorVM vm)
        {
            VM = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            // not elegant, but works
            if (VM.UDDHeight[0] == 164)
            {
                VM.UDDHeight = new List<int> { 287, 41 };
            }
            else
            {
                VM.UDDHeight = new List<int> { 164, 164 };
            }
        }
    }
}
