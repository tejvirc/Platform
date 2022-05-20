namespace Aristocrat.Monaco.Hhr.Client.Tests.Messages.Converters
{
    using System.Runtime.InteropServices;
    using Data;
    using Client.Messages;
    using Client.Messages.Converters;
    using Mappings;
    using AutoMapper;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RacePariResponseConverterTest
    {
        private RacePariResponseConverter _target;

        [TestInitialize]
        public void Initialize()
        {
            var config = new MapperConfiguration(cfg => { cfg.AddProfile<ResponseProfile>(); });
            _target = new RacePariResponseConverter(config.CreateMapper());
        }

        [TestMethod]
        public void Convert_WithMapping_ShouldWork()
        {
            var response = new GMessageRacePariResponse
            {
                Data = new CTemplatePool[25]
            };

            for (int x = 0; x < 25; x++)
            {
                response.Data[x].PrizeDataRace = new int[3000];
            }

            response.Data[0].LastPariPrize1 = 123;
            response.Data[1].LastPariPrize1 = 234;
            response.Data[1].PrizeDataRace[7] = 456;
            response.Data[13].PrizeDataRace[17] = 789;
            response.Data[24].MathPoolGuaranteedBalance = -1;

            // Turn the message into bytes.
            var byteData = MessageUtility.ConvertMessageToByteArray(response);
            Assert.AreEqual(Marshal.SizeOf(typeof(GMessageRacePariResponse)), byteData.Length);
            Assert.AreEqual(123, byteData[0]);
            Assert.AreEqual(234, byteData[12040]); // Size of a CTemplatePool
            Assert.AreEqual(200, byteData[12092]);
            Assert.AreEqual(21,  byteData[156612]);
            Assert.AreEqual(255, byteData[300999]);

            var mappedResponse = _target.Convert(byteData);
            Assert.AreEqual(mappedResponse.TemplatePool[0].LastPariPrize1, response.Data[0].LastPariPrize1);
            Assert.AreEqual(mappedResponse.TemplatePool[1].LastPariPrize1, response.Data[1].LastPariPrize1);
            Assert.AreEqual(mappedResponse.TemplatePool[24].LastPariPrize1, response.Data[24].LastPariPrize1);

            Assert.AreEqual(mappedResponse.TemplatePool[1].PrizeDataRace[7], response.Data[1].PrizeDataRace[7]);
            Assert.AreEqual(mappedResponse.TemplatePool[13].PrizeDataRace[17], response.Data[13].PrizeDataRace[17]);
            Assert.AreEqual(mappedResponse.TemplatePool[23].PrizeDataRace[2900], response.Data[23].PrizeDataRace[2900]);

            Assert.AreEqual(mappedResponse.TemplatePool[24].MathPoolGuaranteedBalance, response.Data[24].MathPoolGuaranteedBalance);
            Assert.AreEqual(mappedResponse.TemplatePool[7].MathPoolGuaranteedBalance, response.Data[7].MathPoolGuaranteedBalance);
        }
    }
}
