namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
    using Aristocrat.Monaco.Test.Common;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Gaming.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Handlers;

    /// <summary>
    /// Contains unit tests for LP8FSendPhysicalReelStopInformationHandler
    /// </summary>
    [TestClass]
    public class LP8FSendPhysicalReelStopInformationHandlerTest
    {
        private LP8FSendPhysicalReelStopInformationHandler _target;
        private Mock<IReelController> _reelController;

        [TestInitialize]
        public void TestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _reelController = MoqServiceManager.CreateAndAddService<IReelController>(MockBehavior.Strict);

            _target = new LP8FSendPhysicalReelStopInformationHandler();
        }

        /// <summary>
        ///     Cleans up class members after execution of a TestMethod.
        /// </summary>
        [TestCleanup]
        public void CleanUp()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SendPhysicalReelStopInformation));
        }

        [TestMethod]
        public void HandleThreeHomedTest()
        {
            _reelController.Setup(x => x.ConnectedReels).Returns(new List<int> { 1, 2, 3 });
            _reelController.Setup(x => x.ReelStates)
                           .Returns(new Dictionary<int, ReelLogicalState>() { { 1, ReelLogicalState.IdleAtStop },
                                                                              { 2, ReelLogicalState.IdleAtStop },
                                                                              { 3, ReelLogicalState.IdleAtStop } });
            _reelController.Setup(x => x.Steps).Returns(new Dictionary<int, int>() { { 1, 0 },
                                                                                     { 2, 0 },
                                                                                     { 3, 0 } });

            var result = _target.Handle(null);

            Assert.AreEqual(0, result.Reel1);
            Assert.AreEqual(0, result.Reel2);
            Assert.AreEqual(0, result.Reel3);
            Assert.AreEqual(0xFF, result.Reel4);
            Assert.AreEqual(0xFF, result.Reel5);
            Assert.AreEqual(0xFF, result.Reel6);
            Assert.AreEqual(0xFF, result.Reel7);
            Assert.AreEqual(0xFF, result.Reel8);
            Assert.AreEqual(0xFF, result.Reel9);
        }

        [TestMethod]
        public void HandleFiveFaultedTest()
        {
            _reelController.Setup(x => x.ConnectedReels).Returns(new List<int> { 1, 2, 3, 4, 5 });
            _reelController.Setup(x => x.ReelStates)
                           .Returns(new Dictionary<int, ReelLogicalState>() { { 1, ReelLogicalState.Tilted },
                                                                              { 2, ReelLogicalState.Disconnected },
                                                                              { 3, ReelLogicalState.IdleUnknown },
                                                                              { 4, ReelLogicalState.Spinning },
                                                                              { 5, ReelLogicalState.Stopping } });
            _reelController.Setup(x => x.Steps).Returns(new Dictionary<int, int>() { { 1, 0 },
                                                                                     { 2, 0 },
                                                                                     { 3, 0 },
                                                                                     { 4, 0 },
                                                                                     { 5, 0 } });

            var result = _target.Handle(null);

            Assert.AreEqual(0xFF, result.Reel1);
            Assert.AreEqual(0xFF, result.Reel2);
            Assert.AreEqual(0xFF, result.Reel3);
            Assert.AreEqual(0xFF, result.Reel4);
            Assert.AreEqual(0xFF, result.Reel5);
            Assert.AreEqual(0xFF, result.Reel6);
            Assert.AreEqual(0xFF, result.Reel7);
            Assert.AreEqual(0xFF, result.Reel8);
            Assert.AreEqual(0xFF, result.Reel9);
        }

        [TestMethod]
        public void HandleFiveSpunTest()
        {
            _reelController.Setup(x => x.ConnectedReels).Returns(new List<int> { 1, 2, 3, 4, 5 });
            _reelController.Setup(x => x.ReelStates)
                           .Returns(new Dictionary<int, ReelLogicalState>() { { 1, ReelLogicalState.IdleAtStop },
                                                                              { 2, ReelLogicalState.IdleAtStop },
                                                                              { 3, ReelLogicalState.IdleAtStop },
                                                                              { 4, ReelLogicalState.IdleAtStop },
                                                                              { 5, ReelLogicalState.IdleAtStop } });
            _reelController.Setup(x => x.Steps).Returns(new Dictionary<int, int>() { { 1, 10 },
                                                                                     { 2, 20 },
                                                                                     { 3, 30 },
                                                                                     { 4, 40 },
                                                                                     { 5, 50 } });

            var result = _target.Handle(null);

            Assert.AreEqual(10, result.Reel1);
            Assert.AreEqual(20, result.Reel2);
            Assert.AreEqual(30, result.Reel3);
            Assert.AreEqual(40, result.Reel4);
            Assert.AreEqual(50, result.Reel5);
            Assert.AreEqual(0xFF, result.Reel6);
            Assert.AreEqual(0xFF, result.Reel7);
            Assert.AreEqual(0xFF, result.Reel8);
            Assert.AreEqual(0xFF, result.Reel9);
        }
    }
}
