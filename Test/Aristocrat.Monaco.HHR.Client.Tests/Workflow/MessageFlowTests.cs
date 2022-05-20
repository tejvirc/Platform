using Aristocrat.Monaco.Hhr.Client.Communication;
using Aristocrat.Monaco.Hhr.Client.Data;
using Aristocrat.Monaco.Hhr.Client.Messages;
using Aristocrat.Monaco.Hhr.Client.WorkFlow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Aristocrat.Monaco.Hhr.Client.Tests.Messages;

namespace Aristocrat.Monaco.Hhr.Client.Tests.WorkFlow
{
    [TestClass]
    public class MessageFlowTest
    {
        Mock<ICrcProvider> _crcProvider;
        Mock<ICryptoProvider> _cryptoProvider;
        Mock<IMessageFactory> _messageFactory;
        Mock<ITcpConnection> _tcpConnection;
        readonly ushort _crc = 10;
        readonly byte[] _bytes = new byte[1024];
        readonly Response _deserializedResponse = new InvalidResponse() { MyProperty = 10 };
        Request _request;
        MessageFlow _target;

        [TestInitialize]
        public void Initialization()
        {
            var random = new Random();
            random.NextBytes(_bytes);
            var header = MessageUtility.GetMessage<MessageHeader>(_bytes);
            header.Command = (uint)Command.CmdInvalidCommand;
            MessageUtility.SetMessage(_bytes, header);
            _crcProvider = new Mock<ICrcProvider>();
            _crcProvider.Setup(x => x.Calculate(It.IsAny<byte[]>())).Returns(_crc);
            _cryptoProvider = new Mock<ICryptoProvider>();
            _cryptoProvider.Setup(x => x.Encrypt(It.IsAny<byte[]>())).Returns(Task.FromResult(MessageUtility.PopulateMessageHeader(new InvalidRequest(), _bytes)));
            _cryptoProvider.Setup(x => x.Decrypt(It.IsAny<byte[]>())).Returns(Task.FromResult(MessageUtility.PopulateMessageHeader(new InvalidRequest(), _bytes)));
            _messageFactory = new Mock<IMessageFactory>();
            _messageFactory.Setup(x => x.Serialize(It.IsAny<Request>())).Returns(_bytes);
            _messageFactory.Setup(x => x.Deserialize(It.IsAny<byte[]>())).Returns(_deserializedResponse);
            _tcpConnection = new Mock<ITcpConnection>();
            _tcpConnection.Setup(x => x.SendBytes(It.IsAny<byte[]>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(true));

            _request = new InvalidRequest();
            _target = new MessageFlow(_messageFactory.Object, _crcProvider.Object, _tcpConnection.Object, _cryptoProvider.Object);
        }

        [DataRow(true, false, false, false, true, DisplayName = "Crc provider throws exception, Expect pipeline to throw exception.")]
        [DataRow(false, true, false, false, true, DisplayName = "CryptoProvider throws exception, Expect pipeline to throw exception.")]
        [DataRow(false, false, true, false, true, DisplayName = "MessageFactory throws exception, expect pipeline to throw exception.")]
        [DataRow(false, false, false, true, true, DisplayName = "TcpConnection throws exception, expect pipeline to throw exception.")]
        [DataRow(true, true, true, true, true, DisplayName = "All components throw exception, expect pipeline to throw exception.")]
        [DataRow(false, false, false, false, false, DisplayName = "All components runs fine, expect pipeline to return TransportManager status.")]
        [DataTestMethod]
        public async Task SenderPipelineComponentsThrowExceptionsExpectException(bool crcProviderThrows, bool cryptoProviderThrows, bool messageFactoryThrows, bool tcpConnectionThrows, bool expectException)
        {
            if (crcProviderThrows)
            {
                _crcProvider.Setup(x => x.Calculate(It.IsAny<byte[]>())).Throws(new ArgumentException());
            }
            if (cryptoProviderThrows)
            {
                _cryptoProvider.Setup(x => x.Encrypt(It.IsAny<byte[]>())).Throws(new ArgumentException());
            }
            if (messageFactoryThrows)
            {
                _messageFactory.Setup(x => x.Serialize(It.IsAny<Request>())).Throws(new ArgumentException());
            }
            if (tcpConnectionThrows)
            {
                _tcpConnection.Setup(x => x.SendBytes(It.IsAny<byte[]>(), It.IsAny<CancellationToken>())).Throws(new ArgumentException());
            }

            var result = _target.Send(_request, new CancellationToken());
            if (expectException)
            {
                await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
                {
                    await result;
                });
            }
            else
            {
                Assert.IsTrue(await result);
                _crcProvider.Verify(x => x.Calculate(It.IsAny<byte[]>()), Times.Once);
                _cryptoProvider.Verify(x => x.Encrypt(It.IsAny<byte[]>()), Times.Once);
                _messageFactory.Verify(x => x.Serialize(It.IsAny<Request>()), Times.Once);
                _tcpConnection.Verify(x => x.SendBytes(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        [TestMethod]
        public async Task SenderPipelineToHandleMultipleMessagesExpectSuccess()
        {
            var result = _target.Send(_request, new CancellationToken());
            Assert.IsTrue(await result);
            _crcProvider.Verify(x => x.Calculate(It.IsAny<byte[]>()), Times.Once);
            _cryptoProvider.Verify(x => x.Encrypt(It.IsAny<byte[]>()), Times.Once);
            _messageFactory.Verify(x => x.Serialize(It.IsAny<Request>()), Times.Once);
            _tcpConnection.Verify(x => x.SendBytes(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()), Times.Once);

            Assert.IsTrue(await _target.Send(_request, new CancellationToken()));
            Assert.IsTrue(await _target.Send(_request, new CancellationToken()));
            Assert.IsTrue(await _target.Send(_request, new CancellationToken()));
            Assert.IsTrue(await _target.Send(_request, new CancellationToken()));
        }

        [TestMethod]
        [Ignore("Test needs refactoring fails in GH action workflow builds")]
        public async Task CancelSenderPipelineDuringProcessingAndExpectException()
        {
            var cts = new CancellationTokenSource();
            _crcProvider.Setup(x => x.Calculate(It.IsAny<byte[]>())).Callback(() => {
                Task.Delay(TimeSpan.FromMilliseconds(150)).Wait();
            });

            cts.CancelAfter(TimeSpan.FromMilliseconds(50));
            var result = _target.Send(_request, cts.Token);
            await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () =>
            {
                await result;
            });
        }

        [DataRow(true, false, false, true, DisplayName = "Crc provider throws exception, Expect pipeline to throw exception.")]
        [DataRow(false, true, false, true, DisplayName = "CryptoProvider throws exception, Expect pipeline to throw exception.")]
        [DataRow(false, false, true, true, DisplayName = "MessageFactory throws exception, expect pipeline to throw exception.")]
        [DataRow(true, true, true, true, DisplayName = "All components throw exception, expect pipeline to throw exception.")]
        [DataRow(false, false, false, false, DisplayName = "All components runs fine, expect pipeline to return TransportManager status.")]
        [DataTestMethod]
        public async Task ReceiverPipelineComponentsThrowExceptionsExpectException(bool crcProviderThrows, bool cryptoProviderThrows, bool messageFactoryThrows, bool expectException)
        {
            if (crcProviderThrows)
            {
                _crcProvider.Setup(x => x.Calculate(It.IsAny<byte[]>())).Throws(new ArgumentException());
            }
            if (cryptoProviderThrows)
            {
                _cryptoProvider.Setup(x => x.Decrypt(It.IsAny<byte[]>())).Throws(new ArgumentException());
            }
            if (messageFactoryThrows)
            {
                _messageFactory.Setup(x => x.Deserialize(It.IsAny<byte[]>())).Throws(new ArgumentException());
            }

            // Setup CRC so that CRC matches
            var data = MessageUtility.ExtractEncryptedHeader(_bytes);
            var header = MessageUtility.GetMessage<MessageEncryptHeader>(_bytes);
            header.Crc = new CrcProvider().Calculate(data);
            MessageUtility.SetMessage(_bytes, header);
            if (!crcProviderThrows)
            {
                _crcProvider.Setup(x => x.Calculate(It.IsAny<byte[]>())).Returns(header.Crc);
            }

            var result = _target.Receive(new Packet(_bytes, _bytes.Length), new CancellationToken());

            if (expectException)
            {
                await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
                {
                    await result;
                });
            }
            else
            {
                Assert.IsInstanceOfType(await result, typeof(InvalidResponse));
                _crcProvider.Verify(x => x.Calculate(It.IsAny<byte[]>()), Times.AtLeastOnce);
                _cryptoProvider.Verify(x => x.Decrypt(It.IsAny<byte[]>()), Times.AtMostOnce);
                _messageFactory.Verify(x => x.Deserialize(It.IsAny<byte[]>()), Times.Once);
            }
        }

        [TestMethod]
        public async Task ReceiverPipelineToProcessMultipleMessages()
        {
            // Setup CRC so that CRC matches
            var data = MessageUtility.ExtractEncryptedHeader(_bytes);
            var header = MessageUtility.GetMessage<MessageEncryptHeader>(_bytes);
            header.Crc = new CrcProvider().Calculate(data);
            MessageUtility.SetMessage(_bytes, header);
            _crcProvider.Setup(x => x.Calculate(It.IsAny<byte[]>())).Returns(header.Crc);

            Assert.IsInstanceOfType(await _target.Receive(new Packet(_bytes, _bytes.Length), new CancellationToken()), typeof(InvalidResponse));
            Assert.IsInstanceOfType(await _target.Receive(new Packet(_bytes, _bytes.Length), new CancellationToken()), typeof(InvalidResponse));
            // This will throw exception
            _crcProvider.Setup(x => x.Calculate(It.IsAny<byte[]>())).Returns(10);

            var result = _target.Receive(new Packet(_bytes, _bytes.Length), default(CancellationToken));
            await Assert.ThrowsExceptionAsync<InvalidDataException>(async () =>
            {
                await result;
            });

            _crcProvider.Setup(x => x.Calculate(It.IsAny<byte[]>())).Returns(header.Crc);
            Assert.IsInstanceOfType(await _target.Receive(new Packet(_bytes, _bytes.Length), new CancellationToken()), typeof(InvalidResponse));
        }

        [TestMethod]
        [Ignore("Test needs refactoring fails in GH action workflow builds")]
        public async Task CancelReceivingPipelineDuringProcessingAndExpectException()
        {
            // Setup CRC so that CRC matches
            var data = MessageUtility.ExtractEncryptedHeader(_bytes);
            var header = MessageUtility.GetMessage<MessageEncryptHeader>(_bytes);
            header.Crc = new CrcProvider().Calculate(data);
            MessageUtility.SetMessage(_bytes, header);
            _crcProvider.Setup(x => x.Calculate(It.IsAny<byte[]>()))
                .Returns(header.Crc)
                .Callback(() => {
                    Task.Delay(TimeSpan.FromMilliseconds(250)).Wait();
                });

            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(50));

            var result = _target.Receive(new Packet(_bytes, _bytes.Length), cts.Token);
            await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () =>
            {
                await result;
            });
        }

        [TestMethod]
        public async Task CancelSenderPipelineBeforeProcessingAndExpectException()
        {
            var cts = new CancellationTokenSource();
            // Start with cancelled token, to simulate task that is cancelled before sending data to pipeline
            cts.Cancel();
            
            var result = _target.Send(_request, cts.Token);
            await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () =>
            {
                await result;
            });
        }

        [TestMethod]
        public async Task CancelReceiverPipelineBeforeProcessingAndExpectException()
        {
            var cts = new CancellationTokenSource();
            // Start with cancelled token, to simulate task that is cancelled before receiving data to pipeline
            cts.Cancel();

            var result = _target.Receive(new Packet(_bytes, _bytes.Length), cts.Token);
            await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () =>
            {
                await result;
            });
        }
    }
}

