namespace Aristocrat.Monaco.Sas.Tests.Consumers
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Sas.Client;
    using Contracts.Client;
    using Hardware.Contracts.TowerLight;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Consumers;
    using Test.Common;

    [TestClass]
    public class SystemEventHandlerTest
    {
        private static readonly Guid TestOperatorDisabledGuid = new Guid("{9705470A-33DA-4c7e-8F0A-886991B2AA4F}");

        private Mock<ISasHost> _sasHostMock;
        private Mock<ISasExceptionHandler> _exceptionHandler;
        private Mock<IPropertiesManager> _propertiesManagerMock;
        private Mock<ISystemDisableManager> _systemDisableManagerMock;
        private Mock<ITowerLight> _lightMock;
        private Mock<ISasVoucherInProvider> _sasVoucherInProvider;
        private SystemEventHandler _target;

        private static IEnumerable<object[]> ConstructorTestData =>
            new List<object[]>
            {
                new object[]
                {
                    null, new Mock<ISasExceptionHandler>().Object,
                    new Mock<ISystemDisableManager>().Object, new Mock<ITowerLight>().Object,
                    new Mock<ISasVoucherInProvider>().Object
                },
                new object[]
                {
                    new Mock<ISasHost>().Object, null,
                    new Mock<ISystemDisableManager>().Object, new Mock<ITowerLight>().Object,
                    new Mock<ISasVoucherInProvider>().Object
                },
                new object[]
                {
                    new Mock<ISasHost>().Object, new Mock<ISasExceptionHandler>().Object,
                    null, new Mock<ITowerLight>().Object,
                    new Mock<ISasVoucherInProvider>().Object
                },
                new object[]
                {
                    new Mock<ISasHost>().Object, new Mock<ISasExceptionHandler>().Object,
                    new Mock<ISystemDisableManager>().Object, null,
                    new Mock<ISasVoucherInProvider>().Object
                },
                new object[]
                {
                    new Mock<ISasHost>().Object, new Mock<ISasExceptionHandler>().Object,
                    new Mock<ISystemDisableManager>().Object,
                    new Mock<ITowerLight>().Object, null
                }
            };

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);

            _sasHostMock = new Mock<ISasHost>(MockBehavior.Default);
            _exceptionHandler = new Mock<ISasExceptionHandler>(MockBehavior.Default);
            _propertiesManagerMock = new Mock<IPropertiesManager>(MockBehavior.Default);
            _systemDisableManagerMock = new Mock<ISystemDisableManager>(MockBehavior.Default);
            _lightMock = new Mock<ITowerLight>(MockBehavior.Strict);
            _sasVoucherInProvider = new Mock<ISasVoucherInProvider>(MockBehavior.Default);
            _target = new SystemEventHandler(
                _sasHostMock.Object,
                _exceptionHandler.Object,
                _systemDisableManagerMock.Object,
                _lightMock.Object,
                _sasVoucherInProvider.Object);
        }

        [DynamicData(nameof(ConstructorTestData))]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenAnyParameterNullExpectException(
            ISasHost sasHost,
            ISasExceptionHandler exceptionHandler,
            ISystemDisableManager systemDisableManager,
            ITowerLight light,
            ISasVoucherInProvider sasVoucherInProvider)
        {
            _target = new SystemEventHandler(
                sasHost,
                exceptionHandler,
                systemDisableManager,
                light,
                sasVoucherInProvider);
        }

        [TestMethod]
        public void OnSasStartedWhenFirstBootAndLightOnTest()
        {
            _lightMock.SetupGet(x => x.IsLit).Returns(true);

            _target.OnSasStarted();

            _lightMock.Verify();
            _exceptionHandler.Verify(m => m.ReportException(It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == GeneralExceptionCode.ChangeLampOn)), Times.Once);
        }

        [TestMethod]
        public void OnSasStartedWhenSecondLoadAndLightOffTest()
        {
            _lightMock.SetupGet(x => x.IsLit).Returns(false);

            _target.OnSasStarted();

            _lightMock.Verify();
            _exceptionHandler.Verify(m => m.ReportException(It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == GeneralExceptionCode.ChangeLampOn)), Times.Never);
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void OnSystemBootedHandler(bool isMemoryCleared)
        {
            _target.OnPlatformBooted(isMemoryCleared);
            _exceptionHandler.Verify(m => m.ReportException(It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == GeneralExceptionCode.EgmPowerLost)), Times.Once);
            _exceptionHandler.Verify(m => m.ReportException(It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == GeneralExceptionCode.EgmPowerApplied)), Times.Once);
            _exceptionHandler.Verify(m => m.ReportException(It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == GeneralExceptionCode.GamingMachineSoftMetersReset)), isMemoryCleared ? Times.Once() : Times.Never());
        }

        [TestMethod]
        public void OnSystemEnabledTest()
        {
            // The system is just disabled when called
            _systemDisableManagerMock.Setup(p => p.IsDisabled).Returns(true);
            _target.OnSystemEnabled();
            _sasHostMock.Verify(m => m.SetLegacyBonusEnabled(It.IsAny<bool>()), Times.Never);

            // The system is still enabled when called.
            _systemDisableManagerMock.Reset();
            _systemDisableManagerMock.Setup(p => p.IsDisabled).Returns(false);
            _target.OnSystemEnabled();
            _sasHostMock.Verify(m => m.SetLegacyBonusEnabled(true), Times.Once);
            _sasHostMock.Verify(m => m.SetLegacyBonusEnabled(false), Times.Never);
        }

        [TestMethod]
        public void OnSystemDisabledTest()
        {
            // when the system is not disabled by the operator.
            _systemDisableManagerMock.Setup(p => p.CurrentDisableKeys).Returns(new List<Guid>());
            _target.OnSystemDisabled();
            _exceptionHandler.Verify(m => m.ReportException(It.Is<ISasExceptionCollection>(ex => ex.ExceptionCode == GeneralExceptionCode.GamingMachineOutOfServiceByOperator)), Times.Never);
            _sasVoucherInProvider.Verify(m => m.OnSystemDisabled(), Times.Once);
            _sasHostMock.Verify(m => m.SetLegacyBonusEnabled(false), Times.Once);
            _sasHostMock.Verify(m => m.SetLegacyBonusEnabled(true), Times.Never);

            // when the system is disabled by the operator.
            _systemDisableManagerMock.Reset();
            _exceptionHandler.Reset();
            _sasHostMock.Reset();
            _sasVoucherInProvider.Reset();
            _systemDisableManagerMock.Setup(p => p.CurrentDisableKeys).Returns(new List<Guid> { TestOperatorDisabledGuid });
            _target.OnSystemDisabled();
            _sasVoucherInProvider.Verify(m => m.OnSystemDisabled(), Times.Once);
            _sasHostMock.Verify(m => m.SetLegacyBonusEnabled(false), Times.Once);
            _sasHostMock.Verify(m => m.SetLegacyBonusEnabled(true), Times.Never);
        }
    }
}
