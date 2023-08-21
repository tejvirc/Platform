namespace Aristocrat.Monaco.G2S.Common.Tests.Transfer.Ftp
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Common.Transfer.Ftp;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    ///     Unit tests for <c>FtpTransferService</c>.
    /// </summary>
    [TestClass]
    public class FtpTransferServiceTests
    {
        /// <summary>
        ///     Ensures that there is three retries at least.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void TestRetrySuccess()
        {
            //There should be three attemps to download file and the last without error.
            TestRetry(2);
        }

        /// <summary>
        ///     Mocks FTP client to simulate specified number of connection fails.
        /// </summary>
        /// <param name="totalCountOfFails">Number of connection fails.</param>
        /// <returns></returns>
        //private Int32 TestRetry(Int32 totalCountOfFails)
        private static void TestRetry(int totalCountOfFails)
        {
            var count = 0;
            var clientMock = new Mock<IFtpClient>();
            var transferMock = new Mock<FtpTransferService>();

            clientMock.Setup(x => x.Connect(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(SuccessWaitAsync)
                .Callback(
                    (string location, string parameters) =>
                    {
                        count++;
                        if (count <= totalCountOfFails)
                        {
                            throw new ApplicationException();
                        }
                    });

            clientMock.Setup(x => x.Download(It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Returns(SuccessWaitAsync);
            transferMock.Setup(x => x.GetFtpClient(It.IsAny<string>())).Returns(clientMock.Object);

            transferMock.Object.Download(
                string.Empty,
                string.Empty,
                null,
                CancellationToken.None);
        }

        private static async Task<bool> SuccessWaitAsync()
        {
            return await Task.FromResult(true);
        }
    }
}