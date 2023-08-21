using Aristocrat.Monaco.Hhr.Client.Communication;
using Aristocrat.Monaco.Hhr.Client.Messages;
using Aristocrat.Monaco.Hhr.Client.WorkFlow;
using Aristocrat.Monaco.Hhr.Events;
using Aristocrat.Monaco.Hhr.Services;
using Aristocrat.Monaco.Kernel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Aristocrat.Monaco.Hhr.Tests.Communication
{
    [TestClass]
    public class CommunicationServiceTests
    {
        private const double HeartbeatInterval = 250.0;

        private TcpListener serverTcpSocket;
        private TcpClient serverTcpClient;

        private readonly Mock<ICentralManager> mockManager = new Mock<ICentralManager>(MockBehavior.Strict);
        private readonly Mock<IEventBus> mockEventBus = new Mock<IEventBus>(MockBehavior.Default);
        private readonly Mock<IPropertiesManager> mockProperties = new Mock<IPropertiesManager>(MockBehavior.Default);

        private CommunicationService commService;

        private int onlineTcpEventCount = 0;
        private int offlineTcpEventCount = 0;
        private int heartbeatEventCount = 0;

        private void OpenServerSocket()
        {
            serverTcpSocket.Start();
            serverTcpSocket.BeginAcceptTcpClient(AcceptClient, null);
            Thread.Sleep(100);
        }

        private void AcceptClient(IAsyncResult ar)
        {
            if (serverTcpSocket == null) return;
            serverTcpClient = serverTcpSocket.EndAcceptTcpClient(ar);
            serverTcpSocket.BeginAcceptTcpClient(AcceptClient, null);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            // Set up sockets we can use for accepting connections, and reading and writing if necessary.
            serverTcpSocket = new TcpListener(new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 65431));

            // Set up the service manager and property manager to handle requests for connections.
            mockProperties.Setup(m => m.GetProperty(HHRPropertyNames.ServerTcpIp, It.IsAny<string>())).Returns("127.0.0.1");
            mockProperties.Setup(m => m.GetProperty(HHRPropertyNames.ServerTcpPort, It.IsAny<int>())).Returns(65431);
            mockProperties.Setup(m => m.GetProperty(HHRPropertyNames.ServerUdpPort, It.IsAny<int>())).Returns(1234);
            mockProperties.Setup(m => m.GetProperty(HHRPropertyNames.HeartbeatInterval, It.IsAny<double>())).Returns(HeartbeatInterval);

            // Set up the event bus so we can monitor for online/offline events from the service.
            mockEventBus.Setup(m => m.Publish(It.IsAny<CentralServerOnline>())).Callback(HandleTcpOnline);
            onlineTcpEventCount = 0;
            mockEventBus.Setup(m => m.Publish(It.IsAny<CentralServerOffline>())).Callback(HandleTcpOffline);
            offlineTcpEventCount = 0;

            // Set up the central manager so we can monitor for hearbeats being sent over the connection.
            mockManager.Setup(
                m => m.Send<HeartBeatRequest, HeartBeatResponse>(
                    It.IsAny<HeartBeatRequest>(), It.IsAny<CancellationToken>())).Callback(HandleMessage);
            heartbeatEventCount = 0;

            // Now we can create the communication service which will be tested in each test.
            commService = new CommunicationService(new TcpConnection(), new UdpConnection(), mockManager.Object, mockProperties.Object, mockEventBus.Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            commService.Disconnect().Wait();
            // This little hack is to avoid the AcceptClient firing on a disposed server socket
            TcpListener tempListen = serverTcpSocket;
            serverTcpSocket = null;
            tempListen?.Stop();
            serverTcpClient?.Close();
            // And this is to ensure the OS has closed the socket before the next test starts.
            Thread.Sleep(1000);
        }

        [TestMethod]
        public async Task Open_WithNoServer_ShouldFail()
        {
            bool success = await commService.ConnectTcp();
            Assert.IsFalse(success);
            Assert.AreEqual(2, offlineTcpEventCount);
            Assert.AreEqual(0, onlineTcpEventCount);
        }

        [TestMethod]
        public async Task Open_WithServer_ShouldSucceed()
        {
            OpenServerSocket();
            bool success = await commService.ConnectTcp();
            Assert.IsTrue(success);
            Assert.AreEqual(1, offlineTcpEventCount);
            Assert.AreEqual(1, onlineTcpEventCount);
        }

        [TestMethod]
        public async Task Open_UdpListen_ShouldSucceed()
        {
            bool success = await commService.ConnectUdp();
            Assert.IsTrue(success);
            Assert.AreEqual(1, offlineTcpEventCount);
            Assert.AreEqual(0, onlineTcpEventCount);
        }

        [TestMethod]
        public async Task Open_WithServer_RaiseDisconnect()
        {
            OpenServerSocket();
            bool success = await commService.ConnectTcp();
            Assert.IsTrue(success);
            Assert.AreEqual(1, offlineTcpEventCount);
            Assert.AreEqual(1, onlineTcpEventCount);
            Thread.Sleep(100);
            serverTcpClient?.Close();
            Thread.Sleep(100);
            Assert.AreEqual(2, offlineTcpEventCount);
            Assert.AreEqual(1, onlineTcpEventCount);
        }

        [TestMethod]
        public async Task Open_WithServer_SendHearbeat()
        {
            OpenServerSocket();
            bool success = await commService.ConnectTcp();
            Assert.IsTrue(success);
            Assert.AreEqual(1, offlineTcpEventCount);
            Assert.AreEqual(1, onlineTcpEventCount);
            Thread.Sleep((int)(HeartbeatInterval * 2.2));
            Assert.AreEqual(2, heartbeatEventCount);
        }

        private void HandleTcpOnline() => onlineTcpEventCount++;

        private void HandleTcpOffline() => offlineTcpEventCount++;

        private void HandleMessage() => heartbeatEventCount++;
    }
}
