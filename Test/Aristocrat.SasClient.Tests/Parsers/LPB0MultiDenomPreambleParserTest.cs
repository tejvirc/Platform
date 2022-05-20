namespace Aristocrat.SasClient.Tests.Parsers
{
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;
    using Sas.Client.LPParsers;

    [TestClass]
    public class LPB0MultiDenomPreambleParserTest
    {
        private Mock<ISasParserFactory> _factory;
        private LPB0MultiDenomPreambleParser _target;
        private const byte _badDenom = 0xFF;
        private const byte _noDenom = 0x0;
        private const ulong _oneCentInMeterValue = 5935235;
        private const ulong _oneCentOutMeterValue = 534123;
        private const byte _oneCentDenom = 0x1;
        private const ulong _fiveCentInMeterValue = 121234;
        private const ulong _fiveCentOutMeterValue = 132451;
        private const byte _fiveCentDenom = 0x2;
        private const ulong _totalCoinIn = _oneCentInMeterValue + _fiveCentInMeterValue;
        private const ulong _totalCoinOut = _oneCentOutMeterValue + _fiveCentOutMeterValue;

        [TestInitialize]
        public void TestInitialize()
        {
            _factory = new Mock<ISasParserFactory>(MockBehavior.Strict);

            var LP11Mock = new Mock<IMultiDenomAwareLongPollParser>(MockBehavior.Strict);
            LP11Mock.Setup(c => c.Parse(
                It.IsAny<IReadOnlyCollection<byte>>(),
                It.Is<long>(e => e == DenominationCodes.GetDenominationForCode(_noDenom))
                )).Returns(GenerateMockParserResponseBytes(LongPoll.SendCoinInMeter, _totalCoinIn));
            LP11Mock.Setup(c => c.Parse(
                It.IsAny<IReadOnlyCollection<byte>>(),
                It.Is<long>(e => e == DenominationCodes.GetDenominationForCode(_oneCentDenom))
                )).Returns(GenerateMockParserResponseBytes(LongPoll.SendCoinInMeter, _oneCentInMeterValue));
            LP11Mock.Setup(c => c.Parse(
                It.IsAny<IReadOnlyCollection<byte>>(),
                It.Is<long>(e => e == DenominationCodes.GetDenominationForCode(_fiveCentDenom))
                )).Returns(GenerateMockParserResponseBytes(LongPoll.SendCoinInMeter, _fiveCentInMeterValue));
            _factory.Setup(c => c.GetParserForLongPoll(LongPoll.SendCoinInMeter)).Returns(LP11Mock.Object);

            var LP12Mock = new Mock<IMultiDenomAwareLongPollParser>(MockBehavior.Strict);
            LP12Mock.Setup(c => c.Parse(
                It.IsAny<IReadOnlyCollection<byte>>(),
                It.Is<long>(e => e == DenominationCodes.GetDenominationForCode(_noDenom))
                )).Returns(GenerateMockParserResponseBytes(LongPoll.SendCoinOutMeter, _totalCoinOut));
            LP12Mock.Setup(c => c.Parse(
                It.IsAny<IReadOnlyCollection<byte>>(),
                It.Is<long>(e => e == DenominationCodes.GetDenominationForCode(_oneCentDenom))
                )).Returns(GenerateMockParserResponseBytes(LongPoll.SendCoinOutMeter, _oneCentOutMeterValue));
            LP12Mock.Setup(c => c.Parse(
                It.IsAny<IReadOnlyCollection<byte>>(),
                It.Is<long>(e => e == DenominationCodes.GetDenominationForCode(_fiveCentDenom))
                )).Returns(GenerateMockParserResponseBytes(LongPoll.SendCoinOutMeter, _fiveCentOutMeterValue));
            _factory.Setup(c => c.GetParserForLongPoll(LongPoll.SendCoinOutMeter)).Returns(LP12Mock.Object);

            _factory.Setup(c => c.GetParserForLongPoll(LongPoll.SendEnabledFeatures)).Returns(new Mock<LPA0SendEnabledFeaturesParser>().Object);

            var LP09Mock = new Mock<IMultiDenomAwareLongPollParser>(MockBehavior.Strict);
            LP09Mock.Setup(c => c.Parse(
                It.IsAny<IReadOnlyCollection<byte>>(),
                It.IsAny<long>()
                )).Returns(GenerateMockParserAckBytes());
            _factory.Setup(c => c.GetParserForLongPoll(LongPoll.EnableDisableGameN)).Returns(LP09Mock.Object);

            // Be sure to add any LongPolls you set up above to this so that it doesn't override them
            // If you don't, you'll get UnhandledLongPollParsers for valid calls
            _factory.Setup(c => c.GetParserForLongPoll(It.Is<LongPoll>(
                d => d != LongPoll.SendCoinInMeter &&
                d != LongPoll.SendCoinOutMeter &&
                d != LongPoll.SendEnabledFeatures &&
                d != LongPoll.EnableDisableGameN
                ))).Returns(new Mock<UnhandledLongPollParser>().Object);

            _target = new LPB0MultiDenomPreambleParser(_factory.Object);
        }

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.MultiDenominationPreamble, _target.Command);
        }

        [TestMethod]
        public void OneCentCoinInTest()
        {
            var incomingCommand = GeneratePreambleInOutBytes(_oneCentDenom, GenerateMeterCommandBytes(LongPoll.SendCoinInMeter));
            var expectedOutcome = GeneratePreambleInOutBytes(_oneCentDenom, GenerateMeterResponseBytes(LongPoll.SendCoinInMeter, _oneCentInMeterValue)).ToList();
            var actualOutcome = _target.Parse(incomingCommand).ToList();
            
            CollectionAssert.AreEqual(expectedOutcome, actualOutcome);
        }

        [TestMethod]
        public void FiveCentCoinOutTest()
        {
            var incomingCommand = GeneratePreambleInOutBytes(_fiveCentDenom, GenerateMeterCommandBytes(LongPoll.SendCoinOutMeter));
            var expectedOutcome = GeneratePreambleInOutBytes(_fiveCentDenom, GenerateMeterResponseBytes(LongPoll.SendCoinOutMeter, _fiveCentOutMeterValue)).ToList();
            var actualOutcome = _target.Parse(incomingCommand).ToList();
            
            CollectionAssert.AreEqual(expectedOutcome, actualOutcome);
        }

        [TestMethod]
        public void NotMultiDenomAwareTest()
        {
            var incomingCommand = GeneratePreambleInOutBytes(_fiveCentDenom, GenerateMeterCommandBytes(LongPoll.SendEnabledFeatures));
            var expectedOutcome = GeneratePreambleInOutBytes(_fiveCentDenom, GenerateErrorResponseBytes(MultiDenomAwareErrorCode.NotMultiDenomAware)).ToList();
            var actualOutcome = _target.Parse(incomingCommand).ToList();
            
            CollectionAssert.AreEqual(expectedOutcome, actualOutcome);
        }

        [TestMethod]
        public void BadDenomTest()
        {
            var incomingCommand = GeneratePreambleInOutBytes(_badDenom, GenerateMeterCommandBytes(LongPoll.SendCoinInMeter));
            var expectedOutcome = GeneratePreambleInOutBytes(_badDenom, GenerateErrorResponseBytes(MultiDenomAwareErrorCode.NotValidPlayerDenom)).ToList();
            var actualOutcome = _target.Parse(incomingCommand).ToList();
            
            CollectionAssert.AreEqual(expectedOutcome, actualOutcome);
        }

        [TestMethod]
        public void UnsupportedLongPoll()
        {
            var incomingCommand = GeneratePreambleInOutBytes(_oneCentDenom, GenerateMeterCommandBytes(LongPoll.None));
            var expectedOutcome = GeneratePreambleInOutBytes(_oneCentDenom, GenerateErrorResponseBytes(MultiDenomAwareErrorCode.LongPollNotSupported)).ToList();
            var actualOutcome = _target.Parse(incomingCommand).ToList();
            
            CollectionAssert.AreEqual(expectedOutcome, actualOutcome);
        }

        [TestMethod]
        public void OneCentAckPollTest()
        {
            var baseCommand = new List<byte>
            {
                (byte)LongPoll.EnableDisableGameN,
                0x00, 0x01, //gameID
                0x00 //disable
            };
            var incomingCommand = GeneratePreambleInOutBytes(_oneCentDenom, baseCommand);
            var expectedOutcome = GeneratePreambleInOutBytes(_oneCentDenom, new List<byte> { (byte)LongPoll.EnableDisableGameN, (byte)TestConstants.SasAddress }).ToList();
            var actualOutcome = _target.Parse(incomingCommand).ToList();
            
            CollectionAssert.AreEqual(expectedOutcome, actualOutcome);
        }

        private IReadOnlyCollection<byte> GeneratePreambleInOutBytes(byte targetDenom, IReadOnlyCollection<byte> targetBytes)
        {
            var byteList = new List<byte>();
            byteList.Add(TestConstants.SasAddress);
            byteList.Add((byte)LongPoll.MultiDenominationPreamble);

            var postLengthBytes = new List<byte>();
            postLengthBytes.Add(targetDenom);
            postLengthBytes.AddRange(targetBytes);

            byteList.Add((byte)postLengthBytes.Count);
            byteList.AddRange(postLengthBytes);
            return byteList;
        }
        
        // The command is as follows:
        // Field          Bytes       Value       Description
        // Address          1         01-7F       Gaming Machine Address
        // Command          1           11        Send Total Coin In Meter
        //
        // Response
        // Field          Bytes       Value       Description
        // Address          1         01-7F       Gaming Machine Address
        // Command          1           11        Send Total Coin In Meter
        // Meter         4 BCD 00000000-99999999  four byte BCD total coin in meter value
        // CRC              2        0000-FFFF    16-bit CRC
        private IReadOnlyCollection<byte> GenerateMeterCommandBytes(LongPoll targetPoll)
        {
            return new List<byte> { (byte)targetPoll };
        }
        private IReadOnlyCollection<byte> GenerateMeterResponseBytes(LongPoll targetPoll, ulong value)
        {
            var byteList = new List<byte>();
            byteList.Add((byte)targetPoll);
            byteList.AddRange(Utilities.ToBcd(value, 4));
            return byteList;
        }
        private IReadOnlyCollection<byte> GenerateErrorResponseBytes(MultiDenomAwareErrorCode errorCode)
        {
            return new List<byte> { (byte)LongPoll.None, (byte)errorCode };
        }

        private IReadOnlyCollection<byte> GenerateMockParserResponseBytes(LongPoll targetPoll, ulong actualValue)
        {
            var responseBytes = new List<byte>();
            responseBytes.Add(TestConstants.SasAddress);
            responseBytes.Add((byte)targetPoll);
            responseBytes.AddRange(Utilities.ToBcd(actualValue, 4));
            return responseBytes;
        }

        private IReadOnlyCollection<byte> GenerateMockParserAckBytes()
        {
            return new List<byte> { TestConstants.SasAddress };
        }
    }
}