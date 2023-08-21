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
    public class ParameterResponseConverterTest
    {
	    private ParameterResponseConverter _target;

	    [TestInitialize]
	    public void Initialize()
	    {
		    var config = new MapperConfiguration(cfg => { cfg.AddProfile<ResponseProfile>(); });
		    _target = new ParameterResponseConverter(config.CreateMapper());
	    }

	    [TestMethod]
	    [ExpectedException(typeof(ArgumentNullException))]
	    public void ConstructWithNullMapperExpectException()
	    {
		    _ = new ParameterResponseConverter(null);
	    }

	    [TestMethod]
	    public void AskMapperToMapAndExpectSuccessMapping()
	    {
		    var response = new SMessageGtParameter()
		    {
			    GameIdCount = 1
		    };

		    var bytes = MessageUtility.ConvertMessageToByteArray(response);
		    var mappedResponse = _target.Convert(bytes);
		    Assert.AreEqual(mappedResponse.GameIdCount, response.GameIdCount);
	    }
    }
}
