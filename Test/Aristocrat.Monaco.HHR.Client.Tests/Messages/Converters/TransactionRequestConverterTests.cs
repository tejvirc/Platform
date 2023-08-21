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
    public class TransactionRequestConverterTests
    {
        private TransactionRequestConverter _target;

        [TestInitialize]
        public void Initialize()
        {
            var config = new MapperConfiguration(cfg => { cfg.AddProfile<RequestProfile>(); });
            _target = new TransactionRequestConverter(config.CreateMapper());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_WithNullMapper_ExpectException()
        {
            _ = new TransactionRequestConverter(null);
        }

        [TestMethod]
        public void CheckConverter_AskMapperToMap_ExpectSuccessMapping()
        {
            var request = new TransactionRequest
            {
                TransactionType = CommandTransactionType.AftInCashable,
                Credit = 10
            };

            var data = _target.Convert(request);
            var mappedData = MessageUtility.ConvertByteArrayToMessage<MessageTransaction>(data);

            Assert.AreEqual(request.Credit, mappedData.Credit);
            Assert.AreEqual(request.TransactionType, (CommandTransactionType) mappedData.TransType);
        }
    }
}
