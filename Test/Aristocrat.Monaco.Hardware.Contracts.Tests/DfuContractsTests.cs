namespace Aristocrat.Monaco.Hardware.Tests.ContractsTests
{
    using Contracts.Dfu;
    using Contracts.SharedDevice;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     This file contains the tests for DfuContracts
    /// </summary>
    [TestClass]
    public class DfuContractsTests
    {
        [TestMethod]
        public void DfuDownloadProgressEventTest()
        {
            // constructor with progress test
            var progress = 50;
            var target = new DfuDownloadProgressEvent(progress);
            Assert.IsNotNull(target);
            Assert.AreEqual(50, target.Progress);
        }

        [TestMethod]
        public void DfuErrorEventTest()
        {
            var target = new DfuErrorEvent();
            Assert.IsNotNull(target);
            Assert.AreEqual(DfuErrorEventId.None, target.Id);

            // constructor with error id
            var id = DfuErrorEventId.DfuErrorAddress;
            var error = 123;
            target = new DfuErrorEvent(id, error);
            Assert.IsNotNull(target);
            Assert.AreEqual(id, target.Id);
            Assert.AreEqual(error, target.VendorSpecificErrorIndex);
        }
    }
}