namespace Aristocrat.Monaco.Asp.Tests.Client.DataSources
{
    using Aristocrat.Monaco.Asp.Client.DataSources;
    using Aristocrat.Monaco.Asp.Progressive;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [TestClass]
    public class JackpotNumberAndControllerIdDataSourceTests
    {
        private Dictionary<string, long> _memberValues = new Dictionary<string, long>
        {
            { "Current_Jackpot_Number_0", 7 },
            { "Jackpot_Controller_Id_ByteOne_0", 8 },
            { "Jackpot_Controller_Id_ByteTwo_0", 9 },
            { "Jackpot_Controller_Id_ByteThree_0", 10 },
        };

        private JackpotNumberAndControllerIdDataSource _subject;
        private dynamic _accessor;
        private Mock<IEventBus> _eventBus;
        private Action<ProgressiveManageUpdatedEvent> _progressiveManageUpdatedEventCallback;

        [TestInitialize]
        public virtual void TestInitialize()
        {
            _eventBus = new Mock<IEventBus>();

            _eventBus.Setup(
                    m => m.Subscribe(It.IsAny<JackpotNumberAndControllerIdDataSource>(), It.IsAny<Action<ProgressiveManageUpdatedEvent>>()))
                .Callback<object, Action<ProgressiveManageUpdatedEvent>>(
                    (subscriber, callback) => _progressiveManageUpdatedEventCallback = callback);

            _subject = new JackpotNumberAndControllerIdDataSource(_eventBus.Object);

            _accessor = new DynamicPrivateObject(_subject);
        }

