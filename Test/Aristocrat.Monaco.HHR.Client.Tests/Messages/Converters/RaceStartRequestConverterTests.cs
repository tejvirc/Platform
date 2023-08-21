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
    public class RaceStartRequestConverterTests
    {
        private RaceStartRequestConverter _target;

        [TestInitialize]
        public void Initialize()
        {
            var config = new MapperConfiguration(cfg => { cfg.AddProfile<RequestProfile>(); });
            _target = new RaceStartRequestConverter(config.CreateMapper());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_WithNullMapper_ExpectException()
        {
            _ = new RaceStartRequestConverter(null);
        }

        [TestMethod]
        public void CheckConverter_AskMapperToMap_ExpectSuccessMapping()
        {
            var request = new RaceStartRequest
            {
                GameId = 1,
                CreditsPlayed = 40,
            };

            var data = _target.Convert(request);
            var mappedData = MessageUtility.ConvertByteArrayToMessage<GMessageRaceStart>(data);

            Assert.AreEqual(request.CreditsPlayed, mappedData.CreditsPlayed);
            Assert.AreEqual(request.GameId, mappedData.GameId);
        }
    }
}
