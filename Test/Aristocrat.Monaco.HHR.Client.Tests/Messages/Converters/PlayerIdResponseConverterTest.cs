namespace Aristocrat.Monaco.Hhr.Client.Tests.Messages.Converters
{
    using Data;
    using Client.Messages;
    using Client.Messages.Converters;
    using Mappings;
    using AutoMapper;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class PlayerIdResponseConverterTest
    {
        private PlayerIdResponseConverter _target;

        [TestInitialize]
        public void Initialize()
        {
            var config = new MapperConfiguration(cfg => { cfg.AddProfile<ResponseProfile>(); });
            _target = new PlayerIdResponseConverter(config.CreateMapper());
        }

        [TestMethod]
        public void Convert_WithMapping_ShouldWork()
        {
            var response = new SMessagePlayerRequestResponse()
            {
                PlayerId = "HELLO!"
            };

            // Turn the message into bytes.
            var byteData = MessageUtility.ConvertMessageToByteArray(response);
            Assert.AreEqual(MessageLengthConstants.PlayerIdLength, byteData.Length);
            Assert.AreEqual(72, byteData[0]);
            Assert.AreEqual(33, byteData[5]);
            Assert.AreEqual(0, byteData[6]);

            var mappedResponse = _target.Convert(byteData);
            Assert.AreEqual(mappedResponse.PlayerId, response.PlayerId);
        }
    }
}
