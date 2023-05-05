namespace Aristocrat.G2S.Client.Communications
{
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     MessageConsumer
    /// </summary>
    internal class MessageConsumer : IMessageConsumer
    {
        private readonly IEgm _egm;
        private readonly IDeviceConnector _deviceConnector;
        private readonly List<IMessageReceiver> _observers = new List<IMessageReceiver>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="MessageConsumer" /> class.
        /// </summary>
        /// <param name="egm">EGM</param>
        /// <param name="deviceConnector">An instance of a IDeviceConnector</param>
        public MessageConsumer(IEgm egm, IDeviceConnector deviceConnector)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _deviceConnector = deviceConnector ?? throw new ArgumentNullException(nameof(deviceConnector));
        }

        /// <inheritdoc />
        public Error Consumes(IPoint2Point point2Point)
        {
            var error = new Error();

            if (_egm.Id != point2Point.egmId)
            {
                error.SetErrorCode(ErrorCode.G2S_MSX002); // Incorrect egmId Specified
                return error;
            }

            foreach (var request in point2Point.Items)
            {
                var command = ClassCommand.Create(request, point2Point.hostId, point2Point.egmId);

                foreach (var observer in _observers)
                {
                    try
                    {
                        observer.Receive(command);
                    }
                    catch (InvalidOperationException)
                    {
                        error.SetErrorCode(ErrorCode.G2S_MSX006); // Inbound Command Queue Full
                        return error;
                    }
                }
            }

            return error;
        }

        public Error Consumes(IBroadcast broadcast)
        {
            var error = new Error();

            foreach (var request in broadcast.Items)
            {
                // We are queueing these broadcasted messages as P2P so that we can have one queue.
                var originalClass = request as IMulticast;
                IClass convertedClass = null;
                IEnumerable<IDevice> devices;
                int deviceId = 1;
                switch (request)
                {
                    case playerMulticast:
                        convertedClass = new player();
                        devices = _deviceConnector.GetDevices<IPlayerDevice>();
                        break;
                    case bonusMulticast:
                        convertedClass = new bonus();
                        devices = _deviceConnector.GetDevices<IBonusDevice>();
                        break;
                    case progressiveMulticast: // Only SetProgressiveValue is converted properly today with respect to the deviceId
                        convertedClass = new progressive();
                        var progDevices = _deviceConnector.GetDevices<IProgressiveDevice>();
                        devices = progDevices;
                        setProgressiveValue setProgValueCommand = originalClass.Item as setProgressiveValue;
                        deviceId = (int)(progDevices?.Where(d => d.ProgressiveId == setProgValueCommand.setLevelValue[0].progId).Select(d => d.Id).DefaultIfEmpty(deviceId).First());
                        break;
                    case noteAcceptorMulticast:
                        convertedClass = new noteAcceptor();
                        devices = _deviceConnector.GetDevices<INoteAcceptorDevice>();
                        break;
                }

               
                if (originalClass == null || convertedClass == null)
                    continue;

                convertedClass.Item = originalClass.Item;
                convertedClass.deviceId = deviceId;

                var command = ClassCommand.Create(convertedClass, 0, _egm.Id);

                foreach (var observer in _observers)
                {
                    try
                    {
                        observer.Receive(command);
                    }
                    catch (InvalidOperationException)
                    {
                        error.SetErrorCode(ErrorCode.G2S_MSX006); // Inbound Command Queue Full
                        return error;
                    }
                }
            }

            return error;
        }

        /// <inheritdoc />
        public void Connect(IMessageReceiver observer)
        {
            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            if (_observers.Contains(observer))
            {
                return;
            }

            _observers.Add(observer);
        }

        /// <inheritdoc />
        public void Disconnect(IMessageReceiver observer)
        {
            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            _observers.Remove(observer);
        }
    }
}