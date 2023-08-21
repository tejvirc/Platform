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
	public class GameRecoveryResponseConverterTests
	{
		private GameRecoveryResponseConverter _target;

		[TestInitialize]
		public void Initialize()
		{
			var config = new MapperConfiguration(cfg => { cfg.AddProfile<ResponseProfile>(); });
			_target = new GameRecoveryResponseConverter(config.CreateMapper());
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Ctor_WithNullMapper_ExpectException()
		{
			_ = new GameRecoveryResponseConverter(null);
		}

		[TestMethod]
		public void CheckConverter_AskMapperToMap_ExpectSuccessMapping()
		{
			var response = new GMessageGameRecoverResponse
			{
				PrizeLoc1 = 10
			};

			var mappedData = MessageUtility.ConvertMessageToByteArray(response);

			var data = _target.Convert(mappedData);

			Assert.AreEqual(data.PrizeLoc1, response.PrizeLoc1);
		}
	}
}