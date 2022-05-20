namespace Aristocrat.Monaco.Asp.Tests.Client.DataSources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Asp.Client.Contracts;
    using Asp.Client.DataSources;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class CurrentMachineModeDataSourceTests
    {

        private CurrentMachineModeDataSource _source;
        private Mock<ICurrentMachineModeStateManager> _currentMachineModeStateManager;

        [TestInitialize]
        public virtual void TestInitialize()
        {
            _currentMachineModeStateManager = new Mock<ICurrentMachineModeStateManager>(MockBehavior.Strict);
            _source = new CurrentMachineModeDataSource(_currentMachineModeStateManager.Object);
        }

        [TestCleanup]
        public void CleanUp()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_source);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullCurrentMachineModeStateManagerTest()
        {
            var _ = new CurrentMachineModeDataSource(null);
        }

        [TestMethod]
        public void DataSourceNameTest()
        {
            var expectedName = "Current_Machine_Mode";
            Assert.AreEqual(expectedName, _source.Name);
        }

        [TestMethod]
        public void MembersTest()
        {
            var expectedMembers = new List<string>()
            {
                "Current_Machine_Mode"
            };

            var actualMembers = _source.Members;

            Assert.AreEqual(expectedMembers.Count, actualMembers.Count);
            Assert.IsTrue(actualMembers.SequenceEqual(expectedMembers));
        }

        [TestMethod]
        public void GetMemberValueTest()
        {
            var expected = MachineMode.AuditMode;
            _currentMachineModeStateManager.Setup(m => m.GetCurrentMode()).Returns(expected);

            Assert.AreEqual(expected, _source.GetMemberValue("Any_string"));
        }

        [TestMethod]
        public void MemberValueChangedTest()
        {
            var expected = MachineMode.DiagnosticTest;
            var memberValueChangedEventsReceived = 0;
            _source.MemberValueChanged += (sender, eventargs) =>
            {
                Assert.AreEqual(expected, eventargs["Current_Machine_Mode"]);
                ++memberValueChangedEventsReceived;
            };

            _currentMachineModeStateManager.Raise(m => m.MachineModeChanged += null, null, expected);

            Assert.AreEqual(1, memberValueChangedEventsReceived);
        }

        [TestMethod]
        public void Dispose_ShouldUnsubscribeAll()
        {
            _currentMachineModeStateManager.Setup(v => v.Dispose());

            _source.Dispose();
            _source.Dispose();

            _currentMachineModeStateManager.Verify(v => v.Dispose(), Times.Once);
        }
    }
}