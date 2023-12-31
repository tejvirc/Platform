﻿namespace Aristocrat.Monaco.Mgam.UI.Settings
{
    using MVVM.Model;

    /// <summary>
    ///     Machine settings.
    /// </summary>
    public class MachineSettings : BaseNotify
    {
        private int _directoryPort;
        private string _serviceName;

        /// <summary>
        ///     Gets or sets the directory service port.
        /// </summary>
        public int DirectoryPort
        {
            get => _directoryPort;

            set => SetProperty(ref _directoryPort, value);
        }

        /// <summary>
        ///     Gets or sets the service location port.
        /// </summary>
        public string ServiceName
        {
            get => _serviceName;

            set => SetProperty(ref _serviceName, value);
        }
    }
}
