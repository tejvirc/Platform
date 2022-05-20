namespace Aristocrat.Monaco.G2S.Common.Tests.Transfer
{
    using Common.Transfer;
    using Common.Transfer.Ftp;
    using Common.Transfer.Http;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Exceptions;
    using Moq;
    using Test.Common;

    [TestClass]
    public class TransferFactoryTests
    {
        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
        }

        [TestMethod]
        public void GetTransferService_ShouldReturnCorrectTransfer_WhenFindIt()
        {
            var transferFactory = new TransferFactory();

            var transferService = transferFactory.GetTransferService(@"https://user:pass@34.221.189.61:443/HOME/user/ATI_Test.zip");
            Assert.AreEqual(true, transferService is HttpTransferService);
            
            transferService = transferFactory.GetTransferService(@"http://user:pass@34.221.189.61:443/HOME/user/ATI_Test.zip");
            Assert.AreEqual(true, transferService is HttpTransferService);

            transferService = transferFactory.GetTransferService(@"ftp://anonymous:@34.221.189.61:21/public/ATI_Test.zip");
            Assert.AreEqual(true, transferService is FtpTransferService);

            transferService = transferFactory.GetTransferService(@"ftps://user:pass@34.221.189.61:990/HOME/user/ATI_Test.zip");
            Assert.AreEqual(true, transferService is FtpTransferService);

            transferService = transferFactory.GetTransferService(@"sftp://user:pass@34.221.189.61:990/HOME/user/ATI_Test.zip");
            Assert.AreEqual(true, transferService is FtpTransferService);
        }


        [TestMethod]
        [ExpectedException(typeof(CommandException))]
        public void GetTransferService_ShouldThrowException_WhenCannotFindIt()
        {
            var transferFactory = new TransferFactory();

            transferFactory.GetTransferService(@"htps://user:pass@34.221.189.61:443/HOME/user/ATI_Test.zip");
        }

    }
}
