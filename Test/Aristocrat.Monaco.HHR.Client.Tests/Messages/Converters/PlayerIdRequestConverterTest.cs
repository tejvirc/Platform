namespace Aristocrat.Monaco.Hhr.Client.Tests.Messages.Converters
{
    using Client.Messages;
    using Client.Messages.Converters;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class PlayerIdRequestConverterTest
    {
        private PlayerIdRequestConverter _target;

        [TestInitialize]
        public void Initialize()
        {
            _target = new PlayerIdRequestConverter();
        }

        [TestMethod]
        public void Convert_WithMapping_ShouldWork()
        {
            var request = new PlayerIdRequest();

            // Turn the message into bytes.
            var byteData = _target.Convert(request);
            Assert.AreEqual(0, byteData.Length);

            // There's nothing to actually test on this message. An exception will be thrown if conversion fails. We
            // can't try to deserialise either, because the marshalling won't work.
        }
    }
}