namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Aristocrat.Sas.Client;
    using Contracts.Client;
    using Hardware.Contracts.TowerLight;
    using Kernel;

    /// <summary>
    /// Handles system events
    /// </summary>
    public class SystemEventHandler : ISystemEventHandler
    {
        private readonly ISasHost _sasHost;
        private readonly ISasExceptionHandler _exceptionHandler;
        private readonly ISystemDisableManager _systemDisableManager;
        private readonly ITowerLight _light;
        private readonly ISasVoucherInProvider _sasVoucherInProvider;

        /// <summary>
        /// Creates a new instance of the SystemEventHandler class
        /// </summary>
        /// <param name="sasHost">The host</param>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        /// <param name="systemDisableManager">The system disable manager</param>
        /// <param name="light">The tower light manager</param>
        /// <param name="sasVoucherInProvider">The SAS voucher in provider</param>
        public SystemEventHandler(
            ISasHost sasHost,
            ISasExceptionHandler exceptionHandler,
            ISystemDisableManager systemDisableManager,
            ITowerLight light,
            ISasVoucherInProvider sasVoucherInProvider)
        {
            _sasHost = sasHost ?? throw new ArgumentNullException(nameof(sasHost));
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            _systemDisableManager =
                systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));
            _light = light ?? throw new ArgumentNullException(nameof(light));
            _sasVoucherInProvider = sasVoucherInProvider ?? throw new ArgumentNullException(nameof(sasVoucherInProvider));
        }

        /// <inheritdoc />
        public void OnPlatformBooted(bool isFirstLoad)
        {
            _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.EgmPowerLost));
            _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.EgmPowerApplied));

            if (isFirstLoad)
            {
                _exceptionHandler.ReportException(
                    new GenericExceptionBuilder(GeneralExceptionCode.GamingMachineSoftMetersReset));
            }
        }

        /// <inheritdoc />
        public void OnSasStarted()
        {
            if (_light.IsLit)
            {
                _exceptionHandler.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.ChangeLampOn));
            }
        }

        /// <inheritdoc />
        public void OnSystemEnabled()
        {
            if (!_systemDisableManager.IsDisabled)
            {
                SetSasFeaturesEnabled(true);
            }
        }

        /// <inheritdoc />
        public void OnSystemDisabled()
        {
            SetSasFeaturesEnabled(false);
            _sasVoucherInProvider.OnSystemDisabled();
        }

        private void SetSasFeaturesEnabled(bool enabled)
        {
            _sasHost.SetLegacyBonusEnabled(!_systemDisableManager.IsDisabled && enabled);
        }
    }
}
