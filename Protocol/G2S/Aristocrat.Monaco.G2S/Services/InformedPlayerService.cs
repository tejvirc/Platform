namespace Aristocrat.Monaco.G2S.Services
{
    using System;
    using System.Collections.Generic;
    using Accounting.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts;
    using Handlers;
    using Handlers.InformedPlayer;
    using Kernel;

    /// <summary>
    ///     Implement the <see cref="IInformedPlayerService" /> service
    /// </summary>
    public class InformedPlayerService : IInformedPlayerService, IService, IDisposable
    {
        private readonly IG2SEgm _egm;
        private readonly IEventBus _eventBus;
        private readonly IPlayerBank _bank;
        private readonly ISystemDisableManager _systemDisableManager;
        private readonly ICommandBuilder<IInformedPlayerDevice, ipStatus> _commandBuilder;

        private bool _disposed;

        /// <summary>
        ///     Handles all player session history and current session
        /// </summary>
        public InformedPlayerService(
            IG2SEgm egm,
            IEventBus eventBus,
            IPlayerBank bank,
            ISystemDisableManager systemDisableManager)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _systemDisableManager =
                systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));
            _commandBuilder = new IpStatusCommandBuilder();
        }

        private static Guid GamePlayDisabledKey => new Guid("{AD7342FC-FC1A-419A-AF78-B48A55C2A325}");

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        /// <inheritdoc />
        public void SetGamePlayState(
            IInformedPlayerDevice device,
            bool enabled,
            string disableMessage,
            bool internallyGenerated = false)
        {
            if (enabled == device.GamePlayEnabled)
            {
                return;
            }

            device.GamePlayEnabled = enabled;

            if (enabled)
            {
                _systemDisableManager.Enable(GamePlayDisabledKey);
            }
            else
            {
                _systemDisableManager.Disable(GamePlayDisabledKey, SystemDisablePriority.Normal, () => disableMessage);
            }

            if (internallyGenerated)
            {
                SendIpStatus();
            }

            EventReport(enabled ? EventCode.G2S_IPE103 : EventCode.G2S_IPE102);
        }

        /// <inheritdoc />
        public void SetMoneyInState(IInformedPlayerDevice device, bool enabled, bool internallyGenerated = false)
        {
            if (enabled == device.MoneyInEnabled)
            {
                return;
            }

            device.MoneyInEnabled = enabled;

            var properties = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            properties.SetProperty(AccountingConstants.MoneyInEnabled, enabled);

            if (internallyGenerated)
            {
                SendIpStatus();
            }

            EventReport(enabled ? EventCode.G2S_IPE101 : EventCode.G2S_IPE100);
        }

        public void OnPlayerSessionStarted()
        {
            var device = _egm.GetDevice<IInformedPlayerDevice>();

            SetMoneyInState(device, device.SessionStartMoneyIn);
            SetGamePlayState(
                device,
                device.SessionStartGamePlay,
                EventCode.G2S_IPE102); // TODO: might should be something else
            device.SessionLimit = device.SessionStartLimit;

            if (device.SessionStartCashOut && _bank.Balance > 0)
            {
                _eventBus.Publish(new CashOutButtonPressedEvent());
            }

            EventReport(EventCode.G2S_IPE112);
        }

        public void OnPlayerSessionEnded()
        {
            var device = _egm.GetDevice<IInformedPlayerDevice>();

            SetMoneyInState(device, device.UnCardedMoneyIn);
            SetGamePlayState(
                device,
                device.UnCardedGamePlay,
                EventCode.G2S_IPE102); // TODO: might should be something else

            if (device.SessionEndCashOut && _bank.Balance > 0)
            {
                _eventBus.Publish(new CashOutButtonPressedEvent());
            }

            EventReport(EventCode.G2S_IPE114);
        }

        /// <inheritdoc />
        public string Name => typeof(InformedPlayerService).ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IInformedPlayerService) };

        /// <inheritdoc />
        public void Initialize()
        {
            _eventBus.Subscribe<PrimaryGameStartedEvent>(this, Handle);
            _eventBus.Subscribe<PrimaryGameEndedEvent>(this, Handle);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _eventBus.UnsubscribeAll(this);
                }

                _disposed = true;
            }
        }

        private void Handle(PrimaryGameStartedEvent evt)
        {
            // TODO - get and check total cashable wager amt
        }

        private void Handle(PrimaryGameEndedEvent evt)
        {
            // TODO - game delays?
        }

        /// <summary>
        ///     Send the IpStatus command to the G2S host spontaneously,
        ///     when money-in-enabled and game-play-enabled statuses change.
        /// </summary>
        private void SendIpStatus()
        {
            var device = _egm.GetDevice<IInformedPlayerDevice>();

            var command = new ipStatus();
            _commandBuilder.Build(device, command);

            var request = device.InformedPlayerClassInstance;
            request.Item = command;

            device.Queue.SendRequest(request);
        }

        private void EventReport(string eventStr, bool noStatus = false)
        {
            var device = _egm.GetDevice<IInformedPlayerDevice>();

            var status = new ipStatus();

            _commandBuilder.Build(device, status);

            var deviceList = device.DeviceList(status);

            EventHandlerDevice.EventReport(
                device.PrefixedDeviceClass(),
                device.Id,
                eventStr,
                noStatus ? null : deviceList);
        }
    }
}