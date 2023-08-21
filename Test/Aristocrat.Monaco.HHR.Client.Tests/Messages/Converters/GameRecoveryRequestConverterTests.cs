using System;
using Aristocrat.Monaco.Hhr.Client.Data;
using Aristocrat.Monaco.Hhr.Client.Mappings;
using Aristocrat.Monaco.Hhr.Client.Messages;
using Aristocrat.Monaco.Hhr.Client.Messages.Converters;
using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aristocrat.Monaco.Hhr.Client.Tests.Messages.Converters
{
	[TestClass]
	public class GameRecoveryRequestConverterTests
	{
		private GameRecoveryRequestConverter _target;

		[TestInitialize]
		public void Initialize()
		{
			var config = new MapperConfiguration(cfg => { cfg.AddProfile<RequestProfile>(); });
			_target = new GameRecoveryRequestConverter(config.CreateMapper());
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Ctor_WithNullMapper_ExpectException()
		{
			_ = new GameRecoveryRequestConverter(null);
		}

		[TestMethod]
		public void CheckConverter_AskMapperToMap_ExpectSuccessMapping()
		{
			var request = new GameRecoveryRequest
			{
				GameNo = 1
			};

			var data = _target.Convert(request);
			var mappedData = MessageUtility.ConvertByteArrayToMessage<GMessageGameRecover>(data);

			Assert.AreEqual(request.GameNo, mappedData.GameNo);
		}
	}
}