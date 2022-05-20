namespace Aristocrat.Monaco.Mgam.Services.Registration
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Services.Directory;
    using Common;
    using Common.Data.Models;
    using Common.Data.Repositories;
    using Common.Events;
    using Kernel;
    using Protocol.Common.Storage.Entity;

    internal class VltServiceLocator : IVltServiceLocator, IDisposable
    {
        private const int LocateResponseTimeout = 30;

        private readonly ILogger _logger;
        private readonly IEventBus _eventBus;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IEgm _egm;

        private Timer _relocateTimer;
        private IDisposable _serviceListener;

        private string _serviceName;

        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="VltServiceLocator"/> class.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="eventBus"></param>
        /// <param name="unitOfWorkFactory"></param>
        /// <param name="egm"></param>
        public VltServiceLocator(
            ILogger<VltServiceLocator> logger,
            IEventBus eventBus,
            IUnitOfWorkFactory unitOfWorkFactory,
            IEgm egm)
        {
            _logger = logger;
            _eventBus = eventBus;
            _unitOfWorkFactory = unitOfWorkFactory;
            _egm = egm;

            _relocateTimer = new Timer(async _ => await RelocateTick(), null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

            SubscribeToEvents();
        }

        /// <inheritdoc />
        ~VltServiceLocator()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [SuppressMessage("ReSharper", "UseNullPropagation")]
        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _eventBus?.UnsubscribeAll(this);

                if (_relocateTimer != null)
                {
                    _relocateTimer.Dispose();
                }

                if (_serviceListener != null)
                {
                    _serviceListener.Dispose();
                }
            }

            _relocateTimer = null;
            _serviceListener = null;

            _disposed = true;
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<RequestServiceLocationEvent>(this, Handle);
        }

        private async Task LocateServices()
        {
            var directory = _egm.GetService<IDirectory>();

            try
            {
                _serviceListener?.Dispose();
                _serviceListener = await directory.LocateServices(GetServiceName(), async endPoint =>
                    {
                        EnableRelocateTimer(false);

                        _logger.LogInfo($"Located VLT Service {_serviceName} at {endPoint}");

                        CreateFirewallRule(endPoint);

                        _eventBus.Publish(new ServiceFoundEvent(endPoint));

                        await Task.CompletedTask;
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Locate service failure");
            }

            EnableRelocateTimer(true);
        }

        private static void CreateFirewallRule(IPEndPoint endPoint)
        {
            var port = unchecked((ushort)endPoint.Port);

            Firewall.AddUdpRule($"{MgamConstants.FirewallVltServiceRuleName} - Port {port}", port);
        }

        private async Task Handle(RequestServiceLocationEvent evt, CancellationToken cancellationToken)
        {
            await LocateServices();
        }

        private void EnableRelocateTimer(bool enable)
        {
            _relocateTimer.Change(enable ? TimeSpan.FromSeconds(LocateResponseTimeout) : Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        private async Task RelocateTick()
        {
            await LocateServices();
        }

        private string GetServiceName()
        {
            if (!string.IsNullOrWhiteSpace(_serviceName))
            {
                return _serviceName;
            }

            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                _serviceName = unitOfWork.Repository<Host>().GetServiceName();

                if (string.IsNullOrWhiteSpace(_serviceName))
                {
                    throw new InvalidOperationException("Service name has not been configured.");
                }
            }

            return _serviceName;
        }
    }
}
