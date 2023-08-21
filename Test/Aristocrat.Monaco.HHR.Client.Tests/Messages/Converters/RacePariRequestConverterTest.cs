namespace Aristocrat.Monaco.Hhr.Client.Tests.Messages.Converters
{
    using Data;
    using Client.Messages;
    using Client.Messages.Converters;
    using Mappings;
    using AutoMapper;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RacePariRequestConverterTest
    {
        private RacePariRequestConverter _target;

        [TestInitialize]
        public void Initialize()
        {
            var config = new MapperConfiguration(cfg => { cfg.AddProfile<RequestProfile>(); });
            _target = new RacePariRequestConverter(config.CreateMapper());
        }

        [TestMethod]
        public void Convert_WithMapping_ShouldWork()
        {
            var request = new RacePariRequest()
            {
                GameId = 123,
                CreditsPlayed = 10,
                LinesPlayed = 40
            };

            // Turn the message into bytes.
            var byteData = _target.Convert(request);
            Assert.AreEqual(12, byteData.Length);
            Assert.AreEqual(123, byteData[0]);
            Assert.AreEqual(10, byteData[4]);
            Assert.AreEqual(40, byteData[8]);
            Assert.AreEqual(0, byteData[11]);

            var data = _target.Convert(request);
            var mappedData = MessageUtility.ConvertByteArrayToMessage<GMessageRacePariRequest>(data);
            Assert.AreEqual(mappedData.GameId, request.GameId);
            Assert.AreEqual(mappedData.CreditsPlayed, request.CreditsPlayed);
            Assert.AreEqual(mappedData.LinesPlayed, request.LinesPlayed);
        }
    }
}