        [TestMethod]
        public void NullConstructorTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new ProgressiveDataSource(null));
        }

        [TestMethod]
        public void DataSourceNameTest()
        {
            var expectedName = "JackpotNumberAndControllerIdDataSource";
            Assert.AreEqual(expectedName, _subject.Name);
        }

        [TestMethod]
        public void MembersTest()
        {
            var expectedMembers = new List<string>();

            for (int progressiveLevel = 0; progressiveLevel < ProgressiveConstants.LinkProgressiveMaxLevels; progressiveLevel++)
            {
                expectedMembers.Add($"Current_Jackpot_Number_{progressiveLevel}");
                expectedMembers.Add($"Jackpot_Controller_Id_ByteOne_{progressiveLevel}");
                expectedMembers.Add($"Jackpot_Controller_Id_ByteTwo_{progressiveLevel}");
                expectedMembers.Add($"Jackpot_Controller_Id_ByteThree_{progressiveLevel}");
            }

            var actualMembers = _subject.Members;
            Assert.AreEqual(expectedMembers.Count, actualMembers.Count);
            Assert.IsTrue(actualMembers.SequenceEqual(expectedMembers));
        }

        [TestMethod]
        public void GetMemberValue_ReturnsCorrectValue()
        {
            _subject.Begin(_memberValues.Keys.ToList());

            _subject.SetMemberValue("Current_Jackpot_Number_0", _memberValues["Current_Jackpot_Number_0"]);
            _subject.SetMemberValue("Jackpot_Controller_Id_ByteOne_0", _memberValues["Jackpot_Controller_Id_ByteOne_0"]);
            _subject.SetMemberValue("Jackpot_Controller_Id_ByteTwo_0", _memberValues["Jackpot_Controller_Id_ByteTwo_0"]);
            _subject.SetMemberValue("Jackpot_Controller_Id_ByteThree_0", _memberValues["Jackpot_Controller_Id_ByteThree_0"]);

            _subject.Begin(_memberValues.Keys.ToList());

            _memberValues.Keys.ToList().ForEach(f => Assert.IsTrue(long.Parse(_subject.GetMemberValue(f).ToString()) == _memberValues[f]));
        }

        [TestMethod]
        public void GetMemberValue_InvalidInput()
        {
            _subject.Begin(_memberValues.Keys.ToList());

            var invalidMemberNameActual = _subject.GetMemberValue("Invalid Member Name_0");
            Assert.AreEqual(0, invalidMemberNameActual);

            var invalidLevelId = _subject.GetMemberValue("Current_Jackpot_Number_100");
            Assert.AreEqual(0, invalidLevelId);

            var invalidInput = _subject.GetMemberValue("Invalid Input");
            Assert.AreEqual(0, invalidInput);
        }

        [TestMethod]
        public void SetMemberValue_Current_Jackpot_Number_And_ControllerId_ShouldOnlyUpdateProgressiveManager()
        {
            _subject.Begin(_memberValues.Keys.ToList());

            _subject.SetMemberValue("Current_Jackpot_Number_0", 123);
            _subject.SetMemberValue("Jackpot_Controller_Id_ByteOne_0", 1);
            _subject.SetMemberValue("Jackpot_Controller_Id_ByteTwo_0", 2);
            _subject.SetMemberValue("Jackpot_Controller_Id_ByteThree_0", 3);

            _subject.Commit();

            _eventBus.Verify(v => v.Publish(It.IsAny<JackpotNumberAndControllerIdUpdateEvent>()), Times.Once);

            _subject.RollBack();

            _subject.Commit();

            //Calling rollback should prevent commit from adding any more calls - so expect same result here as previous check
            _eventBus.Verify(v => v.Publish(It.IsAny<JackpotNumberAndControllerIdUpdateEvent>()), Times.Once);
        }

        [TestMethod]
        public void SetMemberValue_Invalid_Inputs_ShouldNotChangeState()
        {
            _subject.Begin(_memberValues.Keys.ToList());

            _subject.SetMemberValue("Current_Jackpot_Number_0", _memberValues["Current_Jackpot_Number_0"]);
            _subject.SetMemberValue("Jackpot_Controller_Id_ByteOne_0", _memberValues["Jackpot_Controller_Id_ByteOne_0"]);
            _subject.SetMemberValue("Jackpot_Controller_Id_ByteTwo_0", _memberValues["Jackpot_Controller_Id_ByteTwo_0"]);
            _subject.SetMemberValue("Jackpot_Controller_Id_ByteThree_0", _memberValues["Jackpot_Controller_Id_ByteThree_0"]);

            _subject.SetMemberValue("Invalid Member Name_0", 123);
            _memberValues.Keys.ToList().ForEach(f => Assert.AreEqual(_memberValues[f], long.Parse(_subject.GetMemberValue(f).ToString())));

            _subject.SetMemberValue("Invalid Input", 111);
            _memberValues.Keys.ToList().ForEach(f => Assert.AreEqual(_memberValues[f], long.Parse(_subject.GetMemberValue(f).ToString())));
        }

        [TestMethod]
        public void BeginAndRollback_ShouldCacheAndRestoreCurrentValues()
        {
            _subject.Begin(_memberValues.Keys.ToList());

            _subject.SetMemberValue("Current_Jackpot_Number_0", 123);
            _subject.SetMemberValue("Jackpot_Controller_Id_ByteOne_0", 1);
            _subject.SetMemberValue("Jackpot_Controller_Id_ByteTwo_0", 2);
            _subject.SetMemberValue("Jackpot_Controller_Id_ByteThree_0", 3);

            Assert.IsTrue(ValuesMatch(_accessor.CurrentState[0], 0, 123, 1, 2, 3));

            //No backup state for this level at this point
            Assert.IsTrue(ValuesMatch(_accessor.BackupState, 0));

            _subject.Begin(_memberValues.Keys.ToList());

            //CurrentState copied to BackupState
            Assert.IsTrue(ValuesMatch(_accessor.BackupState, 0, 123, 1, 2, 3));

            _subject.SetMemberValue("Current_Jackpot_Number_0", 321);
            _subject.SetMemberValue("Jackpot_Controller_Id_ByteOne_0", 3);
            _subject.SetMemberValue("Jackpot_Controller_Id_ByteTwo_0", 2);
            _subject.SetMemberValue("Jackpot_Controller_Id_ByteThree_0", 1);

            //CurrentState updated with new values
            Assert.IsTrue(ValuesMatch(_accessor.CurrentState[0], 0, 321, 3, 2, 1));

            _subject.RollBack();

            //CurrentState replaced with BackupState
            Assert.IsTrue(ValuesMatch(_accessor.CurrentState[0], 0, 123, 1, 2, 3));
        }

        [TestMethod]
        public void CommitRollback_InvalidLevelId()
        {
            var nullLevelIdJackpot = new JackpotNumberAndControllerIdState { LevelId = null };
            var validLevelIdJackpot = new JackpotNumberAndControllerIdState { LevelId = 1, JackpotNumber = 1 };
            var eventDictionary = new Dictionary<int, JackpotNumberAndControllerIdState>();
            var updatedEvent = new ProgressiveManageUpdatedEvent(eventDictionary);

            Assert.IsNotNull(_progressiveManageUpdatedEventCallback);
            eventDictionary[0] = nullLevelIdJackpot;
            _progressiveManageUpdatedEventCallback(updatedEvent);

            _subject.Begin(_memberValues.Keys.ToList());

            // Backup LevelId is null
            _subject.Commit();

            _eventBus.Verify(v => v.Publish(It.IsAny<JackpotNumberAndControllerIdUpdateEvent>()), Times.Never);

            eventDictionary[0] = validLevelIdJackpot;
            _progressiveManageUpdatedEventCallback(updatedEvent);

            // Backup LevelId should still be null
            _subject.RollBack();

            Assert.AreEqual((long)1, _subject.GetMemberValue("Current_Jackpot_Number_0"));
        }

        [TestMethod]
        public void Dispose_ShouldUnsubscribeAll()
        {
            //Call dispose twice - should only unsubscribe/deregister from events once
            _subject.Dispose();
            _subject.Dispose();

            _eventBus.Verify(s => s.UnsubscribeAll(It.IsAny<object>()), Times.Once);
        }

        private bool ValuesMatch(JackpotNumberAndControllerIdState state, int levelId, long? jackpotNumber = null, int? byteOne = null, int? byteTwo = null, int? byteThree = null)
        {
            return state.LevelId == levelId &&
                state.JackpotNumber == jackpotNumber &&
                state.JackpotControllerIdByteOne == byteOne &&
                state.JackpotControllerIdByteTwo == byteTwo &&
                state.JackpotControllerIdByteThree == byteThree;
        }
    }
}

