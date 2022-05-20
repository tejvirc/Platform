namespace Aristocrat.Monaco.Hhr.Client.Tests.Messages.Converters
{
    using Client.Data;
    using Client.Messages;
    using Client.Messages.Converters;
    using Mappings;
    using AutoMapper;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class HandpayCreateRequestConverterTest
    {
        private HandpayCreateRequestConverter _target;

        [TestInitialize]
        public void Initialize()
        {
            var config = new MapperConfiguration(cfg => { cfg.AddProfile<RequestProfile>(); });
            _target = new HandpayCreateRequestConverter(config.CreateMapper());
        }

        [TestMethod]
        public void Convert_WithMapping_ShouldWork()
        {
            var request = new HandpayCreateRequest
            {
                TransactionId = 432,
                HandpayType = 654,
                PlayerId = "123",
                Amount = 234,
                GameWin = 345,
                GameMapId = 456,
                LastGamePlayTime = 5678,
                LastWager = 624,
                Denomination = 12,
                ProgWin = 34
            };

            var data = _target.Convert(request);
            var mappedData = MessageUtility.ConvertByteArrayToMessage<GMessageCreateHandPayItem>(data);
            Assert.AreEqual(mappedData.TransactionId, request.TransactionId);
            Assert.AreEqual(mappedData.HandpayType, request.HandpayType);
            Assert.AreEqual(mappedData.PlayerId, request.PlayerId);
            Assert.AreEqual(mappedData.Amount, request.Amount);
            Assert.AreEqual(mappedData.GameWin, request.GameWin);
            Assert.AreEqual(mappedData.GameMapId, request.GameMapId);
            Assert.AreEqual(mappedData.LastGamePlayTime, request.LastGamePlayTime);
            Assert.AreEqual(mappedData.LastWager, request.LastWager);
            Assert.AreEqual(mappedData.Denomination, request.Denomination);
            Assert.AreEqual(mappedData.ProgWin, request.ProgWin);
        }
    }
}