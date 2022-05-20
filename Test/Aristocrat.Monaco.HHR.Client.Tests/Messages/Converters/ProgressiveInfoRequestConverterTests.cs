
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
    public class ProgressiveInfoRequestConverterTests
    {
        private ProgressiveInfoRequestConverter _target;

        [TestInitialize]
        public void Initialize()
        {
            var config = new MapperConfiguration(cfg => { cfg.AddProfile<RequestProfile>(); });
            _target = new ProgressiveInfoRequestConverter(config.CreateMapper());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_WithNullMapper_ExpectException()
        {
            _ = new ProgressiveInfoRequestConverter(null);
        }

        [TestMethod]
        public void CheckConverter_AskMapperToMap_ExpectSuccessMapping()
        {
            var request = new ProgressiveInfoRequest
            {
                ProgressiveId = 1
            };

            var data = _target.Convert(request);

            var mappedData = MessageUtility.ConvertByteArrayToMessage<GMessageProgRequest>(data);

            Assert.AreEqual(request.ProgressiveId, mappedData.ProgressiveId);
        }
    }
}
