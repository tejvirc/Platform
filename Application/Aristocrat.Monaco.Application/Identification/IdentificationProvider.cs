namespace Aristocrat.Monaco.Application.Identification
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using Contracts.Identification;
    using Contracts.Protocol;
    using Hardware.Contracts.IdReader;
    using Kernel;

    public class IdentificationProvider : IIdentificationProvider, IService, IDisposable
    {
        private readonly IEventBus _eventBus;

        private IIdReaderProvider _idReaderProvider;
        private readonly IMultiProtocolConfigurationProvider _multiProtocolConfigurationProvider;
        private bool _disposed;

        public IdentificationProvider()
            : this(
                ServiceManager.GetInstance().TryGetService<IIdReaderProvider>(),
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IMultiProtocolConfigurationProvider>())
        {
        }

        public IdentificationProvider(IIdReaderProvider idReaderProvider, IEventBus bus, IMultiProtocolConfigurationProvider multiProtocolConfiguration)
        {
            _eventBus = bus ?? throw new ArgumentNullException(nameof(bus));
            _idReaderProvider = idReaderProvider ?? throw new ArgumentNullException(nameof(idReaderProvider));
            _multiProtocolConfigurationProvider = multiProtocolConfiguration ?? throw new ArgumentNullException(nameof(multiProtocolConfiguration));
        }

        public IIdentificationValidator Handler { get; private set; }

        public bool Register(IIdentificationValidator handler, string name)
        {
            if (!IsValidationHandledBy(name))
            {
                return false;
            }

            Handler = handler;
            return true;
        }

        public bool Clear(string name)
        {
            if (!IsValidationHandledBy(name))
            {
                return false;
            }

            Handler = null;
            return true;
        }

        public string Name => GetType().ToString();

        public ICollection<Type> ServiceTypes => new[] { typeof(IIdentificationProvider) };

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Initialize()
        {
            _eventBus.Subscribe<IdClearedEvent>(this, Handle);
            _eventBus.Subscribe<IdPresentedEvent>(this, Handle);
            _eventBus.Subscribe<ReadErrorEvent>(this, Handle);
            _eventBus.Subscribe<ServiceAddedEvent>(this, Handle);
            _eventBus.Subscribe<ValidationRequestedEvent>(this, Handle);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private async Task Handle(IdClearedEvent e, CancellationToken token)
        {
            if (Handler != null)
            {
                await Handler.ClearValidation(e.IdReaderId, token);
            }
        }

        private void Handle(IdPresentedEvent e)
        {
            Handler?.InitializeValidation(e.IdReaderId);
        }

        private void Handle(ReadErrorEvent e)
        {
            Handler?.HandleReadError(e.IdReaderId);
        }

        private void Handle(ServiceAddedEvent e)
        {
            if (e.ServiceType == typeof(IIdReader))
            {
                _idReaderProvider = ServiceManager.GetInstance().TryGetService<IIdReaderProvider>();
            }
        }

        private async Task Handle(ValidationRequestedEvent e, CancellationToken token)
        {
            if (Handler != null)
            {
                var result = await Handler.ValidateIdentification(e.IdReaderId, e.TrackData, token);

                if (result)
                {
                    _idReaderProvider.SetValidationComplete(e.IdReaderId);
                }
                else
                {
                    _idReaderProvider.SetValidationFailed(e.IdReaderId);
                }
            }
        }

        private bool IsValidationHandledBy(string protocolName)
        {
            var protocolConfiguration = _multiProtocolConfigurationProvider
                .MultiProtocolConfiguration.FirstOrDefault(x => x.Protocol == EnumParser.ParseOrThrow<CommsProtocol>(protocolName));

            return protocolConfiguration != null && protocolConfiguration.IsValidationHandled;
        }
    }
}
