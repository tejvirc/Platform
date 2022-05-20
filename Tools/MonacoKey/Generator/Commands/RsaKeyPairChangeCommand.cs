namespace Generator.Commands
{
    using VMs;
    using System;
    using System.Windows.Input;

    public class RsaKeyPairChangeCommand : ICommand
    {
        protected GeneratorVM vm;

        public RsaKeyPairChangeCommand(GeneratorVM VM)
        {
            vm = VM;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            // This revalidates the USB Keys against the currently selected RSA Key Pairr, by rediscovering the USB Keys.
            vm.GUIEnabled = false;
            await vm.UpdateAsync();
            vm.GUIEnabled = true;
        }
    }
}
