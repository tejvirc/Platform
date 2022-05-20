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
	public class ParameterRequestConverterTest
	{
		private ParameterRequestConverter _target;

		[TestInitialize]
		public void Initialize()
		{
			var config = new MapperConfiguration(cfg => { cfg.AddProfile<RequestProfile>(); });
			_target = new ParameterRequestConverter(config.CreateMapper());
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ConstructWithNullMapperExpectException()
		{
			_ = new ParameterRequestConverter(null);
		}

		[TestMethod]
		public void AskMapperToMapAndExpectSuccessMapping()
		{
			var request = new ParameterRequest
			{
				SerialNumber = "123"
			};

			var data = _target.Convert(request);
			var mappedData = MessageUtility.ConvertByteArrayToMessage<MessageParameterRequest>(data);
			Assert.AreEqual(mappedData.SerialNumber, request.SerialNumber);
			Assert.AreEqual(mappedData.DeviceType, "GT");
		}
	}
}