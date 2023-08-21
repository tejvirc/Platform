namespace Aristocrat.Monaco.Asp.Tests.Client.DataSources
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Aristocrat.Monaco.Asp.Client.DataSources;
    using Aristocrat.Monaco.Asp.Progressive;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System.Threading;

    [TestClass]
    public class ProgressiveDataSourceTests
    {
        private Dictionary<string, long> _memberValues = new Dictionary<string, long>
        {
            { "Total_Amount_Won_0", 100000 },
            { "Total_Jackpot_Hit_Count_0", 2 },
            { "Jackpot_Reset_Counter_0", 3 },
            { "Jackpot_Hit_Status_0", 4 },
            { "LinkJackpot_HitAmountWon_0", 5 },
            { "ProgressiveJackpot_AmountUpdate_0", 6 },
            { "Display_Meter_0", 1 },
            { "ProgressiveJackpot_AmountUpdate_ForDisplay_0", 6 },
            { "Display_Mode_0", 0 },
            { "Number_Of_LP_Levels", 5 }
        };

        private ProgressiveDataSource _subject;
        private Mock<IProgressiveManager> _progressiveManager;
        private Dictionary<int, ILinkProgressiveLevel> _levels;
        private Mock<ILinkProgressiveLevel> _level;

        [TestInitialize]
        public virtual void TestInitialize()
        {
            _progressiveManager = new Mock<IProgressiveManager>();

            _level = new Mock<ILinkProgressiveLevel>();

            _level.SetupGet(s => s.TotalAmountWon).Returns(() => _memberValues["Total_Amount_Won_0"]).Verifiable();
            _level.SetupGet(s => s.TotalJackpotHitCount).Returns(() => _memberValues["Total_Jackpot_Hit_Count_0"]).Verifiable();
            _level.SetupGet(s => s.JackpotResetCounter).Returns(() => _memberValues["Jackpot_Reset_Counter_0"]).Verifiable();
            _level.SetupGet(s => s.JackpotHitStatus).Returns(() => _memberValues["Jackpot_Hit_Status_0"]).Verifiable();
            _level.SetupGet(s => s.LinkJackpotHitAmountWon).Returns(() => _memberValues["LinkJackpot_HitAmountWon_0"]).Verifiable();
            _level.SetupGet(s => s.ProgressiveJackpotAmountUpdate).Returns(() => _memberValues["ProgressiveJackpot_AmountUpdate_0"]).Verifiable();

            _levels = new Dictionary<int, ILinkProgressiveLevel>
            {
                { 0, _level.Object },
                { 2, _level.Object },
                { 3, _level.Object },
                { 4, _level.Object },
                { 5, _level.Object }
            };

            _progressiveManager.SetupGet(s => s.Levels).Returns(() => new ReadOnlyDictionary<int, ILinkProgressiveLevel>(_levels));

            _subject = new ProgressiveDataSource(_progressiveManager.Object);
        }

        [TestMethod]
        public void NullConstructorTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new ProgressiveDataSource(null));
        }

        [TestMethod]
        public void DataSourceNameTest()
        {
            var expectedName = "LinkProgressive";
            Assert.AreEqual(expectedName, _subject.Name);
        }

        [TestMethod]
        public void MembersTest()
        {
            var expectedMembers = new List<string>();

            for (int progressiveLevel = 0; progressiveLevel < ProgressiveConstants.LinkProgressiveMaxLevels; progressiveLevel++)
            {
                expectedMembers.Add($"Total_Amount_Won_{progressiveLevel}");
                expectedMembers.Add($"Total_Jackpot_Hit_Count_{progressiveLevel}");
                expectedMembers.Add($"Jackpot_Reset_Counter_{progressiveLevel}");
                expectedMembers.Add($"Jackpot_Hit_Status_{progressiveLevel}");
                expectedMembers.Add($"LinkJackpot_HitAmountWon_{progressiveLevel}");
                expectedMembers.Add($"ProgressiveJackpot_AmountUpdate_{progressiveLevel}");
                expectedMembers.Add($"Display_Meter_{progressiveLevel}");
                expectedMembers.Add($"ProgressiveJackpot_AmountUpdate_ForDisplay_{progressiveLevel}");
            }
            expectedMembers.Add("Number_Of_LP_Levels");

            var actualMembers = _subject.Members;
            Assert.AreEqual(expectedMembers.Count, actualMembers.Count);
            Assert.IsTrue(actualMembers.SequenceEqual(expectedMembers));
        }

        [TestMethod]
        public void GetMemberValue_ReturnsCorrectValue()
        {
            _memberValues.Keys.Where(w => w != "Total_Amount_Won_0").ToList().ForEach(f => Assert.IsTrue(long.Parse(_subject.GetMemberValue(f).ToString()) == _memberValues[f]));

            Assert.IsTrue(long.Parse(_subject.GetMemberValue("Total_Amount_Won_0").ToString()) == _memberValues["Total_Amount_Won_0"]);

            _level.Verify(s => s.TotalAmountWon, Times.Once);
            _level.Verify(s => s.TotalJackpotHitCount, Times.Once);
            _level.Verify(s => s.JackpotResetCounter, Times.Once);
            _level.Verify(s => s.JackpotHitStatus, Times.Once);
            _level.Verify(s => s.LinkJackpotHitAmountWon, Times.Once);
            _level.Verify(s => s.ProgressiveJackpotAmountUpdate, Times.Exactly(2));
        }

        [TestMethod]
        public void GetMemberValue_InvalidInput()
        {
            Assert.AreEqual(0, _subject.GetMemberValue("Invalid_Member_Name_Invalid_LevelId"));
            Assert.AreEqual(0, _subject.GetMemberValue("Total_Amount_Won_123"));
            Assert.AreEqual(0, _subject.GetMemberValue("Invalid_Name_Valid_LevelId_0"));
        }

        [TestMethod]
        public void GetMemberValue_MissingProgressiveLevel()
        {
            _progressiveManager.SetupGet(s => s.Levels).Returns(() => new Dictionary<int, ILinkProgressiveLevel> { {1, null } });
            Assert.AreEqual(0, _subject.GetMemberValue("Total_Jackpot_Hit_Count_1"));
        }

        [TestMethod]
        public void SetMemberValue_LinkJackpot_HitAmountWon_ShouldUpdateProgressiveManager()
        {
            const int timeout = 1000;

            using (var waiter = new ManualResetEventSlim(false))
            {
                _progressiveManager.Setup(v => v.UpdateLinkJackpotHitAmountWon(It.Is<int>(i => i == 0), It.Is<long>(i => i == 123))).Callback(() => waiter.Set()).Verifiable();

                _subject.SetMemberValue("LinkJackpot_HitAmountWon_0", 123);
                Assert.IsTrue(waiter.Wait(timeout));
            }

            _progressiveManager.Verify(v => v.UpdateLinkJackpotHitAmountWon(It.Is<int>(i => i == 0), It.Is<long>(i => i == 123)), Times.Once);
        }

        //[TestMethod]
        //public void SetMemberValue_ProgressiveJackpot_AmountUpdate_ShouldUpdateProgressiveManager()
        //{
        //    _subject.SetMemberValue("ProgressiveJackpot_AmountUpdate_0", 123);

        //    _progressiveManager.Verify(v => v.UpdateProgressiveJackpotAmountUpdate(It.Is<int>(i => i == 0), It.Is<long>(i => i == 123)), Times.Once);
        //}

        //[TestMethod]
        //public void SetMemberValue_ProgressiveJackpot_AmountUpdate_ForDisplay_ShouldUpdateProgressiveManager()
        //{
        //    _subject.SetMemberValue("ProgressiveJackpot_AmountUpdate_ForDisplay_0", 123);

        //    _progressiveManager.Verify(v => v.UpdateProgressiveJackpotAmountUpdate(It.Is<int>(i => i == 0), It.Is<long>(i => i == 123)), Times.Once);
        //}

        //[TestMethod]
        //public void SetMemberValue_InvalidInput()
        //{
        //    _subject.SetMemberValue("Invalid_Name_ValidLevelId_0", 1);
        //    _subject.SetMemberValue("Invalid_Name_Invalid_LevelId", 1);

        //    _progressiveManager.Verify(v => v.UpdateProgressiveJackpotAmountUpdate(It.Is<int>(i => i == 0), It.Is<long>(i => i == 123)), Times.Never);
        //}

        [TestMethod]
        public void OnNotificationEvent_ShouldFireMemberValueChanged()
        {
            var calledMembers = new HashSet<string>();
            _subject.MemberValueChanged += (s, e) =>
            {
                Assert.AreEqual(1, e.Count);
                calledMembers.Add(e.First().Key);
            };

            _progressiveManager.Raise(v => v.OnNotificationEvent += null, this, new OnNotificationEventArgs(new Dictionary<int, IReadOnlyList<string>> { { 0, new List<string> { ProgressivePerLevelMeters.JackpotHitStatus, ProgressivePerLevelMeters.JackpotResetCounter } } }));

            var expectedMembers = new HashSet<string> { "Jackpot_Hit_Status_0", "Jackpot_Reset_Counter_0" };
            CollectionAssert.AreEquivalent(calledMembers.ToList(), expectedMembers.ToList());
        }

        [TestMethod]
        public void Dispose_ShouldUnsubscribeAll()
        {
            _subject.Dispose();
            _subject.Dispose();

            _progressiveManager.Verify(v => v.Dispose(), Times.Once);
        }
    }
}

