namespace Aristocrat.Monaco.Asp.Tests.Client.Utilities
{
    using Aristocrat.Monaco.Common;
    using Aristocrat.Monaco.Asp.Client.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class EnumParserTests
    {
        [DataRow(0, GameDisableReason.OtherEgmLockups, DisplayName = "Int value 0")]
        [DataRow(1, GameDisableReason.HostInitiated, DisplayName = "Int value 1")]
        [DataRow(2, GameDisableReason.LinkProgressiveCommsFailure, DisplayName = "Int value 2")]
        [DataRow(3, GameDisableReason.VenueShutdown, DisplayName = "Int value 3")]
        [DataRow(4, GameDisableReason.TemporarilyUnavailable, DisplayName = "Int value 4")]
        [DataRow(5, GameDisableReason.Emergency, DisplayName = "Int value 5")]
        [DataRow(6, GameDisableReason.SoftwareSignatureFailure, DisplayName = "Int value 6")]
        [DataRow(7, GameDisableReason.LargeWin, DisplayName = "Int value 7")]
        [DataRow(8, GameDisableReason.PowerUp, DisplayName = "Int value 8")]
        [DataRow(9, GameDisableReason.LogicSealBroken, DisplayName = "Int value 9")]
        [DataRow(0x00, GameDisableReason.OtherEgmLockups, DisplayName = "Hex value 0x00")]
        [DataRow(0x01, GameDisableReason.HostInitiated, DisplayName = "Hex value 0x01")]
        [DataRow(0x02, GameDisableReason.LinkProgressiveCommsFailure, DisplayName = "Hex value 0x02")]
        [DataRow(0x03, GameDisableReason.VenueShutdown, DisplayName = "Hex value 0x03")]
        [DataRow(0x04, GameDisableReason.TemporarilyUnavailable, DisplayName = "Hex value 0x04")]
        [DataRow(0x05, GameDisableReason.Emergency, DisplayName = "Hex value 0x05")]
        [DataRow(0x06, GameDisableReason.SoftwareSignatureFailure, DisplayName = "Hex value 0x06")]
        [DataRow(0x07, GameDisableReason.LargeWin, DisplayName = "Hex value 0x07")]
        [DataRow(0x08, GameDisableReason.PowerUp, DisplayName = "Hex value 0x08")]
        [DataRow(0x09, GameDisableReason.LogicSealBroken, DisplayName = "Hex value 0x09")]
        [DataRow("0", GameDisableReason.OtherEgmLockups, DisplayName = "String value '0'")]
        [DataRow("1", GameDisableReason.HostInitiated, DisplayName = "String value '1'")]
        [DataRow("2", GameDisableReason.LinkProgressiveCommsFailure, DisplayName = "String value '2'")]
        [DataRow("3", GameDisableReason.VenueShutdown, DisplayName = "String value '3'")]
        [DataRow("4", GameDisableReason.TemporarilyUnavailable, DisplayName = "String value '4'")]
        [DataRow("5", GameDisableReason.Emergency, DisplayName = "String value '5'")]
        [DataRow("6", GameDisableReason.SoftwareSignatureFailure, DisplayName = "String value '6'")]
        [DataRow("7", GameDisableReason.LargeWin, DisplayName = "String value '7'")]
        [DataRow("8", GameDisableReason.PowerUp, DisplayName = "String value '8'")]
        [DataRow("9", GameDisableReason.LogicSealBroken, DisplayName = "String value '9'")]
        [DataRow("OtherEgmLockups", GameDisableReason.OtherEgmLockups, DisplayName = "String value 'OtherEgmLockups'")]
        [DataRow("HostInitiated", GameDisableReason.HostInitiated, DisplayName = "String value 'HostInitiated'")]
        [DataRow("LinkProgressiveCommsFailure", GameDisableReason.LinkProgressiveCommsFailure, DisplayName = "String value 'LinkProgressiveCommsFailure'")]
        [DataRow("VenueShutdown", GameDisableReason.VenueShutdown, DisplayName = "String value 'VenueShutdown'")]
        [DataRow("TemporarilyUnavailable", GameDisableReason.TemporarilyUnavailable, DisplayName = "String value 'TemporarilyUnavailable'")]
        [DataRow("Emergency", GameDisableReason.Emergency, DisplayName = "String value 'Emergency'")]
        [DataRow("SoftwareSignatureFailure", GameDisableReason.SoftwareSignatureFailure, DisplayName = "String value 'SoftwareSignatureFailure'")]
        [DataRow("LargeWin", GameDisableReason.LargeWin, DisplayName = "String value 'LargeWin'")]
        [DataRow("PowerUp", GameDisableReason.PowerUp, DisplayName = "String value 'PowerUp'")]
        [DataRow("LogicSealBroken", GameDisableReason.LogicSealBroken, DisplayName = "String value 'LogicSealBroken'")]
        [DataRow("otherEgmLockups", GameDisableReason.OtherEgmLockups, DisplayName = "String value wrong case 'otherEgmLockups'")]
        [DataRow("hostInitiated", GameDisableReason.HostInitiated, DisplayName = "String value wrong case 'hostInitiated'")]
        [DataRow("linkProgressiveCommsFailure", GameDisableReason.LinkProgressiveCommsFailure, DisplayName = "String value wrong case 'linkProgressiveCommsFailure'")]
        [DataRow("venueShutdown", GameDisableReason.VenueShutdown, DisplayName = "String value wrong case 'venueShutdown'")]
        [DataRow("temporarilyUnavailable", GameDisableReason.TemporarilyUnavailable, DisplayName = "String value wrong case 'temporarilyUnavailable'")]
        [DataRow("emergency", GameDisableReason.Emergency, DisplayName = "String value wrong case 'emergency'")]
        [DataRow("softwareSignatureFailure", GameDisableReason.SoftwareSignatureFailure, DisplayName = "String value wrong case 'softwareSignatureFailure'")]
        [DataRow("largeWin", GameDisableReason.LargeWin, DisplayName = "String value wrong case 'largeWin'")]
        [DataRow("powerUp", GameDisableReason.PowerUp, DisplayName = "String value wrong case 'powerUp'")]
        [DataRow("logicSealBroken", GameDisableReason.LogicSealBroken, DisplayName = "String value 'logicSealBroken'")]
        [DataRow(GameDisableReason.OtherEgmLockups, GameDisableReason.OtherEgmLockups, DisplayName = "Enum value GameDisableReason.OtherEgmLockups")]
        [DataRow(GameDisableReason.HostInitiated, GameDisableReason.HostInitiated, DisplayName = "Enum value GameDisableReason.HostInitiated")]
        [DataRow(GameDisableReason.LinkProgressiveCommsFailure, GameDisableReason.LinkProgressiveCommsFailure, DisplayName = "Enum value GameDisableReason.LinkProgressiveCommsFailure")]
        [DataRow(GameDisableReason.VenueShutdown, GameDisableReason.VenueShutdown, DisplayName = "Enum value GameDisableReason.VenueShutdown")]
        [DataRow(GameDisableReason.TemporarilyUnavailable, GameDisableReason.TemporarilyUnavailable, DisplayName = "Enum value GameDisableReason.TemporarilyUnavailable")]
        [DataRow(GameDisableReason.Emergency, GameDisableReason.Emergency, DisplayName = "Enum value GameDisableReason.Emergency")]
        [DataRow(GameDisableReason.SoftwareSignatureFailure, GameDisableReason.SoftwareSignatureFailure, DisplayName = "Enum value GameDisableReason.SoftwareSignatureFailure")]
        [DataRow(GameDisableReason.LargeWin, GameDisableReason.LargeWin, DisplayName = "Enum value GameDisableReason.LargeWin")]
        [DataRow(GameDisableReason.PowerUp, GameDisableReason.PowerUp, DisplayName = "Enum value GameDisableReason.PowerUp")]
        [DataRow(GameDisableReason.LogicSealBroken, GameDisableReason.LogicSealBroken, DisplayName = "Enum value GameDisableReason.LogicSealBroken")]
        [TestMethod]
        public void ParsesValidValues(object value, GameDisableReason? enumValue)
        {
            Assert.AreEqual(EnumParser.Parse<GameDisableReason>(value), (true, enumValue));
        }

        [DataRow("", DisplayName = "Empty string")]
        [DataRow(-1, DisplayName = "Negative number")]
        [DataRow("-1", DisplayName = "Negative number string")]
        [DataRow(null, DisplayName = "Null value")]
        [DataRow("Sandwich", DisplayName = "Invalid string value")]
        [DataRow(11, DisplayName = "Int outside enum range")]
        [DataRow(0x11, DisplayName = "Hex outside enum range")]
        [DataRow("11", DisplayName = "Int string outside enum range")]
        [DataRow(2147483648, DisplayName = "Int overflow")]
        [DataRow(GameEnableStatus.EnableGame, DisplayName = "Value from other enum")]
        [TestMethod]
        public void RejectsInvalidValues(object value)
        {
            Assert.AreEqual(EnumParser.Parse<GameDisableReason>(value), (false, null));
        }
    }
}