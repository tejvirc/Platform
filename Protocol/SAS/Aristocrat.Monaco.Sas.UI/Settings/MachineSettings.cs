namespace Aristocrat.Monaco.Sas.UI.Settings
{
    using System.Collections.ObjectModel;
    using Gaming.Contracts;
    using MVVM.Model;

    /// <summary>
    ///     SAS machine settings.
    /// </summary>
    public class MachineSettings : BaseNotify
    {
        private PortAssignmentSetting _portAssignmentSetting;
        private SasFeaturesSettings _sasFeaturesSettings;
        private CashableLockupStrategy _hostDisableCashoutAction;

        /// <summary>
        ///     Gets or sets the SAS Host settings.
        /// </summary>
        public ObservableCollection<SasHostSetting> SasHostSettings { get; set; }

        /// <summary>
        ///     Gets or sets the port assignment settings
        /// </summary>
        public PortAssignmentSetting PortAssignmentSetting
        {
            get => _portAssignmentSetting;
            set => SetProperty(ref _portAssignmentSetting, value);
        }

        /// <summary>
        ///     Gets or sets the sas features settings
        /// </summary>
        public SasFeaturesSettings SasFeaturesSettings
        {
            get => _sasFeaturesSettings;
            set => SetProperty(ref _sasFeaturesSettings, value);
        }

        /// <summary>
        ///     Gets or sets the host disable cashout action.
        /// </summary>
        public CashableLockupStrategy HostDisableCashoutAction
        {
            get => _hostDisableCashoutAction;

            set => SetProperty(ref _hostDisableCashoutAction, value);
        }
    }
}
