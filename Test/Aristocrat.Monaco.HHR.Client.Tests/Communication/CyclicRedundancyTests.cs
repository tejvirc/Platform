using Aristocrat.Monaco.Hhr.Client.Communication;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aristocrat.Monaco.Hhr.Client.Tests.Communication
{
    [TestClass]
    public class CyclicRedundancyTests
    {
        [DataTestMethod]
        [DataRow(new byte[] { 1, 2, 3, 4, 5 },                           60827)]
        [DataRow(new byte[] { 5, 4, 3, 2, 1 },                           39397)]
        [DataRow(new byte[] { 0, 0, 0, 0, 0 },                           00000)]
        [DataRow(new byte[] { 0, 0, 0, 0, 1 },                           04489)]
        [DataRow(new byte[] { 243, 87, 97, 0, 21, 68, 183, 3, 16, 222 }, 37398)]
        public void CalculateValue_ShouldSucceed(byte[] inputBytes, int outputValue)
        {
            CrcProvider crc = new CrcProvider();
            ushort output = crc.Calculate(inputBytes);
            Assert.AreEqual((ushort) outputValue, output);
        }
    }
}
