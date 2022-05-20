namespace Aristocrat.Monaco.Sas.UI.Settings
{
    using MVVM.Model;
    using Storage.Models;

    /// <summary>
    ///     Gets the port assignment setting
    /// </summary>
    public class PortAssignmentSetting : BaseNotify
    {
        private HostId _aftPort;
        private HostId _validationPort;
        private HostId _progressivePort;
        private HostId _generalControlPort;
        private GameStartEndHost _gameStartEndHosts;
        private HostId _legacyBonusPort;
        private bool _isDualHost;
        private bool _host1NonSasProgressiveHitReporting;
        private bool _host2NonSasProgressiveHitReporting;

        /// <summary>
        ///     Gets or sets the Aft Port
        /// </summary>
        public HostId AftPort
        {
            get => _aftPort;
            set => SetProperty(ref _aftPort, value);
        }

        /// <summary>
        ///     Gets or sets the validation port
        /// </summary>
        public HostId ValidationPort
        {
            get => _validationPort;
            set => SetProperty(ref _validationPort, value);
        }

        /// <summary>
        ///     Gets or sets the progressive port
        /// </summary>
        public HostId ProgressivePort
        {
            get => _progressivePort;
            set => SetProperty(ref _progressivePort, value);
        }

        /// <summary>
        ///     Gets or sets the general control port
        /// </summary>
        public HostId GeneralControlPort
        {
            get => _generalControlPort;
            set => SetProperty(ref _generalControlPort, value);
        }

        /// <summary>
        ///     Gets or sets the game start and end hosts
        /// </summary>
        public GameStartEndHost GameStartEndHosts
        {
            get => _gameStartEndHosts;
            set => SetProperty(ref _gameStartEndHosts, value);
        }

        /// <summary>
        ///     Gets or sets the legacy bonus port
        /// </summary>
        public HostId LegacyBonusPort
        {
            get => _legacyBonusPort;
            set => SetProperty(ref _legacyBonusPort, value);
        }

        /// <summary>
        ///     Gets or sets whether or not we are dual host
        /// </summary>
        public bool IsDualHost
        {
            get => _isDualHost;
            set => SetProperty(ref _isDualHost, value);
        }

        /// <summary>
        ///     Gets or sets whether or not we are Host 1 Non Sas Progressive Hit Reporting
        /// </summary>
        public bool Host1NonSasProgressiveHitReporting
        {
            get => _host1NonSasProgressiveHitReporting;
            set => SetProperty(ref _host1NonSasProgressiveHitReporting, value);
        }

        /// <summary>
        ///     Gets or sets whether or not we are Host 2 Non Sas Progressive Hit Reporting
        /// </summary>
        public bool Host2NonSasProgressiveHitReporting
        {
            get => _host2NonSasProgressiveHitReporting;
            set => SetProperty(ref _host2NonSasProgressiveHitReporting, value);
        }

        /// <summary>
        ///     Performs conversion from <see cref="PortAssignmentSetting"/> to <see cref="PortAssignment"/>
        /// </summary>
        /// <param name="setting">The <see cref="PortAssignmentSetting"/> setting</param>
        public static explicit operator PortAssignment(PortAssignmentSetting setting) => new PortAssignment
        {
            AftPort = setting.AftPort,
            ValidationPort = setting.ValidationPort,
            ProgressivePort = setting.ProgressivePort,
            GameStartEndHosts = setting.GameStartEndHosts,
            GeneralControlPort = setting.GeneralControlPort,
            IsDualHost = setting.IsDualHost,
            LegacyBonusPort = setting.LegacyBonusPort,
            Host1NonSasProgressiveHitReporting =  setting.Host1NonSasProgressiveHitReporting,
            Host2NonSasProgressiveHitReporting = setting.Host2NonSasProgressiveHitReporting
        };

        /// <summary>
        ///     Performs conversion from <see cref="PortAssignment"/> to <see cref="PortAssignmentSetting"/>
        /// </summary>
        /// <param name="assignment">The <see cref="PortAssignment"/> port assignment</param>
        public static explicit operator PortAssignmentSetting(PortAssignment assignment) =>
            new PortAssignmentSetting
            {
                AftPort = assignment.AftPort,
                ValidationPort = assignment.ValidationPort,
                ProgressivePort = assignment.ProgressivePort,
                GameStartEndHosts = assignment.GameStartEndHosts,
                GeneralControlPort = assignment.GeneralControlPort,
                IsDualHost = assignment.IsDualHost,
                LegacyBonusPort = assignment.LegacyBonusPort,
                Host1NonSasProgressiveHitReporting = assignment.Host1NonSasProgressiveHitReporting,
                Host2NonSasProgressiveHitReporting = assignment.Host2NonSasProgressiveHitReporting
            };
    }
}