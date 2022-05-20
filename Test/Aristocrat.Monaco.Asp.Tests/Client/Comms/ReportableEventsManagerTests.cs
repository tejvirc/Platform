namespace Aristocrat.Monaco.Asp.Tests.Client.Comms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.Monaco.Asp.Client.Contracts;
    using Aristocrat.Monaco.Hardware.Contracts.Persistence;
    using Asp.Client.Comms;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;

    [TestClass]
    public class ReportableEventsManagerTests
    {
        private Mock<IPersistentStorageManager> _persistentStorageManager;
        private Mock<IPersistentStorageAccessor> _persistentStorageAccessor;
        private Mock<IPersistentStorageTransaction> _persistentStorageTransaction;
        private IReportableEventsManager _subject;

        private static List<string> _setEvents = new List<string>
            {
                "1_1_1",
                "2_2_2",
                "3_3_3",
                "4_4_4",
                "5_5_5",
            };

        private Dictionary<int, Dictionary<string, object>> _getAllResults = new Dictionary<int, Dictionary<string, object>>
            {
                {
                    0,
                    new Dictionary<string, object>
                    {
                        { "AspActiveEvents", JsonConvert.SerializeObject(_setEvents)
    }
}
                }
            };

        [TestInitialize]
        public void Initialise()
        {
            _persistentStorageManager = new Mock<IPersistentStorageManager>();
            _persistentStorageAccessor = new Mock<IPersistentStorageAccessor>();
            _persistentStorageTransaction = new Mock<IPersistentStorageTransaction>();

            _persistentStorageManager.Setup(s => s.BlockExists(It.IsAny<string>())).Returns(() => true).Verifiable();
            _persistentStorageAccessor.Setup(s => s.GetAll()).Returns(() => new Dictionary<int, Dictionary<string, object>>());

            _persistentStorageManager.Setup(s => s.GetBlock(It.IsAny<string>())).Returns(() => _persistentStorageAccessor.Object).Verifiable();
            _persistentStorageManager.Setup(s => s.CreateBlock(PersistenceLevel.Static, It.IsAny<string>(), 0)).Returns(() => _persistentStorageAccessor.Object).Verifiable();
            _persistentStorageAccessor.Setup(s => s.StartTransaction()).Returns(() => _persistentStorageTransaction.Object).Verifiable();
            _persistentStorageTransaction.Setup(s => s.Commit()).Verifiable();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructor_ShouldThrow()
        {
            new ReportableEventsManager(null);
        }

        [TestMethod]
        public void Constructor_NoExistingBlock_ShouldCreateBlockAndLoadFromDatabase()
        {
            _persistentStorageManager.Setup(s => s.BlockExists(It.IsAny<string>())).Returns(() => false).Verifiable();

            _subject = new ReportableEventsManager(_persistentStorageManager.Object);

            _persistentStorageManager.Verify(v => v.BlockExists(It.IsAny<string>()), Times.Once);
            _persistentStorageManager.Verify(v => v.CreateBlock(PersistenceLevel.Static, It.IsAny<string>(), 0), Times.Once);
            _persistentStorageAccessor.Verify(v => v.GetAll(), Times.Once);
        }

        [TestMethod]
        public void Constructor_ExistingBlock_ShouldGetBlockAndLoadFromDatabase()
        {
            _subject = new ReportableEventsManager(_persistentStorageManager.Object);

            _persistentStorageManager.Verify(v => v.BlockExists(It.IsAny<string>()), Times.Once);
            _persistentStorageManager.Verify(v => v.CreateBlock(PersistenceLevel.Static, It.IsAny<string>(), 0), Times.Never);
            _persistentStorageManager.Verify(v => v.GetBlock(It.IsAny<string>()), Times.Once);
            _persistentStorageAccessor.Verify(v => v.GetAll(), Times.Once);
        }

        [TestMethod]
        public void GetAll_ShouldReturnResults()
        {
            _persistentStorageAccessor.Setup(s => s.GetAll()).Returns(() => _getAllResults);

            _subject = new ReportableEventsManager(_persistentStorageManager.Object);

            var result = _subject.GetAll();

            Assert.IsNotNull(result);

            var expected = _setEvents.Select(s =>
            {
                var parts = s.Split('_');
                return (byte.Parse(parts[0]), byte.Parse(parts[1]), byte.Parse(parts[2]));
            }).ToList();

            CollectionAssert.AreEquivalent(result, expected);

            _persistentStorageAccessor.Verify(v => v.GetAll(), Times.Once);
        }

        [TestMethod]
        public void SetBatch_ShouldUpdateStorageOnlyOncePerEntry()
        {
            _subject = new ReportableEventsManager(_persistentStorageManager.Object);

            var setEvents = _setEvents.Select(s =>
            {
                var parts = s.Split('_');
                return (byte.Parse(parts[0]), byte.Parse(parts[1]), byte.Parse(parts[2]));
            }).ToList();

            //First time, should update storage
            _subject.SetBatch(setEvents);
            _persistentStorageAccessor.Verify(v => v.StartTransaction(), Times.Once);
            _persistentStorageTransaction.Verify(v => v.Commit(), Times.Once);

            //Second time, should not update storage
            _subject.SetBatch(setEvents);
            _persistentStorageAccessor.Verify(v => v.StartTransaction(), Times.Once);
            _persistentStorageTransaction.Verify(v => v.Commit(), Times.Once);
        }

        [TestMethod]
        public void Set_ShouldUpdateStorageOnlyOncePerEntry()
        {
            _subject = new ReportableEventsManager(_persistentStorageManager.Object);

            //First time, should update storage
            _subject.Set(1, 1, 1);
            _persistentStorageAccessor.Verify(v => v.StartTransaction(), Times.Once);
            _persistentStorageTransaction.Verify(v => v.Commit(), Times.Once);

            //Second time, should not update storage
            _subject.Set(1, 1, 1);
            _persistentStorageAccessor.Verify(v => v.StartTransaction(), Times.Once);
            _persistentStorageTransaction.Verify(v => v.Commit(), Times.Once);
        }

        [TestMethod]
        public void ClearBatch_ShouldUpdateStorageOnlyOncePerEntry()
        {
            _persistentStorageAccessor.SetupSequence(s => s.GetAll()).Returns(_getAllResults).Returns(new Dictionary<int, Dictionary<string, object>>());

            _subject = new ReportableEventsManager(_persistentStorageManager.Object);

            var setEvents = _setEvents.Select(s =>
            {
                var parts = s.Split('_');
                return (byte.Parse(parts[0]), byte.Parse(parts[1]), byte.Parse(parts[2]));
            }).ToList();

            //First time, should update storage
            _subject.ClearBatch(setEvents);
            _persistentStorageAccessor.Verify(v => v.StartTransaction(), Times.Once);
            _persistentStorageTransaction.Verify(v => v.Commit(), Times.Once);

            //Second time, should not update storage
            _subject.ClearBatch(setEvents);
            _persistentStorageAccessor.Verify(v => v.StartTransaction(), Times.Once);
            _persistentStorageTransaction.Verify(v => v.Commit(), Times.Once);
        }

        [TestMethod]
        public void Clear_ShouldUpdateStorageOnlyOncePerEntry()
        {
            var secondGetAllResults = new Dictionary<int, Dictionary<string, object>>
            {
                {
                    0,
                    new Dictionary<string, object>
                    {
                        { "AspActiveEvents", JsonConvert.SerializeObject(_setEvents.Where(w => w == "1_1_1")) }
                    }
                }
            };

            _persistentStorageAccessor.SetupSequence(s => s.GetAll()).Returns(_getAllResults).Returns(secondGetAllResults);

            _subject = new ReportableEventsManager(_persistentStorageManager.Object);

            //First time, should update storage
            _subject.Clear(1, 1, 1);
            _persistentStorageAccessor.Verify(v => v.StartTransaction(), Times.Once);
            _persistentStorageTransaction.Verify(v => v.Commit(), Times.Once);

            //Second time, should not update storage
            _subject.Clear(1, 1, 1);
            _persistentStorageAccessor.Verify(v => v.StartTransaction(), Times.Once);
            _persistentStorageTransaction.Verify(v => v.Commit(), Times.Once);
        }
    }
}