
namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using Application.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Kernel;
    using Contracts.SASProperties;
    using Aristocrat.Monaco.Sas.Handlers;
    using Aristocrat.Sas.Client;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;
    using System.Collections.Generic;
    using System;

    /// <summary>
    ///     Tests for the LPB1SendCurrentPlayerDenomHandlerTest class
    /// </summary>
    [TestClass]
    public class LPB1SendCurrentPlayerDenomHandlerTest
    {
        private const long ExpectedDenom = 1000;
        private LPB1SendCurrentPlayerDenomHandler _target;
        private Mock<IPropertiesManager> _propertiesManager;



        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            var denomMeter = new Mock<IMeter>();
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);

            _propertiesManager.Setup(pm => pm.GetProperty(It.Is<string>(p => p.Equals(GamingConstants.SelectedDenom)),
                It.IsAny<int>())).Returns(ExpectedDenom);

            _propertiesManager.Setup(pm => pm.GetProperty(It.Is<string>(p => p.Equals(GamingConstants.IsGameRunning)),
                false)).Returns(true);

            _propertiesManager.Setup(pm => pm.GetProperty(It.Is<string>(p => p.Equals(GamingConstants.SelectedGameId)),
                It.IsAny<int>())).Returns(1);

            _propertiesManager.Setup(pm => pm.GetProperty(It.Is<string>(p => p.Equals(GamingConstants.SelectedDenom)),
                It.IsAny<long>())).Returns(1L);

            var game = new Mock<IGameDetail>();
            game.SetupGet(g => g.Id).Returns(1);
            var denom = new Mock<IDenomination>();
            denom.SetupGet(d => d.Value).Returns(1);
            game.SetupGet(g => g.Denominations).Returns(new List<IDenomination> { denom.Object });

            _propertiesManager.Setup(pm => pm.GetProperty(It.Is<string>(p => p.Equals(GamingConstants.Games)),
                null)).Returns(new List<IGameDetail>(){ game.Object });

            var testManager = new TestPropertiesManager(_propertiesManager.Object);

            var history = new Mock<IGameHistory>();
            history.SetupGet(d => d.IsDiagnosticsActive).Returns(false);

            _target = new LPB1SendCurrentPlayerDenomHandler(testManager, history.Object);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SendCurrentPlayerDenominations));
        }

        [TestMethod]
        public void GoodDataTest()
        {
            const byte expectedCode = 0x01;
            // test good variables
            _propertiesManager.Setup(pm => pm.GetProperty(It.Is<string>(p => p.Equals(SasProperties.MultipleDenominationSupportedKey)),
                It.IsAny<bool>())).Returns(true);

            var expected = _target.Handle(new LongPollData());
            Assert.IsNotNull(expected);
            Assert.IsTrue(expected.Data == expectedCode);
        }

        [TestMethod]
        public void MultiDenomNotSupportedTest()
        {
            // test multi not supported
            _propertiesManager.Setup(pm => pm.GetProperty(It.Is<string>(p => p.Equals(SasProperties.MultipleDenominationSupportedKey)),
                It.IsAny<bool>())).Returns(false);

            var expected = _target.Handle(new LongPollData());
            Assert.IsNull(expected);
        }

        public class TestPropertiesManager : IPropertiesManager
        {
            private readonly IPropertiesManager _propertyManager;
            public TestPropertiesManager(IPropertiesManager property)
            {
                _propertyManager = property;
            }

            public string Name => _propertyManager.Name;

            public ICollection<Type> ServiceTypes => _propertyManager.ServiceTypes;

            public void AddPropertyProvider(IPropertyProvider provider)
            {
                _propertyManager.AddPropertyProvider(provider);
            }

            public object GetProperty(string propertyName, object defaultValue)
            {
                return _propertyManager.GetProperty(propertyName, defaultValue);
            }

            public void Initialize()
            {
                _propertyManager.Initialize();
            }

            public void SetProperty(string propertyName, object propertyValue)
            {
                _propertyManager.SetProperty(propertyName, propertyValue);
            }

            public void SetProperty(string propertyName, object propertyValue, bool isConfig)
            {
                _propertyManager.SetProperty(propertyName, propertyValue, isConfig);
            }
        }
    }
}

