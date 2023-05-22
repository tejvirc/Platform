namespace Aristocrat.Monaco.G2S.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.G2S.DisableProvider;
    using Aristocrat.Monaco.Kernel;
    using FMOD;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class G2SDisableProviderTests
    {
        private const int WaitTime = 1000;
        private Mock<ISystemDisableManager> _systemDisableManager;
        private Mock<IMessageDisplay> _messageDisplay;
        private G2SDisableProvider _target;

        private static IEnumerable<object[]> DisableTestData => new List<object[]>
        {
            new object[]
            {
                SystemDisablePriority.Normal,
                new List<(Guid, G2SDisableStates)>
                {
                    (G2S.Constants.VertexOfflineKey, G2SDisableStates.CommsOffline)
                }
            },
            new object[]
            {
                SystemDisablePriority.Normal,
                new List<(Guid, G2SDisableStates)>
                {
                    (G2S.Constants.VertexLevelMismatchKey, G2SDisableStates.LevelMismatch)
                }
            },
            new object[]
            {
                SystemDisablePriority.Normal,
                new List<(Guid, G2SDisableStates)>
                {
                    (G2S.Constants.VertexStateDisabledKey, G2SDisableStates.ProgressiveState)
                }
            },
            new object[]
            {
                SystemDisablePriority.Normal,
                new List<(Guid, G2SDisableStates)>
                {
                    (G2S.Constants.VertexUpdateNotReceivedKey, G2SDisableStates.ProgressiveValueNotReceived)
                }
            },
            new object[]
            {
                SystemDisablePriority.Normal,
                new List<(Guid, G2SDisableStates)>
                {
                    (G2S.Constants.VertexMeterRollbackKey, G2SDisableStates.ProgressiveMeterRollback)
                }
            },
            new object[]
            {
                SystemDisablePriority.Normal,
                new List<(Guid, G2SDisableStates)>
                {
                    (G2S.Constants.VertexOfflineKey, G2SDisableStates.CommsOffline),
                    (G2S.Constants.VertexLevelMismatchKey, G2SDisableStates.LevelMismatch),
                    (G2S.Constants.VertexStateDisabledKey, G2SDisableStates.ProgressiveState),
                    (G2S.Constants.VertexUpdateNotReceivedKey, G2SDisableStates.ProgressiveValueNotReceived),
                    (G2S.Constants.VertexMeterRollbackKey, G2SDisableStates.ProgressiveMeterRollback)
                }
            }
        };
        
        private static IEnumerable<object[]> EnableTestData => new List<object[]>
        {
            new object[]
            {
                new[] { G2SDisableStates.CommsOffline },
                new List<Guid> { G2S.Constants.VertexOfflineKey }
            },
            new object[]
            {
                new[] { G2SDisableStates.LevelMismatch },
                new List<Guid> { G2S.Constants.VertexLevelMismatchKey }
            },
            new object[]
            {
                new[] { G2SDisableStates.ProgressiveState },
                new List<Guid> { G2S.Constants.VertexStateDisabledKey }
            },
            new object[]
            {
                new[] { G2SDisableStates.ProgressiveValueNotReceived },
                new List<Guid> { G2S.Constants.VertexUpdateNotReceivedKey }
            },
            new object[]
            {
                new[] { G2SDisableStates.ProgressiveMeterRollback },
                new List<Guid> { G2S.Constants.VertexMeterRollbackKey }
            },
            new object[]
            {
                new[] { G2SDisableStates.CommsOffline, G2SDisableStates.LevelMismatch },
                new List<Guid>
                {
                    G2S.Constants.VertexOfflineKey,
                    G2S.Constants.VertexLevelMismatchKey
                }
            }
        };
        

        [TestInitialize]
        public void MyTestInitialize()
        {
            _systemDisableManager = new Mock<ISystemDisableManager>(MockBehavior.Default);
            _messageDisplay = new Mock<IMessageDisplay>(MockBehavior.Default);
            _target = CreateG2SDisableProvider();
        }

        [DataRow(true, false, DisplayName = "Null System Disable Manager")]
        [DataRow(false, true, DisplayName = "Null Message Display")]
        [ExpectedException(typeof(ArgumentNullException))]
        [DataTestMethod]
        public void NullConstructorTest(
            bool nullSystemDisableManager,
            bool nullMessageDisplay)
        {
            _target = CreateG2SDisableProvider(
                nullSystemDisableManager,
                nullMessageDisplay);
        }

        [TestMethod]
        public void IsDisableStateActiveTest()
        {
            Assert.IsTrue(
                _target.Disable(SystemDisablePriority.Immediate, G2SDisableStates.CommsOffline).Wait(WaitTime));

            Assert.IsTrue(_target.IsDisableStateActive(G2SDisableStates.CommsOffline));
            Assert.IsFalse(_target.IsDisableStateActive(G2SDisableStates.LevelMismatch));
        }

        [DynamicData(nameof(DisableTestData))]
        [DataTestMethod]
        public void DisableTest(SystemDisablePriority priority, IList<(Guid guid, G2SDisableStates state)> disableGuids)
        {
            foreach (var (guid, _) in disableGuids)
            {
                _systemDisableManager.Setup(x => x.Disable(guid, priority, It.IsAny<Func<string>>(), null))
                    .Verifiable();
            }

            var disableStates = disableGuids.Select(x => x.state).ToArray();
            var savedStates = disableStates.Aggregate((current, state) => current | state);
            Assert.IsTrue(_target.Disable(priority, disableStates).Wait(WaitTime));

            _systemDisableManager.Verify();
        }

        [DynamicData(nameof(DisableTestData))]
        [DataTestMethod]
        public void SoftDisableTest(SystemDisablePriority priority, IList<(Guid guid, G2SDisableStates state)> disableGuids)
        {
            foreach (var (guid, state) in disableGuids)
            {
                Assert.IsTrue(_target.Disable(priority, state, false).Wait(WaitTime));
                _messageDisplay.Verify(
                    x => x.DisplayMessage(
                        It.Is<DisplayableMessage>(
                            message => message.Id == guid && message.Priority == DisplayableMessagePriority.Normal &&
                                       message.Classification == DisplayableMessageClassification.SoftError)));

                Assert.IsTrue(_target.IsSoftErrorStateActive(state));
            }
        }

        [TestMethod]
        public void ClearSoftDisableOnLockupDisableTest1()
        {
            _target = CreateG2SDisableProvider();
            var state = G2SDisableStates.CommsOffline;
            var guid = G2S.Constants.VertexOfflineKey;
            var priority = SystemDisablePriority.Immediate;

            //initiate soft disable on a state
            Assert.IsTrue(_target.Disable(priority, state, false).Wait(WaitTime));
            _messageDisplay.Verify(
                x => x.DisplayMessage(
                    It.Is<DisplayableMessage>(
                        message => message.Id == guid && message.Priority == DisplayableMessagePriority.Normal &&
                                   message.Classification == DisplayableMessageClassification.SoftError)));

            Assert.IsTrue(_target.IsSoftErrorStateActive(state));
            Assert.IsFalse(_target.IsDisableStateActive(state));

            //then initiate lockup for same condition. verify states switch
            _systemDisableManager.Setup(x => x.Disable(guid, priority, It.IsAny<Func<string>>(), null)).Verifiable();
            _messageDisplay.Setup(x => x.RemoveMessage(guid)).Verifiable();
            Assert.IsTrue(_target.Disable(priority, state, true).Wait(WaitTime));
            _systemDisableManager.Verify();
            _messageDisplay.Verify();

            Assert.IsFalse(_target.IsSoftErrorStateActive(state));
            Assert.IsTrue(_target.IsDisableStateActive(state));
        }

        [TestMethod]
        public void ClearSoftDisableOnLockupDisableTest2()
        {
            _target = CreateG2SDisableProvider();
            var state = G2SDisableStates.CommsOffline;
            var guid = G2S.Constants.VertexOfflineKey;
            var priority = SystemDisablePriority.Immediate;

            //initiate soft disable on a state
            Assert.IsTrue(_target.Disable(priority, state, false).Wait(WaitTime));
            _messageDisplay.Verify(
                x => x.DisplayMessage(
                    It.Is<DisplayableMessage>(
                        message => message.Id == guid && message.Priority == DisplayableMessagePriority.Normal &&
                                   message.Classification == DisplayableMessageClassification.SoftError)));

            Assert.IsTrue(_target.IsSoftErrorStateActive(state));
            Assert.IsFalse(_target.IsDisableStateActive(state));

            //then initiate lockup for same condition. verify states switch
            _systemDisableManager.Setup(x => x.Disable(guid, priority, It.IsAny<Func<string>>(), null)).Verifiable();
            _messageDisplay.Setup(x => x.RemoveMessage(guid)).Verifiable();
            Assert.IsTrue(_target.Disable(priority, state).Wait(WaitTime));
            _systemDisableManager.Verify();
            _messageDisplay.Verify();

            Assert.IsFalse(_target.IsSoftErrorStateActive(state));
            Assert.IsTrue(_target.IsDisableStateActive(state));
        }

        [DynamicData(nameof(EnableTestData))]
        [DataTestMethod]
        public void EnableTest(
            G2SDisableStates[] clearingStates,
            IEnumerable<Guid> disableGuids)
        {
            foreach (var guid in disableGuids)
            {
                _systemDisableManager.Setup(x => x.Enable(guid)).Verifiable();
            }

            _target = CreateG2SDisableProvider();
            _target.Disable(SystemDisablePriority.Immediate, clearingStates).Wait(WaitTime);
            Assert.IsTrue(_target.Enable(clearingStates).Wait(WaitTime));

            foreach(var clearingState in clearingStates)
            {
                Assert.IsFalse(_target.IsDisableStateActive(clearingState));
            }
            
            _systemDisableManager.Verify();
        }

        private G2SDisableProvider CreateG2SDisableProvider(
            bool nullSystemDisableManager = false,
            bool nullMessageDisplay = false)
        {
            return new G2SDisableProvider(
                nullSystemDisableManager ? null : _systemDisableManager.Object,
                nullMessageDisplay ? null : _messageDisplay.Object
            );
        }
    }
}
