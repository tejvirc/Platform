namespace Aristocrat.Monaco.Hhr.Client.Tests.Messages.Converters
{
    using System;
    using Data;
    using Client.Messages;
    using Client.Messages.Converters;
    using Mappings;
    using AutoMapper;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GamePlayRequestConverterTests
    {
        private GamePlayRequestConverter _target;

        [TestInitialize]
        public void Initialize()
        {
            var config = new MapperConfiguration(cfg => { cfg.AddProfile<RequestProfile>(); });
            _target = new GamePlayRequestConverter(config.CreateMapper());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_WithNullMapper_ExpectException()
        {
            _ = new GamePlayRequestConverter(null);
        }

        [TestMethod]
        public void CheckConverter_AskMapperToMap_ExpectSuccessMapping()
        {
            var request = new GamePlayRequest
            {
                CreditsPlayed = 40,
                GameId = 1
            };

            var data = _target.Convert(request);
            var mappedData = MessageUtility.ConvertByteArrayToMessage<GMessageGamePlay>(data);

            Assert.AreEqual(request.CreditsPlayed, mappedData.CreditsPlayed);
            Assert.AreEqual(request.GameId, mappedData.GameId);
        }
    }
}
