﻿namespace Aristocrat.Monaco.G2S.Tests.Handlers.Progressive
{
    using System;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Aristocrat.G2S.Client.Devices;
    using Gaming.Contracts;
    using G2S.Handlers.Progressive;
    using Gaming.Contracts.Progressives;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Aristocrat.Monaco.G2S.Services.Progressive;

    [TestClass]
    public class ProgressiveProfileCommandBuilderTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullProgressiveLevelProviderExpectException()
        {
            var builder = new ProgressiveProfileCommandBuilder(null, null, null, null, null);

            Assert.IsNull(builder);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullTransactionHistoryExpectException()
        {
            var progressive = new Mock<IProgressiveLevelProvider>();
            var builder = new ProgressiveProfileCommandBuilder(progressive.Object, null, null, null, null);

            Assert.IsNull(builder);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullGameProviderExpectException()
        {
            var progressive = new Mock<IProgressiveLevelProvider>();
            var transactions = new Mock<ITransactionHistory>();

            var builder = new ProgressiveProfileCommandBuilder(progressive.Object, transactions.Object, null, null, null);

            Assert.IsNotNull(builder);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullProtocolLinkedProgressiveAdapterExpectException()
        {
            var progressive = new Mock<IProgressiveLevelProvider>();
            var transactions = new Mock<ITransactionHistory>();
            var games = new Mock<IGameProvider>();

            var builder = new ProgressiveProfileCommandBuilder(progressive.Object, transactions.Object, games.Object, null, null);

            Assert.IsNotNull(builder);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullProgressiveDeviceManagerExpectException()
        {
            var progressive = new Mock<IProgressiveLevelProvider>();
            var transactions = new Mock<ITransactionHistory>();
            var games = new Mock<IGameProvider>();
            var protocolLinkedProgressiveAdapter = new Mock<IProtocolLinkedProgressiveAdapter>();

            var builder = new ProgressiveProfileCommandBuilder(progressive.Object, transactions.Object, games.Object, protocolLinkedProgressiveAdapter.Object, null);

            Assert.IsNotNull(builder);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var progressive = new Mock<IProgressiveLevelProvider>();
            var transactions = new Mock<ITransactionHistory>();
            var games = new Mock<IGameProvider>();
            var protocolLinkedProgressiveAdapter = new Mock<IProtocolLinkedProgressiveAdapter>();
            var progressiveDeviceManager = new Mock<IProgressiveDeviceManager>();

            var builder = new ProgressiveProfileCommandBuilder(progressive.Object, transactions.Object, games.Object, protocolLinkedProgressiveAdapter.Object, progressiveDeviceManager.Object);
            
            Assert.IsNotNull(builder);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task WhenBuildWithNullDeviceExpectException()
        {
            var progressive = new Mock<IProgressiveLevelProvider>();
            var transactions = new Mock<ITransactionHistory>();
            var games = new Mock<IGameProvider>();
            var protocolLinkedProgressiveAdapter = new Mock<IProtocolLinkedProgressiveAdapter>();
            var progressiveDeviceManager = new Mock<IProgressiveDeviceManager>();

            var builder = new ProgressiveProfileCommandBuilder(progressive.Object, transactions.Object, games.Object, protocolLinkedProgressiveAdapter.Object, progressiveDeviceManager.Object);

            await builder.Build(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task WhenBuildWithNullProgressiveProfileExpectException()
        {
            var progressive = new Mock<IProgressiveLevelProvider>();
            var transactions = new Mock<ITransactionHistory>();
            var games = new Mock<IGameProvider>();
            var protocolLinkedProgressiveAdapter = new Mock<IProtocolLinkedProgressiveAdapter>();
            var progressiveDeviceManager = new Mock<IProgressiveDeviceManager>();

            var builder = new ProgressiveProfileCommandBuilder(progressive.Object, transactions.Object, games.Object, protocolLinkedProgressiveAdapter.Object, progressiveDeviceManager.Object);

            var device = new Mock<IProgressiveDevice>();

            await builder.Build(device.Object, null);
        }

        // TODO: Uncomment this test when g2s is fixed
        //[TestMethod]
        //public async Task WhenBuildExpectSuccess()
        //{
        //    var progressive = new Mock<IProgressiveProvider>();

        //    var progressiveId = 0;
        //    var noProgressiveInfo = 3000;
        //    var noResponseTimer = 3000;
        //    var timeToLive = 3000;
        //    var configDateTime = default(DateTime);
        //    var minLogentries = 35;

        //    var game = new Mock<IGameProvider>();
        //    var builder = new ProgressiveProfileCommandBuilder(progressive.Object, game.Object);

        //    var device = new Mock<IProgressiveDevice>();
        //    var profile = new progressiveProfile();

        //    device.SetupAllProperties();
        //    device.Setup(p => p.TimeToLive).Returns(timeToLive);
        //    device.Setup(p => p.NoProgressiveInfo).Returns(noProgressiveInfo);
        //    device.Setup(p => p.NoResponseTimer).Returns(new TimeSpan(0, 0, 0, 0, noResponseTimer));
        //    device.Setup(p => p.ConfigComplete).Returns(true);
        //    device.Setup(p => p.RestartStatus).Returns(true);
        //    device.Setup(p => p.MinLogEntries).Returns(minLogentries);
        //    await builder.Build(device.Object, profile);

        //    Assert.AreEqual(profile.configurationId, device.Object.ConfigurationId);
        //    Assert.AreEqual(profile.useDefaultConfig, device.Object.UseDefaultConfig);
        //    Assert.AreEqual(profile.requiredForPlay, device.Object.RequiredForPlay);
        //    Assert.AreEqual(profile.progId, progressiveId);
        //    Assert.AreEqual(profile.noProgInfo, noProgressiveInfo);
        //    Assert.IsNotNull(profile.levelProfile);
        //    Assert.AreEqual(profile.noResponseTimer, noResponseTimer);
        //    Assert.AreEqual(profile.timeToLive, timeToLive);
        //    Assert.IsTrue(profile.configDateTimeSpecified);
        //    Assert.IsTrue(profile.configComplete);
        //    Assert.AreEqual(profile.configDateTime, configDateTime);
        //    Assert.AreEqual(profile.minLogEntries, minLogentries);
        //    Assert.IsTrue(profile.restartStatus);
        //}
    }
}