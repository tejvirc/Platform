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
    public class ProgressiveInfoResponseConverterTests
    {
        private ProgressiveInfoResponseConverter _target;

        [TestInitialize]
        public void Initialize()
        {
            var config = new MapperConfiguration(cfg => { cfg.AddProfile<ResponseProfile>(); });
            _target = new ProgressiveInfoResponseConverter(config.CreateMapper());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_WithNullMapper_ExpectException()
        {
            _ = new ProgressiveInfoResponseConverter(null);
        }

        [TestMethod]
        public void CheckConverter_AskMapperToMap_ExpectSuccessMapping()
        {
            var response = new SMessageProgressiveInfo
            {
                ProgressiveId  = 1,
                ProgLevel = 1,
                ProgContribPercent = 80,
                ProgCurrentValue = 1001,
                ProgReservePercent = 30,
                ProgResetValue = 1000,
                ProgMaximum = 1500,
                ProgCreditsBet = 40
            };

            var data = MessageUtility.ConvertMessageToByteArray<SMessageProgressiveInfo>(response);
            var mappedData = _target.Convert(data);

            Assert.AreEqual(response.ProgressiveId, mappedData.ProgressiveId);
            Assert.AreEqual(response.ProgLevel, mappedData.ProgLevel);
            Assert.AreEqual(response.ProgContribPercent, mappedData.ProgContribPercent);
            Assert.AreEqual(response.ProgCurrentValue, mappedData.ProgCurrentValue);
            Assert.AreEqual(response.ProgResetValue, mappedData.ProgResetValue);
            Assert.AreEqual(response.ProgCreditsBet, mappedData.ProgCreditsBet);
        }
    }
}
