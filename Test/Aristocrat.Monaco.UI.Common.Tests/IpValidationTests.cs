namespace Aristocrat.Monaco.UI.Common.Tests
{
    #region Using

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    #endregion

    [TestClass]
    public class IpValidationTests
    {
        [TestMethod]
        public void ShouldPassHappyPathForIsIpV4AddressValid()
        {
            var ipAddess = "192.168.1.1";
            Assert.IsTrue(IsIpV4AddressValid(ipAddess));
        }

        [TestMethod]
        public void ShouldPassHappyPathForIsIpV4SubnetMaskValid()
        {
            var ipAddess = "255.255.255.255";
            Assert.IsTrue(IsIpV4SubnetMaskValid(ipAddess));
        }

        [TestMethod]
        public void ShouldFailForIsIpV4AddressValidBecauseFirstOctet()
        {
            var ipAddess = "255.255.255.255";
            Assert.IsFalse(IsIpV4AddressValid(ipAddess));
        }

        [TestMethod]
        public void ShouldFailForIsIpV4SubnetMaskValidBecauseNotSubnetMask()
        {
            var ipAddess = "192.168.1.1";
            Assert.IsFalse(IsIpV4SubnetMaskValid(ipAddess));
        }

        [TestMethod]
        public void ShouldFailWithSpecialCharacter()
        {
            var ipAddess = "$192.168.1.188";
            Assert.IsFalse(IsIpV4AddressValid(ipAddess));
            Assert.IsFalse(IsIpV4SubnetMaskValid(ipAddess));
        }

        [TestMethod]
        public void ShouldFailWithSpecialCharacter2()
        {
            var ipAddess = "192.1$68.1.188";
            Assert.IsFalse(IsIpV4AddressValid(ipAddess));
            Assert.IsFalse(IsIpV4SubnetMaskValid(ipAddess));
        }

        [TestMethod]
        public void ShouldFailBecauseReserved()
        {
            var ipAddess = "100.1.2.3";
            Assert.IsFalse(IsIpV4AddressValid(ipAddess));
            Assert.IsFalse(IsIpV4SubnetMaskValid(ipAddess));
        }

        [TestMethod]
        public void ShouldFailBecauseReserved2()
        {
            var ipAddess = "192.88.99.0";
            Assert.IsFalse(IsIpV4AddressValid(ipAddess));
            Assert.IsFalse(IsIpV4SubnetMaskValid(ipAddess));
        }

        [TestMethod]
        public void ShouldFailBecauseNotEnoughOctets()
        {
            var ipAddess = "192.88.99";
            Assert.IsFalse(IsIpV4AddressValid(ipAddess));
            Assert.IsFalse(IsIpV4SubnetMaskValid(ipAddess));
        }

        [TestMethod]
        public void ShouldFailBecauseInExcludeList()
        {
            var ipAddess = "0.0.0.0";
            Assert.IsFalse(IsIpV4AddressValid(ipAddess));
            Assert.IsFalse(IsIpV4SubnetMaskValid(ipAddess));
        }

        [TestMethod]
        public void ShouldFailForLeadingZeros()
        {
            var ipAddess = "092.168.1.188";
            Assert.IsFalse(IsIpV4AddressValid(ipAddess));
            Assert.IsFalse(IsIpV4SubnetMaskValid(ipAddess));
        }

        [TestMethod]
        public void ShouldFailForLeadingZeros2()
        {
            var ipAddess = "092.068.1.188";
            Assert.IsFalse(IsIpV4AddressValid(ipAddess));
            Assert.IsFalse(IsIpV4SubnetMaskValid(ipAddess));
        }

        [TestMethod]
        public void ShouldFailWithSpecialCharacterAndSpace()
        {
            var ipAddess = "$ 192.1$68.1.188";
            Assert.IsFalse(IsIpV4AddressValid(ipAddess));
            Assert.IsFalse(IsIpV4SubnetMaskValid(ipAddess));
        }

        private bool IsIpV4AddressValid(string ipAddress)
        {
            return IpValidation.IsIpV4AddressValid(ipAddress);
        }

        private bool IsIpV4SubnetMaskValid(string ipAddress)
        {
            return IpValidation.IsIpV4SubnetMaskValid(ipAddress);
        }
    }
}