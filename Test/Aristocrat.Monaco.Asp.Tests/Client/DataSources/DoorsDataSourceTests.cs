namespace Aristocrat.Monaco.Asp.Tests.Client.DataSources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts;
    using Asp.Client.Contracts;
    using Asp.Client.DataSources;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class DoorsDataSourceTests
    {
        private Mock<IDoorService> _doorService;
        private Mock<IEventBus> _eventBus;
        private Mock<IMeterManager> _meterManager;
        private DoorsDataSource _doorsDataSource;

        private Mock<IMeter> _mainDoorMeter;
        private Mock<IMeter> _mainTopBoxMeter;
        private Mock<IMeter> _mainBillStackerDoorMeter;
        private Mock<IMeter> _cashBoxMeter;
        private Mock<IMeter> _billStackerMeter;
        private Mock<IPersistentStorageManager> _persistentStorageManager;
        private Mock<IPersistentStorageAccessor> _persistentStorageAccessor;
        private Mock<IPersistentStorageTransaction> _persistentStorageTransaction;
        private Mock<IDataSourceRegistry> _dataSourceRegistry;
        private Mock<IDataSource> _dependencyDatasource;

        private OpenEvent _unexpectedOpenEvent = new OpenEvent(0, null);
        private OpenEvent _mainOpenEvent = new OpenEvent((int)AspDoorLogicalId.Main, AspDoorLogicalId.Main.ToString());
        private OpenEvent _mainOpticOpenEvent = new OpenEvent((int)AspDoorLogicalId.MainOptic, AspDoorLogicalId.MainOptic.ToString());
        private OpenEvent _mainTopBoxOpenEvent = new OpenEvent((int)AspDoorLogicalId.TopMain, AspDoorLogicalId.TopMain.ToString());
        private OpenEvent _mainTopBoxOpticOpenEvent = new OpenEvent((int)AspDoorLogicalId.TopMainOptic, AspDoorLogicalId.TopMainOptic.ToString());
        private OpenEvent _mainBellyOpenEvent = new OpenEvent((int)AspDoorLogicalId.Belly, AspDoorLogicalId.Belly.ToString());
        private OpenEvent _cashBoxOpenEvent = new OpenEvent((int)AspDoorLogicalId.CashBox, AspDoorLogicalId.CashBox.ToString());
        private OpenEvent _billStackerOpenEvent = new OpenEvent((int)AspDoorLogicalId.BillStacker, AspDoorLogicalId.BillStacker.ToString());

        private ClosedEvent _unexpectedClosedEvent = new ClosedEvent(0, null);
        private ClosedEvent _mainCloseEvent = new ClosedEvent((int)AspDoorLogicalId.Main, AspDoorLogicalId.Main.ToString());
        private ClosedEvent _mainOpticCloseEvent = new ClosedEvent((int)AspDoorLogicalId.MainOptic, AspDoorLogicalId.MainOptic.ToString());
        private ClosedEvent _mainTopBoxCloseEvent = new ClosedEvent((int)AspDoorLogicalId.TopMain, AspDoorLogicalId.TopMain.ToString());
        private ClosedEvent _mainTopBoxOpticCloseEvent = new ClosedEvent((int)AspDoorLogicalId.TopMainOptic, AspDoorLogicalId.TopMainOptic.ToString());
        private ClosedEvent _mainBellyCloseEvent = new ClosedEvent((int)AspDoorLogicalId.Belly, AspDoorLogicalId.Belly.ToString());
        private ClosedEvent _cashBoxCloseEvent = new ClosedEvent((int)AspDoorLogicalId.CashBox, AspDoorLogicalId.CashBox.ToString());
        private ClosedEvent _billStackerCloseEvent = new ClosedEvent((int)AspDoorLogicalId.BillStacker, AspDoorLogicalId.BillStacker.ToString());

        private DoorOpenMeteredEvent _unexpectedOpenMeteredEvent = new DoorOpenMeteredEvent(0, false, false, null);
        private DoorOpenMeteredEvent _mainDoorOpenMeteredEvent = new DoorOpenMeteredEvent((int)AspDoorLogicalId.Main, false, false, AspDoorLogicalId.Main.ToString());
        private DoorOpenMeteredEvent _mainTopBoxOpenMeteredEvent = new DoorOpenMeteredEvent((int)AspDoorLogicalId.TopMain, false, false, AspDoorLogicalId.TopMain.ToString());
        private DoorOpenMeteredEvent _mainBellyOpenMeteredEvent = new DoorOpenMeteredEvent((int)AspDoorLogicalId.Belly, false, false, AspDoorLogicalId.Belly.ToString());
        private DoorOpenMeteredEvent _cashBoxOpenMeteredEvent = new DoorOpenMeteredEvent((int)AspDoorLogicalId.CashBox, false, false, AspDoorLogicalId.CashBox.ToString());
        private DoorOpenMeteredEvent _billStackerOpenMeteredEvent = new DoorOpenMeteredEvent((int)AspDoorLogicalId.BillStacker, false, false, AspDoorLogicalId.BillStacker.ToString());

        [TestInitialize]
        public virtual void TestInitialize()
        {
            _mainDoorMeter = new Mock<IMeter>();
            _mainTopBoxMeter = new Mock<IMeter>();
            _mainBillStackerDoorMeter = new Mock<IMeter>();
            _cashBoxMeter = new Mock<IMeter>();
            _billStackerMeter = new Mock<IMeter>();
            _persistentStorageManager = new Mock<IPersistentStorageManager>();
            _persistentStorageAccessor = new Mock<IPersistentStorageAccessor>();
            _persistentStorageTransaction = new Mock<IPersistentStorageTransaction>();
            _dataSourceRegistry = new Mock<IDataSourceRegistry>();
            _dependencyDatasource = new Mock<IDataSource>();

            // Setup required as mocking the CabinetMeterProvider and Event (publish / subscribe) may involve modifying  the core application
            int mainDoorOpenCount = 0;
            int mainTopBoxOpenCount = 0;
            int mainBillStackerCount = 0;
            int cashBoxOpenTotalCount = 0;
            int billStackerOpenTotalCount = 0;

            _mainDoorMeter.Setup(m => m.Increment(1)).Callback<long>(x => ++mainDoorOpenCount);
            _mainDoorMeter.Setup(m => m.Lifetime).Returns(() => mainDoorOpenCount);
            _mainTopBoxMeter.Setup(m => m.Increment(1)).Callback<long>(x => ++mainTopBoxOpenCount);
            _mainTopBoxMeter.Setup(m => m.Lifetime).Returns(() => mainTopBoxOpenCount);
            _mainBillStackerDoorMeter.Setup(m => m.Increment(1)).Callback<long>(x => ++mainBillStackerCount);
            _mainBillStackerDoorMeter.Setup(m => m.Lifetime).Returns(() => mainBillStackerCount);
            _cashBoxMeter.Setup(m => m.Increment(1)).Callback<long>(x => ++cashBoxOpenTotalCount);
            _cashBoxMeter.Setup(m => m.Lifetime).Returns(() => cashBoxOpenTotalCount);
            _billStackerMeter.Setup(m => m.Increment(1)).Callback<long>(x => ++billStackerOpenTotalCount);
            _billStackerMeter.Setup(m => m.Lifetime).Returns(() => billStackerOpenTotalCount);

            // Contrived publish / increment setup
            Action<OpenEvent> mainOpenEventAction = e =>
            {
                if (e.LogicalId == (int)AspDoorLogicalId.Main || e.LogicalId == (int)AspDoorLogicalId.MainOptic)
                {
                    _mainDoorMeter.Object.Increment(1);
                }
                else if (e.LogicalId == (int)AspDoorLogicalId.TopMain || e.LogicalId == (int)AspDoorLogicalId.TopMainOptic)
                {
                    _mainTopBoxMeter.Object.Increment(1);
                }

                _doorService.Setup(m => m.GetDoorOpen(e.LogicalId)).Returns(true);

                _doorsDataSource.OnDoorStatusChanged(e);
            };

            Action<OpenEvent> bellyOpenEventAction = e =>
            {
                _mainBillStackerDoorMeter.Object.Increment(1);
                _doorsDataSource.OnDoorStatusChanged(e);
            };

            Action<OpenEvent> cashBoxOpenEventAction = e =>
            {
                _cashBoxMeter.Object.Increment(1);
                _doorsDataSource.OnDoorStatusChanged(e);
            };

            Action<OpenEvent> billStackerOpenEventAction = e =>
            {
                _billStackerMeter.Object.Increment(1);
                _doorsDataSource.OnDoorStatusChanged(e);
            };

            Action<ClosedEvent> doorCloseEventAction = e =>
            {
                _doorService.Setup(m => m.GetDoorOpen(e.LogicalId)).Returns(false);
                _doorsDataSource.OnDoorStatusChanged(e);
            };

            Action<DoorOpenMeteredEvent> doorOpenMeteredEventAction = e =>
            {
                _doorsDataSource.OnDoorOpenMeterChanged(e);
            };

            _doorService = new Mock<IDoorService>();
            _eventBus = new Mock<IEventBus>();


            int iMainDoorOpenCount = 0;
            int iCashBoxOpenCount = 0;
            int iCurrencyBillStackerOpenCount = 0;

            _persistentStorageTransaction.SetupSet(m => m["IllegalMainDoorOpenedCountField"] = It.IsAny<object>()).Callback<string, object>((fieldName, fieldValue) =>
            {
                iMainDoorOpenCount = (int)fieldValue;
                _persistentStorageTransaction.SetupGet(g => g[fieldName]).Returns(iMainDoorOpenCount);
                _persistentStorageAccessor.SetupGet(g => g[fieldName]).Returns(_persistentStorageTransaction.Object[fieldName]);
            });

            _persistentStorageAccessor.SetupGet(g => g["IllegalMainDoorOpenedCountField"]).Returns(iMainDoorOpenCount);

            _persistentStorageTransaction.SetupSet(m => m["IllegalCashBoxOpenedCountField"] = It.IsAny<object>()).Callback<string, object>((fieldName, fieldValue) =>
            {
                iCashBoxOpenCount = (int)fieldValue;
                _persistentStorageTransaction.SetupGet(g => g[fieldName]).Returns(iCashBoxOpenCount);
                _persistentStorageAccessor.SetupGet(g => g[fieldName]).Returns(_persistentStorageTransaction.Object[fieldName]);
            });

            _persistentStorageAccessor.SetupGet(g => g["IllegalCashBoxOpenedCountField"]).Returns(iCashBoxOpenCount);

            _persistentStorageTransaction.SetupSet(m => m["IllegalBillStackerOpenedCountField"] = It.IsAny<object>()).Callback<string, object>((fieldName, fieldValue) =>
            {
                iCurrencyBillStackerOpenCount = (int)fieldValue;
                _persistentStorageTransaction.SetupGet(g => g[fieldName]).Returns(iCurrencyBillStackerOpenCount);
                _persistentStorageAccessor.SetupGet(g => g[fieldName]).Returns(_persistentStorageTransaction.Object[fieldName]);
            });

            _persistentStorageAccessor.SetupGet(g => g["IllegalBillStackerOpenedCountField"]).Returns(iCurrencyBillStackerOpenCount);

            _persistentStorageManager.Setup(m => m.BlockExists(It.IsAny<string>())).Returns(false);
            _persistentStorageManager.Setup(m => m.CreateBlock(PersistenceLevel.Critical, It.IsAny<string>(), 1)).Returns(_persistentStorageAccessor.Object);

            _persistentStorageManager.Setup(m => m.GetBlock(It.IsAny<string>())).Returns(_persistentStorageAccessor.Object);

            _persistentStorageAccessor.Setup(m => m.StartTransaction()).Returns(_persistentStorageTransaction.Object);
            _persistentStorageTransaction.Setup(m => m.Commit());

            _eventBus.Setup(m => m.Publish(_mainOpenEvent)).Callback<OpenEvent>(e => mainOpenEventAction(e));
            _eventBus.Setup(m => m.Publish(_mainOpticOpenEvent)).Callback<OpenEvent>(e => mainOpenEventAction(e));
            _eventBus.Setup(m => m.Publish(_mainTopBoxOpenEvent)).Callback<OpenEvent>(e => mainOpenEventAction(e));
            _eventBus.Setup(m => m.Publish(_mainTopBoxOpticOpenEvent)).Callback<OpenEvent>(e => mainOpenEventAction(e));
            _eventBus.Setup(m => m.Publish(_mainBellyOpenEvent)).Callback<OpenEvent>(e => bellyOpenEventAction(e));
            _eventBus.Setup(m => m.Publish(_cashBoxOpenEvent)).Callback<OpenEvent>(e => cashBoxOpenEventAction(e));
            _eventBus.Setup(m => m.Publish(_billStackerOpenEvent)).Callback<OpenEvent>(e => billStackerOpenEventAction(e));

            _eventBus.Setup(m => m.Publish(_mainCloseEvent)).Callback<ClosedEvent>(e => doorCloseEventAction(e));
            _eventBus.Setup(m => m.Publish(_mainOpticCloseEvent)).Callback<ClosedEvent>(e => doorCloseEventAction(e));
            _eventBus.Setup(m => m.Publish(_mainTopBoxCloseEvent)).Callback<ClosedEvent>(e => doorCloseEventAction(e));
            _eventBus.Setup(m => m.Publish(_mainTopBoxOpticCloseEvent)).Callback<ClosedEvent>(e => doorCloseEventAction(e));
            _eventBus.Setup(m => m.Publish(_mainBellyCloseEvent)).Callback<ClosedEvent>(e => doorCloseEventAction(e));
            _eventBus.Setup(m => m.Publish(_cashBoxCloseEvent)).Callback<ClosedEvent>(e => doorCloseEventAction(e));
            _eventBus.Setup(m => m.Publish(_billStackerCloseEvent)).Callback<ClosedEvent>(e => doorCloseEventAction(e));

            _eventBus.Setup(m => m.Publish(_mainDoorOpenMeteredEvent)).Callback<DoorOpenMeteredEvent>(e => doorOpenMeteredEventAction(e));
            _eventBus.Setup(m => m.Publish(_mainTopBoxOpenMeteredEvent)).Callback<DoorOpenMeteredEvent>(e => doorOpenMeteredEventAction(e));
            _eventBus.Setup(m => m.Publish(_mainBellyOpenMeteredEvent)).Callback<DoorOpenMeteredEvent>(e => doorOpenMeteredEventAction(e));
            _eventBus.Setup(m => m.Publish(_cashBoxOpenMeteredEvent)).Callback<DoorOpenMeteredEvent>(e => doorOpenMeteredEventAction(e));
            _eventBus.Setup(m => m.Publish(_billStackerOpenMeteredEvent)).Callback<DoorOpenMeteredEvent>(e => doorOpenMeteredEventAction(e));

            _meterManager = new Mock<IMeterManager>();

            _meterManager.Setup(m => m.GetMeter(AspApplicationMeters.MainDoorOpenTotalCount)).Returns(_mainDoorMeter.Object);
            _meterManager.Setup(m => m.GetMeter(AspApplicationMeters.TopMainOpenTotalCount)).Returns(_mainTopBoxMeter.Object);
            _meterManager.Setup(m => m.GetMeter(AspApplicationMeters.BellyDoorOpenTotalCount)).Returns(_billStackerMeter.Object);

            _meterManager.Setup(m => m.GetMeter(AspApplicationMeters.CashBoxOpenTotalCount)).Returns(_cashBoxMeter.Object);
            _meterManager.Setup(m => m.GetMeter(AspApplicationMeters.BillStackerOpenTotalCount)).Returns(_billStackerMeter.Object);


            _dataSourceRegistry.Setup(m => m.GetDataSource("EGMProperty")).Returns(_dependencyDatasource.Object);

            _doorsDataSource = new DoorsDataSource(_doorService.Object, _meterManager.Object, _persistentStorageManager.Object);
        }

        [TestMethod]
        public void NullContructorTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new DoorsDataSource(null, _meterManager.Object, _persistentStorageManager.Object));
            Assert.ThrowsException<ArgumentNullException>(() => new DoorsDataSource(_doorService.Object, null, _persistentStorageManager.Object));
            Assert.ThrowsException<ArgumentNullException>(() => new DoorsDataSource(_doorService.Object, _meterManager.Object, null));
        }

        [TestMethod]
        public void DataSourceNameTest()
        {
            var expectedName = "Doors";
            Assert.AreEqual(expectedName, _doorsDataSource.Name);
        }

        [TestMethod]
        public void MembersTest()
        {
            var expectedMembers = new List<string>()
            {
                "Main_Door_Status",
                "CBox_Door_Status",
                "Bill_Stkr_Door_Status",
                "Main_DoorOpndCnt",
                "CBox_DoorOpndCnt",
                "Bill_Stacker_DoorCnt",
                "Illegal_DoorOpndCnt",
                "ICBox_DoorOpndCnt",
                "IBill_Stacker_DoorCnt"
            };

            var actualMembers = _doorsDataSource.Members;
            Assert.AreEqual(expectedMembers.Count, actualMembers.Count);
            Assert.IsTrue(actualMembers.SequenceEqual(expectedMembers));
        }

        [TestMethod]
        public void CardInsertedAuthorizationTest()
        {
            _doorsDataSource.DataSourceRegistry = _dataSourceRegistry.Object;
            // Aggregated illegal door pareameters count 1. invalid Ids 2. Main Door 3. Cash Box 4. Currency Bill Stacker

            int iMainDoorOpenCountInvalidId = 0;
            int iCashBoxOpenCountInvalidId = 0;
            int iBillStackerOpenCountInvalidId = 0;

            // 1. Invalid out of range Ids
            List<int> invalidIds = new List<int>(new[] { -1, 31, 100 });

            for (int i = 0; i < invalidIds.Count; i++)
            {
                int id = invalidIds[i];

                _dependencyDatasource.Setup(m => m.GetMemberValue("Card_Inserted")).Returns(id);

                _eventBus.Object.Publish(_mainTopBoxOpenEvent);
                _eventBus.Object.Publish(_mainOpenEvent);
                _eventBus.Object.Publish(_mainBellyOpenEvent);

                int expectedMain = (i + 1) * 2;
                iMainDoorOpenCountInvalidId = (int)_doorsDataSource.GetMemberValue("Illegal_DoorOpndCnt");
                iCashBoxOpenCountInvalidId = (int)_doorsDataSource.GetMemberValue("ICBox_DoorOpndCnt");
                iBillStackerOpenCountInvalidId = (int)_doorsDataSource.GetMemberValue("IBill_Stacker_DoorCnt");

                _ = _doorsDataSource.GetMemberValue("Illegal_DoorOpndCnt");

                Assert.AreEqual(expectedMain, iMainDoorOpenCountInvalidId);
                Assert.AreEqual(i, iCashBoxOpenCountInvalidId);
                Assert.AreEqual(i, iBillStackerOpenCountInvalidId);

                _eventBus.Object.Publish(_cashBoxOpenEvent);
                _eventBus.Object.Publish(_billStackerOpenEvent);

                iMainDoorOpenCountInvalidId = (int)_doorsDataSource.GetMemberValue("Illegal_DoorOpndCnt");
                iCashBoxOpenCountInvalidId = (int)_doorsDataSource.GetMemberValue("ICBox_DoorOpndCnt");
                iBillStackerOpenCountInvalidId = (int)_doorsDataSource.GetMemberValue("IBill_Stacker_DoorCnt");

                Assert.AreEqual(expectedMain, iMainDoorOpenCountInvalidId);
                Assert.AreEqual(i + 1, iCashBoxOpenCountInvalidId);
                Assert.AreEqual(i + 1, iBillStackerOpenCountInvalidId);

            }

            int iMainDoorOpenCountMainDoorId = 0;
            int iCashBoxOpenCountMainDoorId = 0;
            int iBillStackerOpenCountMainDoorId = 0;

            // 2. Ids that authorize the main door
            List<byte> mainDoorIds = new List<byte>(new byte[]
            { 12, 13, 14, 15, 16, 17, 18,
              20,
              22, 23, 24, 25, 26, 27, 28, 29, 30 });

            for (int i = 0; i < mainDoorIds.Count; i++)
            {
                byte id = mainDoorIds[i];

                _dependencyDatasource.Setup(m => m.GetMemberValue("Card_Inserted")).Returns(id);

                _eventBus.Object.Publish(_mainTopBoxOpenEvent);
                _eventBus.Object.Publish(_mainOpenEvent);
                _eventBus.Object.Publish(_mainBellyOpenEvent);

                iMainDoorOpenCountMainDoorId = (int)_doorsDataSource.GetMemberValue("Illegal_DoorOpndCnt");
                iCashBoxOpenCountMainDoorId = (int)_doorsDataSource.GetMemberValue("ICBox_DoorOpndCnt");
                iBillStackerOpenCountMainDoorId = (int)_doorsDataSource.GetMemberValue("IBill_Stacker_DoorCnt");

                Assert.AreEqual(iMainDoorOpenCountInvalidId, iMainDoorOpenCountMainDoorId);
                Assert.AreEqual(iCashBoxOpenCountInvalidId, iCashBoxOpenCountMainDoorId);
                Assert.AreEqual(iBillStackerOpenCountInvalidId, iBillStackerOpenCountMainDoorId);

                _eventBus.Object.Publish(_cashBoxOpenEvent);
                _eventBus.Object.Publish(_billStackerOpenEvent);

                iMainDoorOpenCountMainDoorId = (int)_doorsDataSource.GetMemberValue("Illegal_DoorOpndCnt");
                iCashBoxOpenCountMainDoorId = (int)_doorsDataSource.GetMemberValue("ICBox_DoorOpndCnt");
                iBillStackerOpenCountMainDoorId = (int)_doorsDataSource.GetMemberValue("IBill_Stacker_DoorCnt");

                Assert.AreEqual(iMainDoorOpenCountInvalidId, iMainDoorOpenCountMainDoorId);
                Assert.AreEqual(++iCashBoxOpenCountInvalidId, iCashBoxOpenCountMainDoorId);

                if (id == 20)
                    Assert.AreEqual(iBillStackerOpenCountInvalidId, iBillStackerOpenCountMainDoorId);
                else
                    Assert.AreEqual(iBillStackerOpenCountInvalidId += 1, iBillStackerOpenCountMainDoorId);

            }

            // 3. Ids that authorize the cashbox door

            _dependencyDatasource.Setup(m => m.GetMemberValue("Card_Inserted")).Returns(CardInserted.CashCard);

            _eventBus.Object.Publish(_mainTopBoxOpenEvent);
            _eventBus.Object.Publish(_mainOpenEvent);
            _eventBus.Object.Publish(_mainBellyOpenEvent);

            var iMainDoorOpenCountCashBoxId = (int)_doorsDataSource.GetMemberValue("Illegal_DoorOpndCnt");
            var iCashBoxOpenCountMCashBoxId = (int)_doorsDataSource.GetMemberValue("ICBox_DoorOpndCnt");
            var iBillStackerOpenCountMCashBoxId = (int)_doorsDataSource.GetMemberValue("IBill_Stacker_DoorCnt");

            Assert.AreEqual(iMainDoorOpenCountMainDoorId += 2, iMainDoorOpenCountCashBoxId);
            Assert.AreEqual(iCashBoxOpenCountMainDoorId, iCashBoxOpenCountMCashBoxId);
            Assert.AreEqual(iBillStackerOpenCountMainDoorId, iBillStackerOpenCountMCashBoxId);

            _eventBus.Object.Publish(_cashBoxOpenEvent);
            _eventBus.Object.Publish(_billStackerOpenEvent);

            iMainDoorOpenCountCashBoxId = (int)_doorsDataSource.GetMemberValue("Illegal_DoorOpndCnt");
            iCashBoxOpenCountMCashBoxId = (int)_doorsDataSource.GetMemberValue("ICBox_DoorOpndCnt");
            iBillStackerOpenCountMCashBoxId = (int)_doorsDataSource.GetMemberValue("IBill_Stacker_DoorCnt");

            Assert.AreEqual(iMainDoorOpenCountMainDoorId, iMainDoorOpenCountCashBoxId);
            Assert.AreEqual(iCashBoxOpenCountMainDoorId, iCashBoxOpenCountMCashBoxId);
            Assert.AreEqual(iBillStackerOpenCountMainDoorId += 1, iBillStackerOpenCountMCashBoxId);

            _eventBus.Object.Publish(_mainTopBoxOpenEvent);
            _eventBus.Object.Publish(_mainOpenEvent);
            _eventBus.Object.Publish(_mainBellyOpenEvent);

            // 4. Ids that authorize the currrency bill stacker

            _dependencyDatasource.Setup(m => m.GetMemberValue("Card_Inserted")).Returns(CardInserted.BillsCard);

            _eventBus.Object.Publish(_mainTopBoxOpenEvent);
            _eventBus.Object.Publish(_mainOpenEvent);
            _eventBus.Object.Publish(_mainBellyOpenEvent);

            var iMainDoorOpenCountCurrencyBillId = (int)_doorsDataSource.GetMemberValue("Illegal_DoorOpndCnt");
            var cashBoxOpenCountCurrencyBillid = (int)_doorsDataSource.GetMemberValue("ICBox_DoorOpndCnt");
            _ = (int)_doorsDataSource.GetMemberValue("IBill_Stacker_DoorCnt");

            Assert.AreEqual(iMainDoorOpenCountCashBoxId += 2, iMainDoorOpenCountCurrencyBillId);
            Assert.AreEqual(iCashBoxOpenCountMCashBoxId, iCashBoxOpenCountMCashBoxId);
            Assert.AreEqual(iBillStackerOpenCountMCashBoxId, cashBoxOpenCountCurrencyBillid);

            _eventBus.Object.Publish(_cashBoxOpenEvent);
            _eventBus.Object.Publish(_billStackerOpenEvent);

            _ = (int)_doorsDataSource.GetMemberValue("Illegal_DoorOpndCnt");
            _ = (int)_doorsDataSource.GetMemberValue("ICBox_DoorOpndCnt");
            _ = (int)_doorsDataSource.GetMemberValue("IBill_Stacker_DoorCnt");

            Assert.AreEqual(iMainDoorOpenCountCashBoxId, iMainDoorOpenCountCashBoxId);
            Assert.AreEqual(iCashBoxOpenCountMCashBoxId += 1, iCashBoxOpenCountMCashBoxId);
            Assert.AreEqual(iBillStackerOpenCountMCashBoxId, iBillStackerOpenCountMCashBoxId);
        }

        [TestMethod]
        public void GetMemberValueMainDoorStatusTest()
        {
            _doorService.Setup(m => m.GetDoorOpen((int)AspDoorLogicalId.Main)).Returns(true);
            _doorService.Setup(m => m.GetDoorOpen((int)AspDoorLogicalId.TopMain)).Returns(false);
            var status = _doorsDataSource.GetMemberValue("Main_Door_Status");
            Assert.AreEqual(0, status);

            _doorService.Setup(m => m.GetDoorOpen((int)AspDoorLogicalId.Main)).Returns(false);
            _doorService.Setup(m => m.GetDoorOpen((int)AspDoorLogicalId.TopMain)).Returns(true);
            status = _doorsDataSource.GetMemberValue("Main_Door_Status");
            Assert.AreEqual(0, status);

            _doorService.Setup(m => m.GetDoorOpen((int)AspDoorLogicalId.Main)).Returns(false);
            _doorService.Setup(m => m.GetDoorOpen((int)AspDoorLogicalId.TopMain)).Returns(false);
            status = _doorsDataSource.GetMemberValue("Main_Door_Status");
            Assert.AreEqual(1, status);

            _doorService.Setup(m => m.GetDoorOpen((int)AspDoorLogicalId.Main)).Returns(true);
            _doorService.Setup(m => m.GetDoorOpen((int)AspDoorLogicalId.TopMain)).Returns(true);
            status = _doorsDataSource.GetMemberValue("Main_Door_Status");
            Assert.AreEqual(0, status);

            _doorService.Setup(m => m.GetDoorOpen((int)AspDoorLogicalId.MainOptic)).Returns(true);
            _doorService.Setup(m => m.GetDoorOpen((int)AspDoorLogicalId.TopMainOptic)).Returns(false);
            status = _doorsDataSource.GetMemberValue("Main_Door_Status");
            Assert.AreEqual(0, status);

            _doorService.Setup(m => m.GetDoorOpen((int)AspDoorLogicalId.MainOptic)).Returns(false);
            _doorService.Setup(m => m.GetDoorOpen((int)AspDoorLogicalId.TopMainOptic)).Returns(true);
            status = _doorsDataSource.GetMemberValue("Main_Door_Status");
            Assert.AreEqual(0, status);

            _doorService.Setup(m => m.GetDoorOpen((int)AspDoorLogicalId.MainOptic)).Returns(true);
            _doorService.Setup(m => m.GetDoorOpen((int)AspDoorLogicalId.TopMainOptic)).Returns(true);
            status = _doorsDataSource.GetMemberValue("Main_Door_Status");
            Assert.AreEqual(0, status);

            _doorService.Setup(m => m.GetDoorOpen((int)AspDoorLogicalId.Main)).Returns(false);
            _doorService.Setup(m => m.GetDoorOpen((int)AspDoorLogicalId.TopMain)).Returns(false);
            _doorService.Setup(m => m.GetDoorOpen((int)AspDoorLogicalId.MainOptic)).Returns(false);
            _doorService.Setup(m => m.GetDoorOpen((int)AspDoorLogicalId.TopMainOptic)).Returns(false);
            status = _doorsDataSource.GetMemberValue("Main_Door_Status");
            Assert.AreEqual(1, status);
        }

        [TestMethod]
        public void GetMemberValueCashBoxStatusTest()
        {
            _doorService.Setup(m => m.GetDoorOpen((int)AspDoorLogicalId.CashBox)).Returns(true);
            var status = _doorsDataSource.GetMemberValue("CBox_Door_Status");
            Assert.AreEqual(0, status);

            _doorService.Setup(m => m.GetDoorOpen((int)AspDoorLogicalId.CashBox)).Returns(false);
            status = _doorsDataSource.GetMemberValue("CBox_Door_Status");
            Assert.AreEqual(1, status);
        }

        [TestMethod]
        public void GetMemberValueBillStackerBoxTest()
        {
            _doorService.Setup(m => m.GetDoorOpen((int)AspDoorLogicalId.BillStacker)).Returns(true);
            var status = _doorsDataSource.GetMemberValue("Bill_Stkr_Door_Status");
            Assert.AreEqual(0, status);

            _doorService.Setup(m => m.GetDoorOpen((int)AspDoorLogicalId.BillStacker)).Returns(false);
            status = _doorsDataSource.GetMemberValue("Bill_Stkr_Door_Status");
            Assert.AreEqual(1, status);
        }

        [TestMethod]
        public void MainDoorStatusEventAndCountTest()
        {
            List<string> expectedEventSequence = new List<string>(new[]{
                "Main_Door_Status",
                "Main_Door_Status",
            });

            Action<object, Dictionary<string, object>> memberValueChangedHandler = (sender, eventargs) =>
            {
                Assert.AreEqual(1, eventargs.Count);
                Assert.AreEqual(expectedEventSequence[0], eventargs.First().Key);
                expectedEventSequence.RemoveAt(0);
            };

            _doorsDataSource.MemberValueChanged += new EventHandler<Dictionary<string, object>>(memberValueChangedHandler);

            // Sequence of expected and unexepcted open/close status events
            _eventBus.Object.Publish(_mainOpenEvent);
            _eventBus.Object.Publish(_mainOpticOpenEvent);
            _eventBus.Object.Publish(_mainTopBoxOpenEvent);
            _eventBus.Object.Publish(_mainTopBoxOpticOpenEvent);
            _eventBus.Object.Publish(_mainTopBoxOpticCloseEvent);
            _eventBus.Object.Publish(_mainTopBoxCloseEvent);
            _eventBus.Object.Publish(_mainOpticCloseEvent);
            _eventBus.Object.Publish(_mainCloseEvent);
            // only 2 events Open/Closed expected
            Assert.AreEqual(0, expectedEventSequence.Count);

            _doorsDataSource.MemberValueChanged -= new EventHandler<Dictionary<string, object>>(memberValueChangedHandler);

            var mainDoorTotalCount = _doorsDataSource.GetMemberValue("Main_DoorOpndCnt");

            Assert.AreEqual((long)4, mainDoorTotalCount);
        }

        [TestMethod]
        public void OtherNotMainDoorStatusEventAndCountTest()
        {
            List<string> expectedEventSequence = new List<string>(new[]{
                //"Belly_Door_Status",
                "CBox_Door_Status",
                "Bill_Stkr_Door_Status",
                //"Belly_Door_Status",
                "Bill_Stkr_Door_Status",
                "CBox_Door_Status",
                //"Belly_Door_Status",
                "Bill_Stkr_Door_Status"
            });

            Action<object, Dictionary<string, object>> memberValueChangedHandler = (sender, eventargs) =>
            {
                Assert.AreEqual(1, eventargs.Count);
                Assert.AreEqual(expectedEventSequence[0], eventargs.First().Key);
                expectedEventSequence.RemoveAt(0);
            };

            _doorsDataSource.MemberValueChanged += new EventHandler<Dictionary<string, object>>(memberValueChangedHandler);

            // Sequence of expected and unexepcted open/close status events
            //_eventBus.Object.Publish(_mainBellyOpenEvent);
            _eventBus.Object.Publish(_cashBoxOpenEvent);
            _eventBus.Object.Publish(_billStackerOpenEvent);
            //_eventBus.Object.Publish(_mainBellyOpenEvent);
            _eventBus.Object.Publish(_unexpectedOpenEvent);
            _eventBus.Object.Publish(_unexpectedOpenEvent);
            _eventBus.Object.Publish(_billStackerOpenEvent);
            _eventBus.Object.Publish(_unexpectedClosedEvent);
            _eventBus.Object.Publish(_cashBoxOpenEvent);
            _eventBus.Object.Publish(_unexpectedClosedEvent);
            //_eventBus.Object.Publish(_mainBellyCloseEvent);
            _eventBus.Object.Publish(_billStackerOpenEvent);

            // sequence completed successfully
            Assert.AreEqual(0, expectedEventSequence.Count);

            _doorsDataSource.MemberValueChanged -= new EventHandler<Dictionary<string, object>>(memberValueChangedHandler);

            var mainDoorTotalCount = _doorsDataSource.GetMemberValue("Main_DoorOpndCnt");

            Assert.AreEqual((long)0, mainDoorTotalCount);

            var cashBoxTtoalCount = _doorsDataSource.GetMemberValue("CBox_DoorOpndCnt");
            Assert.AreEqual((long)2, cashBoxTtoalCount);

            var billStackTotalCount = _doorsDataSource.GetMemberValue("Bill_Stacker_DoorCnt");
            Assert.AreEqual((long)3, billStackTotalCount);
        }

        [TestMethod]
        public void GetMemberValueDoorOpenMeteredCountTest()
        {
            List<string> expectedEventSequence = new List<string>(new[]{
                "Main_DoorOpndCnt",
                "Main_DoorOpndCnt",
                //"Belly_DoorOpndCnt",
                "CBox_DoorOpndCnt",
                "Bill_Stacker_DoorCnt",
                //"Belly_DoorOpndCnt",
                "Bill_Stacker_DoorCnt",
                "CBox_DoorOpndCnt",
                //"Belly_DoorOpndCnt",
                "Bill_Stacker_DoorCnt"
            });

            Action<object, Dictionary<string, object>> memberValueChangedHandler = (sender, eventargs) =>
            {
                Assert.AreEqual(1, eventargs.Count);
                Assert.AreEqual(expectedEventSequence[0], eventargs.First().Key);
                expectedEventSequence.RemoveAt(0);
            };

            _doorsDataSource.MemberValueChanged += new EventHandler<Dictionary<string, object>>(memberValueChangedHandler);

            // Sequence of expected and unexepcted open/close metered count events
            _eventBus.Object.Publish(_mainTopBoxOpenMeteredEvent);
            _eventBus.Object.Publish(_mainDoorOpenMeteredEvent);
            //_eventBus.Object.Publish(_mainBellyOpenMeteredEvent);
            _eventBus.Object.Publish(_cashBoxOpenMeteredEvent);
            _eventBus.Object.Publish(_billStackerOpenMeteredEvent);
            //_eventBus.Object.Publish(_mainBellyOpenMeteredEvent);
            _eventBus.Object.Publish(_unexpectedOpenMeteredEvent);
            _eventBus.Object.Publish(_unexpectedOpenMeteredEvent);
            _eventBus.Object.Publish(_billStackerOpenMeteredEvent);
            _eventBus.Object.Publish(_unexpectedOpenMeteredEvent);
            _eventBus.Object.Publish(_cashBoxOpenMeteredEvent);
            //_eventBus.Object.Publish(_mainBellyOpenMeteredEvent);
            _eventBus.Object.Publish(_unexpectedOpenMeteredEvent);
            _eventBus.Object.Publish(_billStackerOpenMeteredEvent);

            // sequence completed successfully
            Assert.AreEqual(0, expectedEventSequence.Count);
        }
    }
}