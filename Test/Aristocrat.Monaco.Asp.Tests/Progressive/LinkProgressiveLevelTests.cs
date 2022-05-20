namespace Aristocrat.Monaco.Asp.Tests.Progressive
{
    using Aristocrat.Monaco.Asp.Progressive;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives.Linked;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;

    [TestClass]
    public class LinkProgressiveLevelTests
    {
        private static long _currentJackpotControllerId = 0;
        private readonly Func<int, string, long> _getMeterValue = (int levelId, string meterName) => _currentJackpotControllerId;

        private readonly LinkedProgressiveLevel _linkedLevel = new LinkedProgressiveLevel
        {
            ProtocolName = "DACOM",
            ProgressiveGroupId = 1,
            LevelId = 0,
            Amount = 1000,
            Expiration = DateTime.UtcNow + TimeSpan.FromDays(365),
            CurrentErrorStatus = ProgressiveErrors.None,
            ClaimStatus = new LinkedProgressiveClaimStatus
            {
                WinAmount = 1234,
                Status = LinkedClaimState.Hit
            }
        };

        private readonly ProgressiveLevel _progressiveLevel = new ProgressiveLevel
        {
            DeviceId = 1,
            LevelId = 0,
            LevelName = "TestLevelName",
            LevelType = ProgressiveLevelType.LP,
            CurrentValue = 1234
        };

        private LinkProgressiveLevel _subject;

        [TestInitialize]
        public virtual void TestInitialize()
        {
            _subject = new LinkProgressiveLevel(_progressiveLevel.LevelId, _progressiveLevel.LevelName, _getMeterValue, _ => _linkedLevel.Amount);
        }

        [TestMethod]
        public void NullConstructorTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new LinkProgressiveLevel(_progressiveLevel.LevelId, _progressiveLevel.LevelName, null, _ => _linkedLevel.Amount));
            Assert.ThrowsException<ArgumentNullException>(() => new LinkProgressiveLevel(_progressiveLevel.LevelId, _progressiveLevel.LevelName, (int levelId, string meterName) => 0, null));
        }

        [DataRow(123456789, 21, 205, 91)]
        [DataRow(0, 0, 0, 0)]
        [DataRow(1, 1, 0, 0)]
        [DataRow(10, 10, 0, 0)]
        [DataRow(100, 100, 0, 0)]
        [DataRow(1000, 232, 3, 0)]
        [DataRow(10000, 16, 39, 0)]
        [DataRow(100000, 160, 134, 1)]
        [DataRow(1000000, 64, 66, 15)]
        [DataRow(10000000, 128, 150, 152)]
        [DataRow(100000000, 0, 225, 245)]
        [DataRow(999999999, 255, 201, 154)]
        [TestMethod]
        public void GetJackpotControllerId_ParsesCorrectly(long meterValue, int byteOne, int byteTwo, int byteThree)
        {
            _currentJackpotControllerId = meterValue;

            Assert.IsTrue(_subject.JackpotControllerIdByteOne == byteOne, $"Byte 1 - {_subject.JackpotControllerIdByteOne} does not match {byteOne}");
            Assert.IsTrue(_subject.JackpotControllerIdByteTwo == byteTwo, $"Byte 2 - {_subject.JackpotControllerIdByteTwo} does not match {byteTwo}");
            Assert.IsTrue(_subject.JackpotControllerIdByteThree == byteThree, $"Byte 3 - {_subject.JackpotControllerIdByteThree} does not match {byteThree}");

            CheckProperties();
        }

        private void CheckProperties()
        {
            Assert.IsTrue(_subject.JackpotResetCounter == _getMeterValue(_progressiveLevel.LevelId, ProgressivePerLevelMeters.JackpotResetCounter), $"JackpotResetCounter - {_subject.JackpotResetCounter} does not match");
            Assert.IsTrue(_subject.TotalAmountWon == _getMeterValue(_progressiveLevel.LevelId, ProgressivePerLevelMeters.TotalJackpotAmount), $"TotalAmountWon - {_subject.TotalAmountWon} does not match");
            Assert.IsTrue(_subject.TotalJackpotHitCount == _getMeterValue(_progressiveLevel.LevelId, ProgressivePerLevelMeters.TotalJackpotHitCount), $"TotalJackpotHitCount - {_subject.TotalJackpotHitCount} does not match");
            Assert.IsTrue(_subject.LinkJackpotHitAmountWon == _getMeterValue(_progressiveLevel.LevelId, ProgressivePerLevelMeters.LinkJackpotHitAmountWon), $"LinkJackpotHitAmountWon - {_subject.LinkJackpotHitAmountWon} does not match");
            Assert.IsTrue(_subject.JackpotHitStatus == _getMeterValue(_progressiveLevel.LevelId, ProgressivePerLevelMeters.JackpotHitStatus), $"JackpotHitStatus - {_subject.JackpotHitStatus} does not match");
            Assert.IsTrue(_subject.CurrentJackpotNumber == _getMeterValue(_progressiveLevel.LevelId, ProgressivePerLevelMeters.CurrentJackpotNumber), $"CurrentJackpotNumber - {_subject.CurrentJackpotNumber} does not match");
            Assert.IsTrue(_subject.ProgressiveJackpotAmountUpdate == _linkedLevel.Amount, $"ProgressiveJackpotAmountUpdate - {_subject.ProgressiveJackpotAmountUpdate} does not match");
        }
    }
}
