namespace Aristocrat.G2S.Client.Communications
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     MessageConsumer
    /// </summary>
    internal class MessageConsumer : IMessageConsumer
    {
        private readonly IEgm _egm;
        private readonly List<IMessageReceiver> _observers = new List<IMessageReceiver>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="MessageConsumer" /> class.
        /// </summary>
        /// <param name="egm">EGM</param>
        public MessageConsumer(IEgm egm)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
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

        public Error Consumes(IMulticast broadcast)
        {
            var error = new Error();

            var request = broadcast.Item;
            var command = ClassCommand.Create(request, 0, "");

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