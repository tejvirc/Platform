using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Aristocrat.Monaco.Protocol.Common;
using Aristocrat.Monaco.Protocol.Common.Communication;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aristocrat.Monaco.Hhr.Client.Tests.Communication
{
    [TestClass]
    public class UdpConnectionTests : IObserver<Packet>
    {
        private readonly ManualResetEvent _waiter = new ManualResetEvent(false);
        private IUdpConnection _udpConnection;
        private Packet _lastReceivedPacket;

        private IPEndPoint _endPoint;
        private SocketAsyncEventArgs _sendEventArg;
        public Socket _sendSocket;

        [TestInitialize]
        public void Initialize()
        {
            CreateBroadcastManager();
        }

        [TestCleanup]
        public async Task TestCleanupAsync()
        {
            await _udpConnection.Close();

            _sendSocket?.Close();
            _sendSocket?.Dispose();

            _lastReceivedPacket = null;
        }

        [Ignore]
        [TestMethod]
        public async Task TestConnectionAsync()
        {
            bool result = await _udpConnection.Open(_endPoint);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task TestConnectionFailureAsync()
        {
            // Not a class D (multicast) address (or broadcast).
            var endpoint = new IPEndPoint(new IPAddress(new byte[] { 55, 255, 255, 255 }), 65432);
            bool result = await _udpConnection.Open(endpoint);

            Assert.IsFalse(result);
        }

        [Ignore]
        [TestMethod]
        public async Task TestDisconnectionAsync()
        {
            bool result = await _udpConnection.Open(_endPoint);
            Assert.IsTrue(result);
            await _udpConnection.Close();
        }

        [Ignore]
        [TestMethod]
        public async Task TestConnectionReceiveAsync()
        {
            bool result = await _udpConnection.Open(_endPoint);
            Assert.IsTrue(result);

            SetupSocketSend();
            SendAsync();

            _waiter.WaitOne();

            Assert.IsNotNull(_lastReceivedPacket);
            var expectedPayload = GetTestPayload();
            Assert.AreEqual(expectedPayload.Length, _lastReceivedPacket.Length);
            Assert.IsTrue(Enumerable.SequenceEqual(
                expectedPayload.Data.Take(expectedPayload.Length),
                _lastReceivedPacket.Data.Take(_lastReceivedPacket.Length)));
        }

        private void SetupSocketSend()
        {
            _sendSocket = new Socket(_endPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            _sendSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.IpTimeToLive, true);
            _sendSocket.SetSocketOption(
                SocketOptionLevel.Socket,
                SocketOptionName.ExclusiveAddressUse, false);

            _sendEventArg = new SocketAsyncEventArgs();
            _sendEventArg.Completed += OnSend;
            _sendEventArg.RemoteEndPoint = _endPoint;
            var msg = GetTestPayload().Data;
            _sendEventArg.SetBuffer(msg, 0, msg.Length);
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(Packet value)
        {
            _lastReceivedPacket = value;
            _waiter.Set();
        }

        private void CreateBroadcastManager()
        {
            _endPoint = new IPEndPoint(new IPAddress(new byte[] { 224, 1, 1, 1 }), 65432);
            _udpConnection = new UdpConnection();
            _udpConnection.IncomingBytes.Subscribe(this);
        }

        private void SendAsync()
        {
            if (!_sendSocket.SendToAsync(_sendEventArg))
            {
                OnSend(null, _sendEventArg);
            }
        }

        private void OnSend(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                return;
            }

            _ = e.BytesTransferred;
        }

        private Packet GetTestPayload()
        {
            return new Packet(new byte[] { 1, 2, 3 }, 3);
        }
    }
}
