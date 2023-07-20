namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Input;
    using Aristocrat.Toolkit.Mvvm.Extensions;
    using CommunityToolkit.Mvvm.Input;
    using Contracts;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Events;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Monaco.Localization.Properties;
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
        public OptionsPageViewModel(string pageNameResourceKey) : base(pageNameResourceKey, MenuExtensionPointPath)
        {
            VolumeControlIsVisible = GetConfigSetting(OperatorMenuSetting.VolumeControlVisible, false);
            VolumeViewModel = new VolumeViewModel { InputEnabled = true };

            OutOfServiceButtonIsVisible = GetConfigSetting(OperatorMenuSetting.OutOfServiceVisible, false);
            OutOfServiceViewModel = new OutOfServiceViewModel();

            PrintVerificationButtonIsVisible = GetConfigSetting(OperatorMenuSetting.MainButtonPrintVerificationVisible, true);
            PrintVerificationButtonClickedCommand = new RelayCommand<object>(_ => Print(OperatorMenuPrintData.Custom1));
        }

        // This needs to be false to allow printing from this page
        protected override bool IsContainerPage => false;

        public bool VolumeControlIsVisible { get; }

        public VolumeViewModel VolumeViewModel { get; }

        public bool OutOfServiceButtonIsVisible { get; }

        public OutOfServiceViewModel OutOfServiceViewModel { get; }

        public bool PrintVerificationButtonIsVisible { get; }

        public ICommand PrintVerificationButtonClickedCommand { get; }

        private IDisableByOperatorManager DisableByOperatorManager
            => ServiceManager.GetInstance().GetService<IDisableByOperatorManager>();

        protected override void OnLoaded()
        {
            if (DisableByOperatorManager.DisabledByOperator)
            {
                HandleSystemDisabledByOperatorEvent(false);
            }

            EventBus?.Subscribe<SystemEnabledByOperatorEvent>(this, _ => HandleSystemDisabledByOperatorEvent(true));
            EventBus?.Subscribe<SystemDisabledByOperatorEvent>(this, _ => HandleSystemDisabledByOperatorEvent(false));

            VolumeViewModel.OnLoaded();
            OutOfServiceViewModel.OnLoaded();

            base.OnLoaded();
        }

        protected override void OnUnloaded()
        {
            VolumeViewModel.OnUnloaded();
            OutOfServiceViewModel.OnUnloaded();

            base.OnUnloaded();
        }

        protected override void OnInputStatusChanged()
        {
            var active = true;
            var text = string.Empty;
            switch (AccessRestriction)
            {
                case OperatorMenuAccessRestriction.InGameRound:
                    text = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EndGameRoundBeforeOutOfServiceText);
                    active = false;
                    break;
                case OperatorMenuAccessRestriction.ZeroCredits:
                    // Override the Zero-Credits requirement if this property is set.
                    if (!(bool)PropertiesManager.GetProperty(
                        ApplicationConstants.MachineSetupConfigEnterOutOfServiceWithCreditsEnabled,
                        false))
                    {
                        text = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EnteringOutOfServiceModeRequiresZeroCreditsText);
                        active = false;
                    }
                    break;
            }

            OutOfServiceViewModel.OutOfServiceModeButtonIsEnabled = active;
            InputStatusText = text;

            if (!active && PopupOpen)
            {
                Execute.OnUIThread(
                    () =>
                    {
                        EventBus.Publish(new OperatorMenuPopupEvent(false, string.Empty));
                    });
            }
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

        private void HandleSystemDisabledByOperatorEvent(bool enabled)
        {
            InputStatusText = enabled ? string.Empty : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OutOfService);
        }
    }
}
