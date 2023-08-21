namespace Aristocrat.Monaco.Hhr.Client.Tests.Messages.Converters
{
    using System;
    using Data;
    using Client.Messages;
    using Client.Messages.Converters;
    using AutoMapper;
    using Mappings;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GameProgressiveUpdateResponseConverterTests
    {
        private GameProgressiveUpdateResponseConverter _target;

        [TestInitialize]
        public void Initialize()
        {
            var config = new MapperConfiguration(cfg => { cfg.AddProfile<ResponseProfile>(); });
            _target = new GameProgressiveUpdateResponseConverter(config.CreateMapper());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_WithNullMapper_ExpectException()
        {
            _ = new GameProgressiveUpdateResponseConverter(null);
        }

        [TestMethod]
        public void CheckConverter_AskMapperToMap_ExpectSuccessMapping()
        {
            var response = new SMessageProgressivePrize
            {
                Id = 1,
                Amount = 1000,
                Status = 1,
                CreditsBet = 40
            };

            var data = MessageUtility.ConvertMessageToByteArray<SMessageProgressivePrize>(response);
            var mappedData = _target.Convert(data);

            Assert.AreEqual(response.Id, mappedData.Id);
            Assert.AreEqual(response.Amount, mappedData.Amount);
            Assert.AreEqual(response.CreditsBet, mappedData.CreditsBet);
            Assert.AreEqual(response.Status, mappedData.Status);
        }
    }
}
