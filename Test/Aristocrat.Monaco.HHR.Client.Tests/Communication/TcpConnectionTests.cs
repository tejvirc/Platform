using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Aristocrat.Monaco.Hhr.Client.Communication;
using Aristocrat.Monaco.Protocol.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aristocrat.Monaco.Hhr.Client.Tests.Communication
{
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using Client.Messages;
    using Data;

    public class ConnectionObserver : IObserver<ConnectionStatus>, IObserver<Packet>
    {
        public ConnectionStatus ConnectionStatus;
        public Packet LastTcpPacket;

        public ConnectionObserver(ITcpConnection connection)
        {
            connection.ConnectionStatus.Subscribe(this);
            connection.IncomingBytes.Subscribe(this);
        }

        public void Clear()
        {
            ConnectionStatus = null;
            LastTcpPacket = null;
        }

        public void OnNext(ConnectionStatus value)
        {
            ConnectionStatus = value;
        }

        public void OnNext(Packet value)
        {
            LastTcpPacket = value;
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }
    }

    [TestClass]
    public class TcpConnectionTests
    {
        private const int sleepTimeout = 1000;
        private const int TestPort = 987;
        private static readonly byte[] testBytes = new byte[] { 1, 2, 3 };
        private static byte[] testBuffer = new byte[] { 0, 0, 0, 0, 0 };

        private TcpConnection _tcpConnection;
        private ConnectionObserver _connectionObserver;

        private void CreateTransportManager()
        {
            _tcpConnection = new TcpConnection();
            _connectionObserver = new ConnectionObserver(_tcpConnection);
        }

        private TcpListener serverSocket;
        private TcpClient serverClient;
        private void OpenServerSocket()
        {
            serverSocket = new TcpListener(new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), TestPort));
            serverSocket.Start();
            serverSocket.BeginAcceptTcpClient(
                ar =>
                {
                    serverClient = serverSocket.EndAcceptTcpClient(ar);
                }, null);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _tcpConnection?.Close();
            _connectionObserver.Clear();
            serverSocket?.Stop();
            serverClient?.Close();
            testBuffer = new byte[] { 0, 0, 0, 0, 0 };
        }

        [TestMethod]
        public void Open_WithNoServer_ShouldFail()
        {
            CreateTransportManager();
            bool result = _tcpConnection.Open(new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), TestPort)).Result;
            Assert.IsFalse(result);
            Assert.AreEqual(ConnectionState.Disconnected, _connectionObserver.ConnectionStatus.ConnectionState);
        }

        [TestMethod]
        public void Send_WithNoConnection_ShouldFail()
        {
            CreateTransportManager();
            bool result = _tcpConnection.SendBytes(testBytes).Result;
            Assert.IsFalse(result);
            Assert.AreEqual(ConnectionState.Disconnected, _connectionObserver.ConnectionStatus.ConnectionState);
        }

        [TestMethod]
        public void Open_WithServer_ShouldSucceed()
        {
            OpenServerSocket();
            CreateTransportManager();
            bool result = _tcpConnection.Open(new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), TestPort)).Result;
            Assert.IsTrue(result);
            Assert.AreEqual(ConnectionState.Connected, _connectionObserver.ConnectionStatus.ConnectionState);
        }

        [TestMethod]
        public void Wait_WithServerClosing_ShouldDisconnect()
        {
            OpenServerSocket();
            CreateTransportManager();
            bool result = _tcpConnection.Open(new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), TestPort)).Result;
            Assert.IsTrue(result);
            Thread.Sleep(sleepTimeout);
            Assert.AreEqual(ConnectionState.Connected, _connectionObserver.ConnectionStatus.ConnectionState);
            serverClient.Close();
            Thread.Sleep(sleepTimeout);
            Assert.AreEqual(ConnectionState.Disconnected, _connectionObserver.ConnectionStatus.ConnectionState);
        }

        [TestMethod]
        public async Task Send_WithServerReceiving_ShouldBeSent()
        {
            OpenServerSocket();
            CreateTransportManager();
            bool result = await _tcpConnection.Open(new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), TestPort));
            Assert.IsTrue(result);
            result = _tcpConnection.SendBytes(testBytes).Result;
            Assert.IsTrue(result);
            Thread.Sleep(sleepTimeout);
            int bytesRead = serverClient.GetStream().Read(testBuffer, 0, 5);
            Assert.AreEqual(3, bytesRead);
            Assert.AreEqual(1, testBuffer[0]);
            Assert.IsNull(_connectionObserver.LastTcpPacket);
        }

        [TestMethod]
        public async Task Wait_WithServerSending_ShouldBeReceived()
        {
            OpenServerSocket();
            CreateTransportManager();
            bool result = await _tcpConnection.Open(new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), TestPort));
            Assert.IsTrue(result);
            Thread.Sleep(sleepTimeout);
            var encryptedHeader = new MessageEncryptHeader()
            {
                EncryptionLength = Marshal.SizeOf<MessageEncryptHeader>()
            };
            var data = MessageUtility.ConvertMessageToByteArray(encryptedHeader);
            serverClient.GetStream().Write(data, 0, data.Length);
            Thread.Sleep(sleepTimeout);
            
            Assert.AreEqual(data.Length, _connectionObserver.LastTcpPacket.Length);
        }
   }
}
