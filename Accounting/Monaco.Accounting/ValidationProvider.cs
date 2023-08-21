namespace Aristocrat.Monaco.Accounting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts.Protocol;
    using Common;
    using Contracts;
    using Contracts.Handpay;
    using Kernel;
    using log4net;

    /// <inheritdoc cref="IValidationProvider" />
    public class ValidationProvider : IValidationProvider, IService
    {
        private readonly IEventBus _eventBus;
        private readonly IMultiProtocolConfigurationProvider _multiProtocolConfigurationProvider;
        private static readonly ILog Logger = LogManager.GetLogger(nameof(ValidationProvider));

        public ValidationProvider(
            IMultiProtocolConfigurationProvider multiProtocolConfigurationProvider,
            IEventBus eventBus)
        {
            _multiProtocolConfigurationProvider = multiProtocolConfigurationProvider;
            _eventBus = eventBus;
        }

        public ValidationProvider()
            : this(
                ServiceManager.GetInstance().GetService<IMultiProtocolConfigurationProvider>(),
                ServiceManager.GetInstance().GetService<IEventBus>())
        {
        }

        private CommsProtocol? ProtocolHandlingValidation =>
            _multiProtocolConfigurationProvider.MultiProtocolConfiguration.SingleOrDefault(x => x.IsValidationHandled)
                ?.Protocol;

        public string Name => GetType().ToString();

        public ICollection<Type> ServiceTypes => new[] {typeof(IValidationProvider)};

        public void Initialize()
        {
        }

        public IVoucherValidator GetVoucherValidator(bool waitForService)
        {
            return GetService<IVoucherValidator>(waitForService);
        }

        public ICurrencyValidator GetCurrencyValidator(bool waitForService)
        {
            return GetService<ICurrencyValidator>(waitForService);
        }

        public IHandpayValidator GetHandPayValidator(bool waitForService)
        {
            return GetService<IHandpayValidator>(waitForService);
        }

        public bool Register(string protocolName, IVoucherValidator voucherValidator)
        {
            return RegisterService(protocolName, voucherValidator);
        }

        public bool Register(string protocolName, ICurrencyValidator currencyValidator)
        {
            return RegisterService(protocolName, currencyValidator);
        }

        public bool Register(string protocolName, IHandpayValidator handpayValidator)
        {
            return RegisterService(protocolName, handpayValidator);
        }

        public bool UnRegister(string protocolName, IVoucherValidator voucherValidator)
        {
            return UnRegisterService(protocolName, voucherValidator);
        }

        public bool UnRegister(string protocolName, ICurrencyValidator currencyValidator)
        {
            return UnRegisterService(protocolName, currencyValidator);
        }

        public bool UnRegister(string protocolName, IHandpayValidator handpayValidator)
        {
            return UnRegisterService(protocolName, handpayValidator);
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

        private bool RegisterService(string protocolName, IService validatorService)
        {
            if (EnumParser.ParseOrThrow<CommsProtocol>(protocolName) != ProtocolHandlingValidation)
            {
                return false;
            }

            ServiceManager.GetInstance().AddService(validatorService);
            return true;

        }

        private bool UnRegisterService(string protocolName, IService validatorService)
        {
            if (EnumParser.ParseOrThrow<CommsProtocol>(protocolName) != ProtocolHandlingValidation)
            {
                return false;
            }

            try
            {
                ServiceManager.GetInstance().RemoveService(validatorService);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to unregister service: {validatorService.GetType().FullName}", ex);
                return false;
            }

            return true;
        }
    }
}