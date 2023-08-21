namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using Accounting.Contracts;
    using Application.Contracts;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.SASProperties;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Handlers;
    using Test.Common;

    /// <summary>
    ///     Contains the unit tests for the LPB3SendTokenDenominationHandlerTest class
    /// </summary>
    [TestClass]
    public class LPB3SendTokenDenominationHandlerTest
    {
        private const int TokenDenomination = 3;
        private const byte TokenDenominationCode = 0x18;
        private const byte TokenUnsupportedDenominationCode = 0x00;
        private LPB3SendTokenDenominationHandler _target;
        private Mock<IPropertiesManager> _propertiesManager;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _target = new LPB3SendTokenDenominationHandler(_propertiesManager.Object);
            _propertiesManager.Setup(m => m.GetProperty(SasProperties.TokenDenominationKey, 0)).Returns(TokenDenomination);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullIPropertiesManagerTest()
        {
            // test will fail if exception wasn't thrown 
            _target = new LPB3SendTokenDenominationHandler(null);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SendTokenDenomination));
        }

        [DataTestMethod]
        [DataRow(true, false, TokenDenominationCode)]
        [DataRow(false, true, TokenDenominationCode)]
        [DataRow(true, true, TokenDenominationCode)]
        [DataRow(false, false, TokenUnsupportedDenominationCode)]
        public void HandleSupportedTest(bool coinSupported, bool hopperSupported, byte expectedTokenDenominationCode)
        {
            _propertiesManager.Setup(m => m.GetProperty(SasProperties.CoinAcceptorSupportedKey, false)).Returns(coinSupported);
            _propertiesManager.Setup(m => m.GetProperty(SasProperties.HopperSupportedKey, false)).Returns(hopperSupported);

            var expected = new LongPollReadSingleValueResponse<byte>(expectedTokenDenominationCode);
            var actual = _target.Handle(new LongPollData());

            Assert.AreEqual(actual.Data, expected.Data);
        }
    }
}
