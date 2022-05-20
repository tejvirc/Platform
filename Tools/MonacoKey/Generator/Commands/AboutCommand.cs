namespace Generator.Commands
{
    using Generator.VMs;
    using MahApps.Metro.Controls.Dialogs;
    using System;
    using System.Windows.Input;

    public class AboutCommand : ICommand
    {
        GeneratorVM VM;

        public AboutCommand(GeneratorVM vm)
        {
            VM = vm;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            string message = "The Platform Key Generator is designed to create multiple USB keys at once. Simply insert USB drives " +
                "into your computer, and they will populate in the USB Drives section. If you would like to make the USB drives into USB keys, then pick your desired command in the Command " +
                "Selection section, then click Generate." + Environment.NewLine + Environment.NewLine+ Environment.NewLine +
                @"If you need more help, see the confluence page. The link has automatically been copied to your clipboard. " +
                "Just go to a browser and paste it (ctrl + v). Or type this link: https://confy.aristocrat.com/pages/viewpage.action?pageId=77083955" + 
                Environment.NewLine + Environment.NewLine + Environment.NewLine +
                "This app was developed by Alexander Yozzo, under the direction of James Helm.";
            string title = "About";

            System.Windows.Clipboard.SetText(@"https://confy.aristocrat.com/pages/viewpage.action?pageId=77083955");
            DialogCoordinator.Instance.ShowMessageAsync(VM, title, message);
        }
    }
}
