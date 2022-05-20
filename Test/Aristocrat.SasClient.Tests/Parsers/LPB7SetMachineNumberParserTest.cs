namespace Aristocrat.SasClient.Tests.Parsers
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;
    using Sas.Client.LongPollDataClasses;
    using Sas.Client.LPParsers;

    /// <summary>
    ///     Contains the tests for the LPB7SetMachineNumberParser class
    /// </summary>
    [TestClass]
    public class LPB7SetMachineNumberParserTest
    {
        private readonly LPB7SetMachineNumberParser _target = new LPB7SetMachineNumberParser();

        private readonly Mock<ISasLongPollHandler<LongPollSetMachineNumbersResponse, LongPollSetMachineNumbersData>>
            _handler =
                new Mock<ISasLongPollHandler<LongPollSetMachineNumbersResponse, LongPollSetMachineNumbersData>>(
                    MockBehavior.Strict);

        [TestMethod]
        public void CommandTest()
        {
            Assert.AreEqual(LongPoll.SetMachineNumbers, _target.Command);
        }

        [TestMethod]
        public void ParseSucceedTest()
        {
            const int assetNumber = 12345;
            const byte assetNumberHeaderSize = 5;
            const string floorLocation = "northeast";
            const byte assetNumberAndLengthSize = 6;
            const MachineNumbersControlFlags flags = MachineNumbersControlFlags.FloorLocationConfigurable |
                                                     MachineNumbersControlFlags.AssetNumberConfigurable;
            const byte supportFlags = 0x03;
            const int assetNumberLength = 4;
            var floorLocationByteArray = System.Text.Encoding.ASCII.GetBytes(floorLocation);

            var responseGood = new LongPollSetMachineNumbersResponse(flags, assetNumber, floorLocation);
            var expectedGood = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SetMachineNumbers,
                (byte)(assetNumberAndLengthSize + floorLocationByteArray.Length),
                supportFlags
            };
            expectedGood.AddRange(Utilities.ToBinary(assetNumber, assetNumberLength));
            expectedGood.Add((byte)floorLocationByteArray.Length);
            expectedGood.AddRange(floorLocationByteArray);

            var command = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SetMachineNumbers,
                (byte)(floorLocationByteArray.Length + assetNumberHeaderSize)
            };
            command.AddRange(Utilities.ToBinary(assetNumber, assetNumberLength));
            command.Add((byte)floorLocationByteArray.Length);
            command.AddRange(floorLocationByteArray);

            _handler.Setup(m => m.Handle(It.IsAny<LongPollSetMachineNumbersData>())).Returns(responseGood);
            _target.InjectHandler(_handler.Object);

            var resultCommand = _target.Parse(command);
            Assert.IsNotNull(resultCommand);
            var actual = resultCommand.ToList();
            CollectionAssert.AreEqual(expectedGood, actual);
        }

        /// <summary>
        ///     Test for query only of assets
        /// </summary>
        /// <remarks>
        ///     This test sets asset number and floor location size to 0 to query these two elements. Also sets the config set flags to false.
        /// </remarks>
        [TestMethod]
        public void ParseQueryOnly()
        {
            const int assetNumber = 0;
            const int systemAssetNumber = 12345;
            const byte assetNumberSize = 5;
            const string floorLocation = "";
            const string systemFloorLocation = "northeast";
            const MachineNumbersControlFlags flags = MachineNumbersControlFlags.None;
            const byte assetNumberAndLengthSize = 6;
            const byte supportFlags = 0x0;
            const int assetNumberLength = 4;
            var responseGood = new LongPollSetMachineNumbersResponse(flags, systemAssetNumber, systemFloorLocation);
            var systemFloorLocationByteArray = System.Text.Encoding.ASCII.GetBytes(systemFloorLocation);

            // command will be an asset number and floor length 0 to simply query.
            var command = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SetMachineNumbers, assetNumberSize
            };
            command.AddRange(Utilities.ToBinary(assetNumber, assetNumberLength));
            command.Add((byte)floorLocation.Length);
            command.Add(TestConstants.FakeCrc);
            command.Add(TestConstants.FakeCrc);

            // this is a query so we should get back a floor location and asset number. 
            var expectedGood = new List<byte>
            {
                TestConstants.SasAddress,
                (byte)LongPoll.SetMachineNumbers,
                (byte)(assetNumberAndLengthSize + systemFloorLocationByteArray.Length),
                supportFlags
            };

            expectedGood.AddRange(Utilities.ToBinary(systemAssetNumber, assetNumberLength));
            expectedGood.Add((byte)systemFloorLocationByteArray.Length);
            expectedGood.AddRange(systemFloorLocationByteArray);

            _handler.Setup(m => m.Handle(It.IsAny<LongPollSetMachineNumbersData>())).Returns(responseGood);
            _target.InjectHandler(_handler.Object);

            var actual = _target.Parse(command).ToList();
            CollectionAssert.AreEqual(expectedGood, actual);
        }

        [TestMethod]
        public void ParseFailedLengthTest()
        {
            var command = new List<byte> { TestConstants.SasAddress, (byte)LongPoll.SetMachineNumbers };

            var expectedResult = new List<byte> { TestConstants.SasAddress | SasConstants.Nack };

            _handler.Setup(m => m.Handle(It.IsAny<LongPollSetMachineNumbersData>()))
                .Returns((LongPollSetMachineNumbersResponse)null);
            _target.InjectHandler(_handler.Object);

            var actual = _target.Parse(command).ToList();
            CollectionAssert.AreEqual(expectedResult, actual);
        }

        [TestMethod]
        public void ParseFailedFloorLocationLengthTest()
        {
            const byte assetNumberSize = 5;
            const int assetNumber = 0;
            const int assetNumberLength = 4;
            const string floorLocation = "12345678901234567890123456789012345678901";
            var command = new List<byte>
            {
                TestConstants.SasAddress, (byte)LongPoll.SetMachineNumbers, assetNumberSize
            };
            command.AddRange(Utilities.ToBinary(assetNumber, assetNumberLength));
            command.Add((byte)floorLocation.Length);
            command.Add(TestConstants.FakeCrc);
            command.Add(TestConstants.FakeCrc);

            var expectedResult = new List<byte> { TestConstants.SasAddress | SasConstants.Nack };

            _handler.Setup(m => m.Handle(It.IsAny<LongPollSetMachineNumbersData>()))
                .Returns((LongPollSetMachineNumbersResponse)null);
            _target.InjectHandler(_handler.Object);

            var actual = _target.Parse(command).ToList();
            CollectionAssert.AreEqual(expectedResult, actual);
        }
    }
}