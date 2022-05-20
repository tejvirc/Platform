namespace Aristocrat.Monaco.Accounting.Tests
{
    using System.Collections.Generic;
    using Application.Contracts;
    using Application.Contracts.Protocol;
    using Aristocrat.Monaco.Common;
    using Contracts;
    using Contracts.Handpay;
    using Contracts.Wat;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     Unit tests for ValidationProvider and Fund transfer provider
    /// </summary>
    [TestClass]
    public class ValidatorAndFundTransferProviderTests
    {
        private readonly List<string> _protocolList = new List<string> {ProtocolNames.SAS, ProtocolNames.MGAM, ProtocolNames.G2S, ProtocolNames.Test, ProtocolNames.HHR};
        private Mock<IMultiProtocolConfigurationProvider> _multiProtocolConfigurationProvider;
        public Mock<IEventBus> Bus { get; private set; }

        [TestInitialize]
        public void TestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _multiProtocolConfigurationProvider =
                MoqServiceManager.CreateAndAddService<IMultiProtocolConfigurationProvider>(MockBehavior.Default);
            Bus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
        }

        [TestMethod]
        public void ValidationProvider_WhenVoucherValidatorRegistered_ReturnsRegisteredVoucherValidator()
        {
            // Setup
            _multiProtocolConfigurationProvider.Setup(c => c.MultiProtocolConfiguration).Returns(
                GenerateMultiConfiguration(CommsProtocol.SAS, CommsProtocol.MGAM, CommsProtocol.HHR));
            IValidationProvider validationProvider = new ValidationProvider();
            IService registeredValidator = null;

            IVoucherValidator validatorService;
            // Action
            _protocolList.ForEach(x =>
            {
                validatorService = new Mock<IVoucherValidator>().Object;
                // When the add service is called, it will assign the registered validator to the one passed.
                // Please note that only one AddService() will be called depending on which protocol is handling validation.
                MoqServiceManager.Instance.Setup(c => c.AddService(validatorService))
                    .Callback<IService>(e => registeredValidator = e);
                MoqServiceManager.Instance.Setup(c => c.RemoveService(validatorService)).Callback<IService>(e =>
                {
                    if (registeredValidator == e)
                    {
                        registeredValidator = null;
                    }
                });
                validationProvider.Register(x, validatorService);
            });
            MoqServiceManager.Instance.Setup(x => x.TryGetService<IVoucherValidator>())
                .Returns(registeredValidator as IVoucherValidator);

            // Assert
            // This makes sure that the AddService() of service manager was called by the ValidationProvider
            // with the correct validation provider and GetVoucherValidator() returns the same.
            Assert.AreEqual(validationProvider.GetVoucherValidator(), registeredValidator);
            validationProvider.UnRegister(ProtocolNames.SAS, validationProvider.GetVoucherValidator());
            Assert.AreEqual(registeredValidator, null);
        }

        [TestMethod]
        public void ValidationProvider_WhenAllValidatorsRegistered_ReturnsRegisteredValidators()
        {
            // Setup
            _multiProtocolConfigurationProvider.Setup(c => c.MultiProtocolConfiguration).Returns(
                GenerateMultiConfiguration(CommsProtocol.SAS, CommsProtocol.MGAM, CommsProtocol.HHR));

            IValidationProvider validationProvider = new ValidationProvider();
            IService registeredVoucherValidator = null;
            IService registeredCurrencyValidator = null;
            IService registeredHandPayValidator = null;

            // Action
            _protocolList.ForEach(x =>
            {
                var voucherValidator = new Mock<IVoucherValidator>().Object;
                // When the add service is called, it will assign the registered validator to the one passed.
                // Please note that only one AddService() per validation type will be called depending on which protocol is handling validation.
                MoqServiceManager.Instance.Setup(c => c.AddService(voucherValidator))
                    .Callback<IService>(e => registeredVoucherValidator = e);
                validationProvider.Register(x, voucherValidator);
                var currencyValidator = new Mock<ICurrencyValidator>().Object;
                MoqServiceManager.Instance.Setup(c => c.AddService(currencyValidator))
                    .Callback<IService>(e => registeredCurrencyValidator = e);
                validationProvider.Register(x, currencyValidator);
                var handPayValidator = new Mock<IHandpayValidator>().Object;
                MoqServiceManager.Instance.Setup(c => c.AddService(handPayValidator))
                    .Callback<IService>(e => registeredHandPayValidator = e);
                validationProvider.Register(x, handPayValidator);
                MoqServiceManager.Instance.Setup(c => c.RemoveService(voucherValidator)).Callback<IService>(e =>
                {
                    if (registeredVoucherValidator == voucherValidator)
                    {
                        registeredVoucherValidator = null;
                    }
                });
                MoqServiceManager.Instance.Setup(c => c.RemoveService(currencyValidator)).Callback<IService>(e =>
                {
                    if (registeredCurrencyValidator == e)
                    {
                        registeredCurrencyValidator = null;
                    }
                });
                MoqServiceManager.Instance.Setup(c => c.RemoveService(handPayValidator)).Callback<IService>(e =>
                {
                    if (registeredHandPayValidator == e)
                    {
                        registeredHandPayValidator = null;
                    }
                });
            });

            MoqServiceManager.Instance.Setup(x => x.TryGetService<IVoucherValidator>())
                .Returns(registeredVoucherValidator as IVoucherValidator);
            MoqServiceManager.Instance.Setup(x => x.TryGetService<ICurrencyValidator>())
                .Returns(registeredCurrencyValidator as ICurrencyValidator);
            MoqServiceManager.Instance.Setup(x => x.TryGetService<IHandpayValidator>())
                .Returns(registeredHandPayValidator as IHandpayValidator);

            // Assert
            // This makes sure that the AddService() of service manager was called by the ValidationProvider
            // with the correct validation provider and the GetXXXValidator() returns the same.
            Assert.AreEqual(validationProvider.GetVoucherValidator(), registeredVoucherValidator);
            Assert.AreEqual(validationProvider.GetCurrencyValidator(), registeredCurrencyValidator);
            Assert.AreEqual(validationProvider.GetHandPayValidator(), registeredHandPayValidator);
            validationProvider.UnRegister(ProtocolNames.SAS, validationProvider.GetVoucherValidator());
            Assert.AreEqual(registeredVoucherValidator, null);
            validationProvider.UnRegister(ProtocolNames.SAS, validationProvider.GetCurrencyValidator());
            Assert.AreEqual(registeredCurrencyValidator, null);
            validationProvider.UnRegister(ProtocolNames.SAS, validationProvider.GetHandPayValidator());
            Assert.AreEqual(registeredHandPayValidator, null);
        }

        [TestMethod]
        public void ValidationProvider_WhenCurrencyValidatorRegistered_ReturnsRegisteredCurrencyValidator()
        {
            // Setup
            _multiProtocolConfigurationProvider.Setup(c => c.MultiProtocolConfiguration).Returns(
                GenerateMultiConfiguration(CommsProtocol.SAS, CommsProtocol.MGAM, CommsProtocol.HHR));
            IValidationProvider validationProvider = new ValidationProvider();
            IService registeredValidator = null;

            // Action
            _protocolList.ForEach(x =>
            {
                var validator = new Mock<ICurrencyValidator>().Object;
                // When the add service is called, it will assign the registered validator to the one passed.
                // Please note that only one AddService() will be called depending on which protocol is handling validation.
                MoqServiceManager.Instance.Setup(c => c.AddService(validator))
                    .Callback<IService>(e => registeredValidator = e);
                MoqServiceManager.Instance.Setup(c => c.RemoveService(validator)).Callback<IService>(e =>
                {
                    if (registeredValidator == e)
                    {
                        registeredValidator = null;
                    }
                });
                validationProvider.Register(x, validator);
            });
            MoqServiceManager.Instance.Setup(x => x.TryGetService<ICurrencyValidator>())
                .Returns(registeredValidator as ICurrencyValidator);

            // Assert
            // This makes sure that the AddService() of service manager was called by the ValidationProvider
            // with the correct validation provider and the GetCurrencyValidator() returns the same.
            Assert.AreEqual(validationProvider.GetCurrencyValidator(), registeredValidator);
            validationProvider.UnRegister(ProtocolNames.SAS, validationProvider.GetCurrencyValidator());
            Assert.AreEqual(registeredValidator, null);
        }

        [TestMethod]
        public void ValidationProvider_WhenHandPayValidatorRegistered_ReturnsRegisteredHandPayValidator()
        {
            // Setup
            _multiProtocolConfigurationProvider.Setup(c => c.MultiProtocolConfiguration).Returns(
                GenerateMultiConfiguration(CommsProtocol.SAS, CommsProtocol.MGAM, CommsProtocol.HHR));
            IValidationProvider validationProvider = new ValidationProvider();
            IService registeredValidator = null;

            //Action
            _protocolList.ForEach(x =>
            {
                var validator = new Mock<IHandpayValidator>().Object;
                // When the add service is called, it will assign the registered validator to the one passed.
                // Please note that only one AddService() will be called depending on which protocol is handling validation.
                MoqServiceManager.Instance.Setup(c => c.AddService(validator))
                    .Callback<IService>(e => registeredValidator = e);
                MoqServiceManager.Instance.Setup(c => c.RemoveService(validator)).Callback<IService>(e =>
                {
                    if (registeredValidator == e)
                    {
                        registeredValidator = null;
                    }
                });
                validationProvider.Register(x, validator);
            });
            MoqServiceManager.Instance.Setup(x => x.TryGetService<IHandpayValidator>())
                .Returns(registeredValidator as IHandpayValidator);

            // Assert
            // This makes sure that the AddService() of service manager was called by the ValidationProvider
            // with the correct validation provider and the GetHandPayValidator() returns the same.
            Assert.AreEqual(validationProvider.GetHandPayValidator(), registeredValidator);
            validationProvider.UnRegister(ProtocolNames.SAS, validationProvider.GetHandPayValidator());
            Assert.AreEqual(registeredValidator, null);
        }

        [TestMethod]
        public void FundTransferProvider_WhenWatOnProviderRegistered_ReturnsRegisteredWatOnProvider()
        {
            //Setup
            _multiProtocolConfigurationProvider.Setup(c => c.MultiProtocolConfiguration).Returns(
                GenerateMultiConfiguration(CommsProtocol.SAS, CommsProtocol.MGAM, CommsProtocol.HHR));
            IFundTransferProvider fundTransferProvider = new FundTransferProvider();
            IService registeredWatOnProvider = null;

            // Action
            _protocolList.ForEach(x =>
            {
                var watOnProvider = new Mock<IWatTransferOnProvider>().Object;
                // When the add service is called, it will assign the registeredWATOnProvider provider to the one passed.
                // Please note that only one AddService() will be called depending on which protocol is handling fund transfer.
                MoqServiceManager.Instance.Setup(c => c.AddService(watOnProvider))
                    .Callback<IService>(e => registeredWatOnProvider = e);
                MoqServiceManager.Instance.Setup(c => c.RemoveService(watOnProvider)).Callback<IService>(e =>
                {
                    if (registeredWatOnProvider == e)
                    {
                        registeredWatOnProvider = null;
                    }
                });
                fundTransferProvider.Register(x, watOnProvider);
            });

            MoqServiceManager.Instance.Setup(x => x.TryGetService<IWatTransferOnProvider>())
                .Returns(registeredWatOnProvider as IWatTransferOnProvider);

            // Assert
            // This makes sure that the AddService() of service manager was called by the FundTransferProvider
            // with the correct WAT on provider and the GetWatTransferOnProvider() returns the same.
            Assert.AreEqual(fundTransferProvider.GetWatTransferOnProvider(), registeredWatOnProvider);
            fundTransferProvider.UnRegister(ProtocolNames.MGAM, fundTransferProvider.GetWatTransferOnProvider());
            Assert.AreEqual(registeredWatOnProvider, null);
        }

        [TestMethod]
        public void FundTransferProvider_WhenWatOffProviderRegistered_ReturnsRegisteredWatOffProvider()
        {
            // Setup
            _multiProtocolConfigurationProvider.Setup(c => c.MultiProtocolConfiguration).Returns(
                GenerateMultiConfiguration(CommsProtocol.SAS, CommsProtocol.MGAM, CommsProtocol.HHR));
            IFundTransferProvider fundTransferProvider = new FundTransferProvider();
            IService registeredWatOffProvider = null;

            // Action
            _protocolList.ForEach(x =>
            {
                var watOffProvider = new Mock<IWatTransferOffProvider>().Object;
                // When the add service is called, it will assign the registeredWatOffProvider provider to the one passed.
                // Please note that only one AddService() will be called depending on which protocol is handling fund transfer.
                MoqServiceManager.Instance.Setup(c => c.AddService(watOffProvider))
                    .Callback<IService>(e => registeredWatOffProvider = e);
                MoqServiceManager.Instance.Setup(c => c.RemoveService(watOffProvider)).Callback<IService>(e =>
                {
                    if (registeredWatOffProvider == e)
                    {
                        registeredWatOffProvider = null;
                    }
                });
                fundTransferProvider.Register(x, watOffProvider);
            });
            MoqServiceManager.Instance.Setup(x => x.TryGetService<IWatTransferOffProvider>())
                .Returns(registeredWatOffProvider as IWatTransferOffProvider);

            // Assert
            // This makes sure that the AddService() of service manager was called by the FundTransferProvider
            // with the correct WAT off provider and the GetWatTransferOffProvider() returns the same.
            Assert.AreEqual(fundTransferProvider.GetWatTransferOffProvider(), registeredWatOffProvider);
            fundTransferProvider.UnRegister(ProtocolNames.MGAM, fundTransferProvider.GetWatTransferOffProvider());
            Assert.AreEqual(registeredWatOffProvider, null);
        }

        [TestMethod]
        public void ValidationProvider_WhenNoProtocolHandlingValidation_ReturnsNull()
        {
            // Setup
            _multiProtocolConfigurationProvider.Setup(c => c.MultiProtocolConfiguration).Returns(
                new List<ProtocolConfiguration>
                {
                    new ProtocolConfiguration(CommsProtocol.SAS),
                    new ProtocolConfiguration(CommsProtocol.MGAM),
                    new ProtocolConfiguration(CommsProtocol.HHR)
                });

            IValidationProvider validationProvider = new ValidationProvider();
            IService registeredValidator = null;

            // Action
            _protocolList.ForEach(x =>
            {
                var validator = new Mock<IVoucherValidator>().Object;
                // When the add service is called, it will assign the registeredValidator provider to the one passed.
                // Please note that only one AddService() will be called depending on which protocol is handling fund transfer.
                // In this test, no protocol is handling the validation, hence it would never be called.
                MoqServiceManager.Instance.Setup(c => c.AddService(validator))
                    .Callback<IService>(e => registeredValidator = e);
                MoqServiceManager.Instance.Setup(c => c.RemoveService(validator)).Callback<IService>(e =>
                {
                    if (registeredValidator == e)
                    {
                        registeredValidator = null;
                    }
                });
                validationProvider.Register(x, validator);
            });

            MoqServiceManager.Instance.Setup(x => x.TryGetService<IVoucherValidator>())
                .Returns(registeredValidator as IVoucherValidator);

            // Assert
            // This makes sure that the AddService() was never called by ValidationProvider and hance
            // GetVoucherValidator will return null.
            Assert.AreEqual(validationProvider.GetVoucherValidator(), null);
        }

        [TestMethod]
        public void FundTransferProvider_WhenNoProtocolHandlingFundTransfer_ReturnsNullFundTransferProviders()
        {
            // Setup
            _multiProtocolConfigurationProvider.Setup(c => c.MultiProtocolConfiguration).Returns(
                new List<ProtocolConfiguration>
                {
                    new ProtocolConfiguration(CommsProtocol.SAS),
                    new ProtocolConfiguration(CommsProtocol.MGAM),
                    new ProtocolConfiguration(CommsProtocol.HHR)
                });
            IFundTransferProvider fundTransferProvider = new FundTransferProvider();
            IService registeredWatOnProvider = null;
            IService registeredWatOffProvider = null;

            // Action
            _protocolList.ForEach(x =>
            {
                var watOnProvider = new Mock<IWatTransferOnProvider>().Object;
                var watOffProvider = new Mock<IWatTransferOffProvider>().Object;
                // When the add service is called, it will assign the registeredWatOnProvider/registeredWatOffProvider provider
                // to the one passed. Please note that only one AddService() will be called per provider type depending on
                // which protocol is handling fund transfer. In this test, no protocol is handling the fund transfer, hence it would never be called.
                MoqServiceManager.Instance.Setup(c => c.AddService(watOnProvider))
                    .Callback<IService>(e => registeredWatOnProvider = e);
                MoqServiceManager.Instance.Setup(c => c.AddService(watOffProvider))
                    .Callback<IService>(e => registeredWatOffProvider = e);
                MoqServiceManager.Instance.Setup(c => c.RemoveService(watOnProvider)).Callback<IService>(e =>
                {
                    if (registeredWatOnProvider == e)
                    {
                        registeredWatOnProvider = null;
                    }
                });
                MoqServiceManager.Instance.Setup(c => c.RemoveService(watOffProvider)).Callback<IService>(e =>
                {
                    if (registeredWatOffProvider == e)
                    {
                        registeredWatOffProvider = null;
                    }
                });
                fundTransferProvider.Register(x, watOnProvider);
                fundTransferProvider.Register(x, watOffProvider);
            });

            MoqServiceManager.Instance.Setup(x => x.TryGetService<IWatTransferOnProvider>())
                .Returns(registeredWatOnProvider as IWatTransferOnProvider);
            MoqServiceManager.Instance.Setup(x => x.TryGetService<IWatTransferOffProvider>())
                .Returns(registeredWatOffProvider as IWatTransferOffProvider);

            // Assert
            // This makes sure that the AddService() was never called by FundTransferProvider and hence
            // GetWatTransferOnProvider & GetWatTransferOffProvider will return null.
            Assert.AreEqual(fundTransferProvider.GetWatTransferOnProvider(), null);
            Assert.AreEqual(fundTransferProvider.GetWatTransferOffProvider(), null);
        }

        private IEnumerable<ProtocolConfiguration> GenerateMultiConfiguration(CommsProtocol validationProtocol,
            CommsProtocol fundTransferProtocol,
            CommsProtocol progressiveProtocol)
        {
            var multiProtocolConfiguration = new List<ProtocolConfiguration>();
            _protocolList.ForEach(x => multiProtocolConfiguration.Add(new ProtocolConfiguration(EnumParser.ParseOrThrow<CommsProtocol>(x))));
            multiProtocolConfiguration.ForEach(config =>
            {
                config.IsValidationHandled = config.Protocol == validationProtocol;
                config.IsFundTransferHandled = config.Protocol == fundTransferProtocol;
                config.IsProgressiveHandled = config.Protocol == progressiveProtocol;
            });
            return multiProtocolConfiguration;
        }
    }
}