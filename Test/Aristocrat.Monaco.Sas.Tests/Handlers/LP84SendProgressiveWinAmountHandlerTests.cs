namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts.Extensions;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.SASProperties;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Progressive;
    using Sas.Handlers;

    /// <summary>
    ///     Contains the unit tests for the LP84SendProgressiveWinAmountHandlerTests class
    /// </summary>
    [TestClass]
    public class LP84SendProgressiveWinAmountHandlerTests
    {
        private Mock<IProgressiveWinDetailsProvider> _progressiveProvider;
        private LP84SendProgressiveWinAmountHandler _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _progressiveProvider = new Mock<IProgressiveWinDetailsProvider>(MockBehavior.Default);
            _target = new LP84SendProgressiveWinAmountHandler(_progressiveProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullConstructorTest()
        {
            // test will fail if exception wasn't thrown 
            _target = new LP84SendProgressiveWinAmountHandler(null);
        }

        [TestMethod]
        public void HandleTest()
        {
            var expected = new SendProgressiveWinAmountResponse { LevelId = 1, GroupId = 1, WinAmount = 100 };
            var jackpots = new List<JackpotInfo>
            {
                new JackpotInfo { DeviceId = 1, LevelId = 1, WinAmount = 100000 }
            };

            _progressiveProvider.Setup(x => x.GetLastProgressiveWin()).Returns(
                new ProgressiveWinDetails
                {
                    LevelId = expected.LevelId,
                    GroupId = expected.GroupId,
                    WinAmount = expected.WinAmount.CentsToMillicents()
                });

            var actual = _target.Handle(new LongPollData());
            Assert.AreEqual(expected.GroupId, actual.GroupId);
            Assert.AreEqual(expected.WinAmount, actual.WinAmount);
            Assert.AreEqual(expected.LevelId, actual.LevelId);
        }
    }
}