namespace Aristocrat.Monaco.Sas.Tests.Consumers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.Monaco.Sas.Storage.Models;
    using Aristocrat.Sas.Client;
    using Contracts.SASProperties;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Progressive;
    using Sas.Consumers;
    using Sas.Exceptions;
    using Test.Common;

    [TestClass]
    public class GameEndedConsumerTest
    {
        private GameEndedConsumer _target;
        private Mock<ISasExceptionHandler> _exceptionHandler;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IProgressiveWinDetailsProvider> _progressiveWinDetailsProvider;

        [TestInitialize]
        public void Initialize()
        {
            _exceptionHandler = new Mock<ISasExceptionHandler>(MockBehavior.Strict);
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
            _progressiveWinDetailsProvider = new Mock<IProgressiveWinDetailsProvider>(MockBehavior.Default);
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);

            _target = new GameEndedConsumer(
                _exceptionHandler.Object,
                _propertiesManager.Object,
                _progressiveWinDetailsProvider.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [DataTestMethod]
        [DataRow(true, false, false)]
        [DataRow(false, true, false)]
        [DataRow(false, false, true)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorTest(bool nullException, bool nullProperties, bool nullProgressWinProvider)
        {
            _target = new GameEndedConsumer(
                nullException ? null : _exceptionHandler.Object,
                nullProperties ? null : _propertiesManager.Object,
                nullProgressWinProvider ? null : _progressiveWinDetailsProvider.Object);
        }

        [TestMethod]
        public void ConsumeTest()
        {
            var expectedResult = new GameEndedExceptionBuilder(10_00, 1);
            GameEndedExceptionBuilder actual = null;
            _exceptionHandler.Setup(m => m.ReportException(It.IsAny<Func<byte, ISasExceptionCollection>>(), GeneralExceptionCode.GameHasEnded))
                .Callback((Func<byte, ISasExceptionCollection> g, GeneralExceptionCode _) => actual = g.Invoke(1) as GameEndedExceptionBuilder)
                .Verifiable();

            var gameId = 1;
            var denomination = 1_00_000;  // $1 in millicents
            const string wagerCategory = "1";
            var historyLog = new Mock<IGameHistoryLog>(MockBehavior.Strict);

            historyLog.Setup(m => m.TotalWon).Returns(10_00);  // $10.00 win
            historyLog.Setup(m => m.ShallowCopy()).Returns(historyLog.Object);
            historyLog.Setup(m => m.Jackpots).Returns(Enumerable.Empty<JackpotInfo>());
            _propertiesManager.Setup(x => x.GetProperty(SasProperties.SasHosts, It.IsAny<object>()))
                .Returns(new List<Host> { new Host { AccountingDenom = 1 }, new Host { AccountingDenom = 1 } });

            var @event = new GameEndedEvent(gameId, denomination, wagerCategory, historyLog.Object);

            _target.Consume(@event);

            // we said credits are $1 and we won $10
            Assert.IsNotNull(actual);
            CollectionAssert.AreEquivalent(expectedResult, actual);

            _exceptionHandler.Verify();
        }
    }
}
