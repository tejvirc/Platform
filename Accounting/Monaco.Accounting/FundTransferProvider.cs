namespace Aristocrat.Monaco.Accounting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts.Protocol;
    using Common;
    using Contracts;
    using Contracts.Wat;
    using Kernel;

    /// <inheritdoc cref="IFundTransferProvider" />
    [CLSCompliant(false)]
    public class FundTransferProvider : IFundTransferProvider, IService
    {
        private readonly IEventBus _eventBus;
        private readonly IMultiProtocolConfigurationProvider _multiProtocolConfigurationProvider;

        public FundTransferProvider(
            IMultiProtocolConfigurationProvider multiProtocolConfigurationProvider,
            IEventBus eventBus)
        {
            _multiProtocolConfigurationProvider = multiProtocolConfigurationProvider;
            _eventBus = eventBus;
        }

        public FundTransferProvider()
            : this(
                ServiceManager.GetInstance().GetService<IMultiProtocolConfigurationProvider>(),
                ServiceManager.GetInstance().GetService<IEventBus>())
        {
        }

        private CommsProtocol? ProtocolHandlingFundTransfer => _multiProtocolConfigurationProvider.MultiProtocolConfiguration
            .SingleOrDefault(
                x => x.IsFundTransferHandled)?.Protocol;

        public IWatTransferOffProvider GetWatTransferOffProvider(bool waitForService = false)
        {
            return GetService<IWatTransferOffProvider>(waitForService);
        }

        public IWatTransferOnProvider GetWatTransferOnProvider(bool waitForService = false)
        {
            return GetService<IWatTransferOnProvider>(waitForService);
        }

        public bool Register(string protocolName, IWatTransferOffProvider handler)
        {
            return RegisterService(protocolName, handler);
        }

        public bool Register(string protocolName, IWatTransferOnProvider handler)
        {
            return RegisterService(protocolName, handler);
        }

        public bool UnRegister(string protocolName, IWatTransferOffProvider handler)
        {
            return UnRegisterService(protocolName, handler);
        }

        public bool UnRegister(string protocolName, IWatTransferOnProvider handler)
        {
            return UnRegisterService(protocolName, handler);
        }

        public string Name => typeof(FundTransferProvider).FullName;

        public ICollection<Type> ServiceTypes => new[] {typeof(IFundTransferProvider)};

        public void Initialize()
        {
        }

        private T GetService<T>(bool waitForService)
        {
            var validator = ServiceManager.GetInstance().TryGetService<T>();
            if (validator != null || !waitForService) return validator;

            using (var serviceWaiter = new ServiceWaiter(_eventBus))
            {
                serviceWaiter.AddServiceToWaitFor<T>();

                serviceWaiter.WaitForServices();

                return ServiceManager.GetInstance().TryGetService<T>();
            }
        }

        private bool RegisterService(string protocolName, IService fundTransferService)
        {
            if (EnumParser.ParseOrThrow<CommsProtocol>(protocolName) != ProtocolHandlingFundTransfer)
            {
                return false;
            }

            ServiceManager.GetInstance().AddService(fundTransferService);
            return true;

        }

        private bool UnRegisterService(string protocolName, IService service)
        {
            if (EnumParser.ParseOrThrow<CommsProtocol>(protocolName) != ProtocolHandlingFundTransfer)
            {
                return false;
            }

            ServiceManager.GetInstance().RemoveService(service);
            return true;

        }
    }
}