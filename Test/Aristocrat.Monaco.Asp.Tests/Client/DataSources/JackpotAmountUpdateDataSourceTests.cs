namespace Aristocrat.Monaco.Asp.Tests.Client.DataSources
{
    using Aristocrat.Monaco.Asp.Client.DataSources;
    using Aristocrat.Monaco.Asp.Progressive;
    using Aristocrat.Monaco.Test.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [TestClass]
    public class JackpotAmountUpdateDataSourceTests
    {
        private Dictionary<string, object> _initialValues = new Dictionary<string, object>
        {
            { "Link_Progressive_Update_Flag_0", (long)0 },
            { "Link_Progressive_Display_Amount_0", (long)0 },
            { "Link_Mystery_Update_Flag_0", (long)0 },
            { "Link_Mystery_Display_Amount_0", (long)0 },
        };

        private JackpotAmountUpdateDataSource _subject;
        private dynamic _accessor;
        private Mock<IProgressiveManager> _progressiveManager;

        [TestInitialize]
        public void TestInitialize()
        {
            _progressiveManager = new Mock<IProgressiveManager>();

            _progressiveManager.Setup(s => s.GetJackpotAmountsPerLevel()).Returns(SubmitAndBuildExpectedResult(new Dictionary<string, long>()));

            _subject = new JackpotAmountUpdateDataSource(_progressiveManager.Object);

            _accessor = new DynamicPrivateObject(_subject);
        }

        [TestMethod]
        public void NullConstructorTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new JackpotAmountUpdateDataSource(null));
        }

        [TestMethod]
        public void DataSourceNameTest()
        {
            var expectedName = "JackpotAmountUpdate";
            Assert.AreEqual(expectedName, _subject.Name);
        }

        [TestMethod]
        public void MembersTest()
        {
            var expectedMembers = new List<string>();

            for (int progressiveLevel = 0; progressiveLevel < ProgressiveConstants.LinkProgressiveMaxLevels; progressiveLevel++)
            {
                expectedMembers.Add($"Link_Progressive_Update_Flag_{progressiveLevel}");
                expectedMembers.Add($"Link_Progressive_Display_Amount_{progressiveLevel}");
                expectedMembers.Add($"Link_Mystery_Update_Flag_{progressiveLevel}");
                expectedMembers.Add($"Link_Mystery_Display_Amount_{progressiveLevel}");
            }

            var actualMembers = _subject.Members;
            Assert.AreEqual(expectedMembers.Count, actualMembers.Count);
            Assert.IsTrue(actualMembers.SequenceEqual(expectedMembers));
        }

        [TestMethod]
        public void GetMemberValue_ReturnsCorrectValue()
        {
            _initialValues.Keys.ToList().ForEach(f => Assert.IsTrue(Convert.ToInt64(_subject.GetMemberValue(f)).Equals(_initialValues[f])));
        }

        [TestMethod]
        public void GetMemberValue_InvalidInput()
        {
            Assert.AreEqual(0, _subject.GetMemberValue("Invalid_Name_Valid_LevelId_0"));
            Assert.AreEqual(0, _subject.GetMemberValue("Invalid_Name_Invalid_LevelId"));
        }

        //[TestMethod]
        //public void SetMemberValue_ShouldUpdateWhenChangesMade()
        //{
        //    var _publishedChanges = new Dictionary<int, long>();

        //    var changes1 = new Dictionary<int, long>
        //    {
        //        { 0, 1000 },
        //        { 1, 2000 },
        //        { 2, 3000 },
        //        { 3, 4000 },
        //    };

        //    _subject.Begin(_initialValues.Keys.ToList());

        //    changes1.ToList().ForEach(w =>
        //    {
        //        _subject.SetMemberValue($"Link_Progressive_Update_Flag_{w.Key}", 1);
        //        _subject.SetMemberValue($"Link_Progressive_Display_Amount_{w.Key}", w.Value);
        //        _subject.SetMemberValue($"Link_Mystery_Update_Flag_{w.Key}", 1);
        //        _subject.SetMemberValue($"Link_Mystery_Display_Amount_{w.Key}", w.Value);
        //    });

        //    _subject.Commit();
        //    _progressiveManager.Verify(v => v.UpdateProgressiveJackpotAmountUpdate(It.IsAny<Dictionary<int, long>>()), Times.Once);
        //}

        //[TestMethod]
        //public void SetMemberValue_NewLevelId()
        //{
        //    var expectedKey = 10;
        //    var expectedValue = (long)1234;
        //    _subject.SetMemberValue($"Link_Progressive_Update_Flag_10", 1);
        //    _subject.SetMemberValue($"Link_Progressive_Display_Amount_10", (long)1234);

        //    _subject.Commit();
        //    _progressiveManager.Verify(v => v.UpdateProgressiveJackpotAmountUpdate(
        //        It.Is<Dictionary<int, long>>(d => d[expectedKey] == expectedValue)), Times.Once);
        //}

        //[TestMethod]
        //public void SetMemberValue_InvalidInput()
        //{
        //    _subject.SetMemberValue("Invalid_Member_Name_Valid_LevelId_0", 1);
        //    _subject.SetMemberValue("Invalid_Member_Name_Invalid_LevelId", 2);
        //    _subject.SetMemberValue("Link_Progressive_Update_Flag_0", "Invalid_Value");

        //    _subject.Commit();
        //    _progressiveManager.Verify(v => v.UpdateProgressiveJackpotAmountUpdate(It.IsAny<Dictionary<int, long>>()), Times.Never);
        //}

        [TestMethod]
        public void BeginAndRollback_ShouldCacheAndRestoreCurrentValues()
        {
            var update1 = new Dictionary<string, long>
            {
                { "Link_Progressive_Update_Flag_0", 0 },
                { "Link_Progressive_Display_Amount_0", 1000 },
                { "Link_Progressive_Update_Flag_1", 1 },
                { "Link_Progressive_Display_Amount_1", 2000 },
                { "Link_Progressive_Update_Flag_2", 0 },
                { "Link_Progressive_Display_Amount_2", 3000 },
                { "Link_Progressive_Update_Flag_3", 1 },
                { "Link_Progressive_Display_Amount_3", 4000 }
            };

            update1.ToList().ForEach(f => _subject.SetMemberValue(f.Key, f.Value));

            Assert.IsTrue(ValuesMatch(_accessor.CurrentState, SubmitAndBuildExpectedResult(update1)));

            _subject.Begin(_initialValues.Keys.ToList());

            var update2 = new Dictionary<string, long>
            {
                { "Link_Progressive_Update_Flag_0", 1 },
                { "Link_Progressive_Display_Amount_0", 4000 },
                { "Link_Progressive_Update_Flag_1", 0 },
                { "Link_Progressive_Display_Amount_1", 3000 },
                { "Link_Progressive_Update_Flag_2", 1 },
                { "Link_Progressive_Display_Amount_2", 2000 },
                { "Link_Progressive_Update_Flag_3", 0 },
                { "Link_Progressive_Display_Amount_3", 1000 }
            };

            //CurrentState updated with new values
            Assert.IsTrue(ValuesMatch(_accessor.CurrentState, SubmitAndBuildExpectedResult(update2)));

            _subject.RollBack();
        }

        private Dictionary<int, LevelAmountUpdateState> SubmitAndBuildExpectedResult(Dictionary<string, long> updates)
        {
            if (!updates.Any())
            {
                var newState = new Dictionary<int, LevelAmountUpdateState>();
                for (int i = 0; i < 4; i++)
                {
                    newState.Add(i, new LevelAmountUpdateState { Amount = 0, Update = false });
                }

                return newState;
            }

            updates.ToList().ForEach(f => _subject.SetMemberValue(f.Key, f.Value));

            _subject.Commit();

            var expectedState = updates.ToList().Select(s =>
            {
                var (memberName, levelId) = GetMemberDetails(s.Key);
                return new { MemberName = memberName, LevelId = levelId, s.Value };
            })
            .GroupBy(g => g.LevelId)
            .ToDictionary(pair => pair.Key, pair => new LevelAmountUpdateState
            {
                Amount = updates[$"Link_Progressive_Display_Amount_{pair.Key}"],
                Update = updates[$"Link_Progressive_Update_Flag_{pair.Key}"] == 1
            });

            return expectedState;
        }

        private bool ValuesMatch(Dictionary<int, LevelAmountUpdateState> backup, Dictionary<int, LevelAmountUpdateState> current)
        {
            return !backup.Select(s => new { s.Value.Amount, s.Value.Update }).Except(current.Select(s => new { s.Value.Amount, s.Value.Update })).Any();
        }

        private static (string Member, int LevelId) GetMemberDetails(string member)
        {
            var level = member.Split('_').Last();
            var memberName = member.Replace($"_{level}", "");

            return (memberName, int.Parse(level));
        }
    }
}
