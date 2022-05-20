namespace Aristocrat.Monaco.Asp.Tests.Client.DataSources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.Monaco.Test.Common;
    using Asp.Client.DataSources;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using static Aristocrat.Monaco.Asp.Client.DataSources.LogicSealDataSource;

    [TestClass]
    public class LogicSealDataSourceTests
    {
        private Mock<IDoorService> _doorService;
        private Mock<IPersistentStorageManager> _persistentStorageManager;
        private Mock<IPersistentStorageAccessor> _persistentStorageAccessor;
        private Mock<IPersistentStorageTransaction> _persistentStorageTransaction;
        private Mock<ISystemDisableManager> _systemDisableManager;

        private LogicSealDataSource _logicSealDataSource;

        private OpenEvent _logicOpenEvent = new OpenEvent((int)AspDoorLogicalId.Logic, AspDoorLogicalId.Logic.ToString());
        private ClosedEvent _logicClosedEvent = new ClosedEvent((int)AspDoorLogicalId.Logic, AspDoorLogicalId.Logic.ToString());

        private object _logicSealStatusFieldTestValue = string.Empty;
        private object _logicSealBrokenCountFieldTestValue = string.Empty;
        private object _logicSealTestValue = string.Empty;

        private string InvalidSealValue = "FF_INVALID_CODE_INPUT_FF";

        [TestInitialize]
        public virtual void TestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MockLocalization.Setup(MockBehavior.Default);
            _systemDisableManager = new Mock<ISystemDisableManager>();
            _doorService = new Mock<IDoorService>(MockBehavior.Strict);
            _persistentStorageManager = new Mock<IPersistentStorageManager>(MockBehavior.Strict);
            _persistentStorageAccessor = new Mock<IPersistentStorageAccessor>();
            _persistentStorageTransaction = new Mock<IPersistentStorageTransaction>();

            _persistentStorageTransaction.SetupSet(m => m["LogicSealStatusField"] = It.IsAny<object>()).Callback<string, object>((fieldName, fieldValue) =>
            {
                _logicSealStatusFieldTestValue = fieldValue;
                _persistentStorageTransaction.SetupGet(g => g[fieldName]).Returns(_logicSealStatusFieldTestValue);
                _persistentStorageAccessor.SetupGet(g => g[fieldName]).Returns(_persistentStorageTransaction.Object[fieldName]);
            });

            _persistentStorageTransaction.SetupSet(m => m["LogicSealBrokenCountField"] = It.IsAny<object>()).Callback<string, object>((fieldName, fieldValue) =>
            {
                _logicSealBrokenCountFieldTestValue = fieldValue;
                _persistentStorageTransaction.SetupGet(g => g[fieldName]).Returns(_logicSealBrokenCountFieldTestValue);
                _persistentStorageAccessor.SetupGet(g => g[fieldName]).Returns(_persistentStorageTransaction.Object[fieldName]);
            });

            _persistentStorageTransaction.SetupSet(m => m["VerificationCodeField"] = It.IsAny<object>()).Callback<string, object>((fieldName, fieldValue) =>
            {
                _logicSealTestValue = fieldValue;
                _persistentStorageTransaction.SetupGet(g => g[fieldName]).Returns(_logicSealTestValue);
                _persistentStorageAccessor.SetupGet(g => g[fieldName]).Returns(_persistentStorageTransaction.Object[fieldName]);
            });

            _persistentStorageAccessor.Setup(m => m.StartTransaction()).Returns(_persistentStorageTransaction.Object);
            _persistentStorageTransaction.Setup(m => m.Commit());
            _persistentStorageManager.Setup(m => m.BlockExists(It.IsAny<string>())).Returns(false);
            _persistentStorageManager.Setup(m => m.CreateBlock(PersistenceLevel.Critical, It.IsAny<string>(), 1)).Returns(_persistentStorageAccessor.Object);
            _persistentStorageManager.Setup(m => m.GetBlock(It.IsAny<string>())).Returns(_persistentStorageAccessor.Object);

            _doorService.Setup(m => m.GetDoorClosed((int)AspDoorLogicalId.Logic)).Returns(true);
            _logicSealDataSource = new LogicSealDataSource(_doorService.Object, _persistentStorageManager.Object, _systemDisableManager.Object);
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new LogicSealDataSource(null, _persistentStorageManager.Object, _systemDisableManager.Object));
            Assert.ThrowsException<ArgumentNullException>(() => new LogicSealDataSource(_doorService.Object, null, _systemDisableManager.Object));
            Assert.ThrowsException<ArgumentNullException>(() => new LogicSealDataSource(_doorService.Object, _persistentStorageManager.Object, null));
            
            _persistentStorageManager.Setup(m => m.BlockExists(It.IsAny<string>())).Returns(false);
            _logicSealDataSource = new LogicSealDataSource(_doorService.Object, _persistentStorageManager.Object, _systemDisableManager.Object);
            Assert.IsNotNull(_logicSealDataSource);

            _persistentStorageManager.Setup(m => m.BlockExists(It.IsAny<string>())).Returns(true);
            _logicSealDataSource = new LogicSealDataSource(_doorService.Object, _persistentStorageManager.Object, _systemDisableManager.Object);
            Assert.IsNotNull(_logicSealDataSource);
        }

        [TestMethod]
        public void DataSourceNameTest()
        {
            var expectedName = "Logic_Door";
            Assert.AreEqual(expectedName, _logicSealDataSource.Name);
        }

        [TestMethod]
        public void MembersTest()
        {
            var expectedMembers = new List<string>()
            {
                "Total_LgcSlBroken_Cnt",
                "Logic_Seal_Status",
                "Verification_Code",
                "Logic_Door_Status"
            };

            var actualMembers = _logicSealDataSource.Members;
            Assert.AreEqual(expectedMembers.Count, actualMembers.Count);
            Assert.IsTrue(actualMembers.SequenceEqual(expectedMembers));
        }

        [TestMethod]
        public void GetMembersMapTest()
        {
            // Door
            // Open 
            _doorService.Setup(m => m.GetDoorClosed((int)DoorLogicalId.Logic)).Returns(false);
            var status = _logicSealDataSource.GetMemberValue("Logic_Door_Status");
            Assert.AreEqual(1, status);

            // Close
            _doorService.Setup(m => m.GetDoorClosed((int)DoorLogicalId.Logic)).Returns(true);
            status = _logicSealDataSource.GetMemberValue("Logic_Door_Status");
            Assert.AreEqual(0, status);

            // Seal 
            // Sealed 
            _persistentStorageAccessor.Setup(m => m["LogicSealStatusField"]).Returns(LogicSealStatusEnum.Sealed);
            status = _logicSealDataSource.GetMemberValue("Logic_Seal_Status");
            Assert.AreEqual(0, status);

            // Broken 
            _persistentStorageAccessor.Setup(m => m["LogicSealStatusField"]).Returns(LogicSealStatusEnum.Broken);
            status = _logicSealDataSource.GetMemberValue("Logic_Seal_Status");
            Assert.AreEqual(1, status);

            // Count
            _persistentStorageAccessor.Setup(m => m["LogicSealBrokenCountField"]).Returns(5);
            var count = _logicSealDataSource.GetMemberValue("Total_LgcSlBroken_Cnt");
            Assert.AreEqual(5, count);

            // Verification Code
            // Null 
            _persistentStorageAccessor.Setup(m => m["VerificationCodeField"]).Returns(null);
            var code = _logicSealDataSource.GetMemberValue("Verification_Code");
            Assert.AreEqual(null, code);

            // Valid 
            _persistentStorageAccessor.Setup(m => m["VerificationCodeField"]).Returns("FFFFFFFFFFFFFFF");
            code = _logicSealDataSource.GetMemberValue("Verification_Code");
            Assert.AreEqual("FFFFFFFFFFFFFFF", code);
        }

        [TestMethod]
        public void SetMembersTest()
        {
            // Valid
            _doorService.Setup(m => m.GetDoorClosed((int)AspDoorLogicalId.Logic)).Returns(true);
            _persistentStorageAccessor.Setup(m => m["LogicSealStatusField"]).Returns(LogicSealStatusEnum.Broken);
            _logicSealDataSource.SetMemberValue("Verification_Code", "FFFFFFFFFFFFFFF");
            var code = _logicSealDataSource.GetMemberValue("Verification_Code");
            Assert.AreEqual("FFFFFFFFFFFFFFF", code);

            // Invalid
            _doorService.Setup(m => m.GetDoorClosed((int)AspDoorLogicalId.Logic)).Returns(true);
            _persistentStorageAccessor.Setup(m => m["LogicSealStatusField"]).Returns(LogicSealStatusEnum.Broken);
            Assert.ThrowsException<Exception>(() => _logicSealDataSource.SetMemberValue("Verification_Code", InvalidSealValue));
            code = (string)_logicSealDataSource.GetMemberValue("Verification_Code");

            // does not alter the previous stored result
            Assert.AreEqual("FFFFFFFFFFFFFFF", code);
        }

        [TestMethod]
        public void DoorStatusSealEventAndCountTest()
        {
            List<string> expectedEventSequence = new List<string>(new string[]{
                // x8 repeated sequence but different assert expectations depending on door
                // open event type
                // open
                "Total_LgcSlBroken_Cnt",
                "Logic_Seal_Status",
                "Verification_Code",
                "Logic_Door_Status",
                // closed
                "Logic_Door_Status", 
                // open
                "Logic_Door_Status",
                // closed
                "Logic_Door_Status",
                // sealed
                "Logic_Seal_Status",
                "Verification_Code",
                // open
                "Total_LgcSlBroken_Cnt",
                "Logic_Seal_Status",
                "Verification_Code",
                "Logic_Door_Status",
            });

            var expectedDisableReason = "Logic Seal Is Broken";
            _systemDisableManager.Setup(e => e.Disable(It.IsAny<Guid>(), SystemDisablePriority.Immediate, It.IsAny<Func<string>>(), null))
                .Callback<Guid, SystemDisablePriority, Func<string>, Type>(
                    (guid, priority, func, type) => Assert.AreEqual(expectedDisableReason, func.Invoke()));

            Action<object, Dictionary<string, object>> memberValueChangedHandler = new Action<object, Dictionary<string, object>>((sender, eventargs) =>
            {
                Assert.AreEqual(1, eventargs.Count);
                Assert.AreEqual(expectedEventSequence[0], eventargs.First().Key);
                expectedEventSequence.RemoveAt(0);
            });

            _logicSealDataSource.MemberValueChanged += new EventHandler<Dictionary<string, object>>(memberValueChangedHandler);

            var totalLgcSlBrokenCnt = _logicSealDataSource.GetMemberValue("Total_LgcSlBroken_Cnt");
            Assert.AreEqual((int)0, totalLgcSlBrokenCnt);

            // open x3 (contrived)
            _doorService.Setup(m => m.GetDoorClosed((int)AspDoorLogicalId.Logic)).Returns(false);
            _logicSealDataSource.HandleEvent(_logicOpenEvent);

            totalLgcSlBrokenCnt = _logicSealDataSource.GetMemberValue("Total_LgcSlBroken_Cnt");
            var doorStatus = _logicSealDataSource.GetMemberValue("Logic_Door_Status");
            Assert.AreEqual((int)1, totalLgcSlBrokenCnt);
            Assert.AreEqual(1, doorStatus);

            // closed
            _doorService.Setup(m => m.GetDoorClosed((int)AspDoorLogicalId.Logic)).Returns(true);
            _logicSealDataSource.HandleEvent(_logicClosedEvent);

            doorStatus = _logicSealDataSource.GetMemberValue("Logic_Door_Status");
            Assert.AreEqual(0, doorStatus);

            // open
            _doorService.Setup(m => m.GetDoorClosed((int)AspDoorLogicalId.Logic)).Returns(false);
            _logicSealDataSource.HandleEvent(_logicOpenEvent);

            totalLgcSlBrokenCnt = _logicSealDataSource.GetMemberValue("Total_LgcSlBroken_Cnt");
            doorStatus = _logicSealDataSource.GetMemberValue("Logic_Door_Status");
            // count remains at 1 (not sealed before open)
            Assert.AreEqual((int)1, totalLgcSlBrokenCnt);
            Assert.AreEqual(1, doorStatus);

            // closed
            _doorService.Setup(m => m.GetDoorClosed((int)AspDoorLogicalId.Logic)).Returns(true);
            _logicSealDataSource.HandleEvent(_logicClosedEvent);

            totalLgcSlBrokenCnt = _logicSealDataSource.GetMemberValue("Total_LgcSlBroken_Cnt");
            doorStatus = _logicSealDataSource.GetMemberValue("Logic_Door_Status");
            Assert.AreEqual((int)1, totalLgcSlBrokenCnt);
            Assert.AreEqual(0, doorStatus);

            // invalid seal
            Assert.ThrowsException<Exception>(() => _logicSealDataSource.SetMemberValue("Verification_Code", InvalidSealValue));

            // sealed
            _logicSealDataSource.SetMemberValue("Verification_Code", "FFFFFFFFFFFFFFF");

            // open
            _doorService.Setup(m => m.GetDoorClosed((int)AspDoorLogicalId.Logic)).Returns(false);
            _logicSealDataSource.HandleEvent(_logicOpenEvent);

            totalLgcSlBrokenCnt = _logicSealDataSource.GetMemberValue("Total_LgcSlBroken_Cnt");
            doorStatus = _logicSealDataSource.GetMemberValue("Logic_Door_Status");
            Assert.AreEqual((int)2, totalLgcSlBrokenCnt);
            Assert.AreEqual(1, doorStatus);

            // expected event sequence complete
            Assert.AreEqual(expectedEventSequence.Count, 0);
        }
    }
}