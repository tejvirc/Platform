namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Input;
    using Aristocrat.Monaco.Application.Contracts.OperatorMenu;
    using Aristocrat.Monaco.Hardware.Contracts.Ticket;
    using MVVM.Command;
    using OperatorMenu;

    /// <summary>
    ///     A <see cref="OptionsPageViewModel" /> contains the logic for options page.
    /// </summary>
    [CLSCompliant(false)]
    public class OptionsPageViewModel : OperatorMenuMultiPageViewModelBase
    {
        private const string MenuExtensionPointPath = "/Application/OperatorMenu/OptionsMenu";

        /// <summary>
        ///     Initializes a new instance of the <see cref="OptionsPageViewModel" /> class.
        /// </summary>
        public OptionsPageViewModel(string displayPageTitle) : base(displayPageTitle, MenuExtensionPointPath)
        {
            VolumeControlIsVisible = GetConfigSetting(OperatorMenuSetting.VolumeControlVisible, false);
            VolumeViewModel = new VolumeViewModel { InputEnabled = true };

            OutOfServiceButtonIsVisible = GetConfigSetting(OperatorMenuSetting.OutOfServiceVisible, false);
            OutOfServiceViewModel = new OutOfServiceViewModel();

            PrintVerificationButtonIsVisible = GetConfigSetting(OperatorMenuSetting.MainButtonPrintVerificationVisible, true);
            PrintVerificationButtonClickedCommand = new ActionCommand<object>(_ => Print(OperatorMenuPrintData.Custom1));
        }

        // This needs to be false to allow printing from this page
        protected override bool IsContainerPage => false;

        public bool VolumeControlIsVisible { get; }

        public VolumeViewModel VolumeViewModel { get; }

        public bool OutOfServiceButtonIsVisible { get; }

        public OutOfServiceViewModel OutOfServiceViewModel { get; }

        public bool PrintVerificationButtonIsVisible { get; }

        public ICommand PrintVerificationButtonClickedCommand { get; }

        protected override void OnLoaded()
        {
            VolumeViewModel.OnLoaded();
            OutOfServiceViewModel.OnLoaded();

            base.OnLoaded();
        }

        protected override void OnUnloaded()
        {
            VolumeViewModel.OnUnloaded();
            base.OnUnloaded();
        }

        protected override IEnumerable<Ticket> GenerateTicketsForPrint(OperatorMenuPrintData dataType)
        {
            IEnumerable<Ticket> tickets = null;

            switch (dataType)
            {
                case OperatorMenuPrintData.Custom1:
                    tickets = GeneratePrintVerificationTickets();
                    break;
            }

            return tickets;
        }
    }
}