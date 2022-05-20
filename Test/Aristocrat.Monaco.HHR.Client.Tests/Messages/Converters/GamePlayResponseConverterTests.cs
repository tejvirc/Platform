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
    public class GamePlayResponseConverterTests
    {
        private GamePlayResponseConverter _target;

        [TestInitialize]
        public void Initialize()
        {
            var config = new MapperConfiguration(cfg => { cfg.AddProfile<ResponseProfile>(); });
            _target = new GamePlayResponseConverter(config.CreateMapper());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_WithNullMapper_ExpectException()
        {
            _ = new GamePlayResponseConverter(null);
        }

        [TestMethod]
        public void CheckConverter_AskMapperToMap_ExpectSuccessMapping()
        {
            var response = new SMessageGameBonanza
            {
                GameId = 1,
                BOverride = 1,
            };

            var data = MessageUtility.ConvertMessageToByteArray<SMessageGameBonanza>(response);
            var mappedData = _target.Convert(data);

            Assert.AreEqual(response.GameId, mappedData.GameId);
            Assert.AreEqual(response.BOverride, (byte)(mappedData.BOverride ? 1u : 0u));
            Assert.AreEqual(response.BOverride == 1, mappedData.BOverride);
        }
    }
}
