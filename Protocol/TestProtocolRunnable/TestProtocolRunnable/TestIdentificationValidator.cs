namespace Aristocrat.Monaco.TestProtocol
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Application.Contracts.Identification;
    using Cabinet.Contracts;
    using Gaming.Contracts.InfoBar;
    using Hardware.Contracts.CardReader;
    using Kernel;
    using log4net;

    public class TestIdentificationValidator : IIdentificationValidator, IService, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly Guid InfoBarOwnershipKey = new Guid("{AC4B8731-66A0-460C-B303-8357FD9E7516}");

        private IEventBus _bus;
        private IIdentificationProvider _idProvider;
        private IEmployeeLogin _employeeLogin;
        private string _employeeId;
        private bool _disposed;

        /// <inheritdoc />
        public string Name => GetType().FullName;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IIdentificationValidator) };

        public bool IgnoreKeySwitches { get; set; }

        /// <inheritdoc />
        public void Initialize()
        {
            var services = ServiceManager.GetInstance();
            _bus = services.GetService<IEventBus>();
            _employeeLogin = services.GetService<IEmployeeLogin>();
            _idProvider = services.GetService<IIdentificationProvider>();

            _idProvider.Register(this, ProtocolNames.Test);
        }

        /// <inheritdoc />
        public void InitializeValidation(int readerId)
        {
        }

        /// <inheritdoc />
        public Task ClearValidation(int readerId, CancellationToken token)
        {
            _employeeLogin.Logout(_employeeId);

            Logger.Debug($"Logging out employee '{_employeeId}'");
            ClearMessages(InfoBarRegion.Left);
            ShowMessage(
                $"EMPLOYEE LOGGED OUT: {_employeeId}",
                InfoBarColor.White,
                InfoBarRegion.Center,
                InfoBarDisplayTransientMessageEvent.DefaultMessageDisplayDuration);

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void HandleReadError(int readerId)
        {
        }

        /// <inheritdoc />
        public Task<bool> ValidateIdentification(int readerId, TrackData trackData, CancellationToken token)
        {
            _employeeId = trackData?.Track1;
            _employeeLogin.Login(_employeeId);

            Logger.Debug($"Logging in employee '{_employeeId}'");
            ShowMessage($"Employee: {_employeeId}", InfoBarColor.White, InfoBarRegion.Left);
            ShowMessage(
                "EMPLOYEE LOGIN SUCCESS.",
                InfoBarColor.Green,
                InfoBarRegion.Center,
                InfoBarDisplayTransientMessageEvent.DefaultMessageDisplayDuration);

            return Task.FromResult(true);
        }

        public Task LogoffPlayer()
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _idProvider.Clear(ProtocolNames.Test);
                }

                _disposed = true;
            }
        }

        private void ShowMessage(string message, InfoBarColor color, InfoBarRegion region, TimeSpan duration = default)
        {
            if (duration != default)
            {
                _bus.Publish(
                    new InfoBarDisplayTransientMessageEvent(
                        InfoBarOwnershipKey,
                        message,
                        duration,
                        color,
                        InfoBarColor.Black,
                        region,
                        DisplayRole.Main));
            }
            else
            {
                _bus.Publish(
                    new InfoBarDisplayStaticMessageEvent(
                        InfoBarOwnershipKey,
                        message,
                        color,
                        InfoBarColor.Black,
                        region,
                        DisplayRole.Main));
            }
        }

        private void ClearMessages(params InfoBarRegion[] regions)
        {
            _bus.Publish(new InfoBarClearMessageEvent(InfoBarOwnershipKey, DisplayRole.Main, regions));
        }
    }
}
