namespace Aristocrat.Monaco.Asp.Client.Comms
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    /// <summary>
    ///     Asp protocol transport layer. Responsible for sending/receiving transport layer messages.
    /// </summary>
    public class TransportLayer : DataLinkLayer
    {
        // Maximum number of events to queue.
        private const int MaxEventQueueSize = 100;
        private readonly ConcurrentQueue<AppResponse> _events = new ConcurrentQueue<AppResponse>();
        private readonly TransportPacket _message;
        private readonly DataLinkPacket _response = new DataLinkPacket();
        private readonly ConcurrentQueue<AppResponse> _responses = new ConcurrentQueue<AppResponse>();
        private readonly TransportPacket _transportResponsePacket;
        private readonly Dictionary<TransportMessageType, Action> _typeHandler;

        public TransportLayer(ICommPort port)
            : base(port)
        {
            _typeHandler = new Dictionary<TransportMessageType, Action>
            {
                { TransportMessageType.Query, HandleQuery }, { TransportMessageType.Control, HandleControl }
            };
            _message = new TransportPacket();
            _transportResponsePacket = new TransportPacket(_response);
        }

        private bool SendingEvents { get; set; }

        private ControlData GetControlAck(ControlData controlCommand)
        {
            switch (controlCommand)
            {
                case ControlData.ResumeSendingEvents:
                    SendingEvents = true;
                    return ControlData.ReceivedResumeAckData;
                case ControlData.StopSendingEvents:
                    SendingEvents = false;
                    return ControlData.ReceivedStopAckData;
                case ControlData.ReceivedInvalidCommandAckData:
                    break;
            }

            return ControlData.ReceivedInvalidCommandAckData;
        }

        private void HandleControl()
        {
            _transportResponsePacket.Clear();
            var msg = _transportResponsePacket.AddMessage(2, TransportMessageType.ControlAck);
            msg.ControlData = GetControlAck(_message[0].ControlData);
        }

        private void HandleQuery()
        {
            ProcessPacket(_message[0]);
            PackagePendingResponses();
        }

        protected virtual void ProcessPacket(TransportMessage message)
        {
        }

        protected override void OnLinkStatusChanged()
        {
            while (_responses.TryDequeue(out _))
            {
                // Remove all pending responses.
            }

            base.OnLinkStatusChanged();
        }

        protected override DataLinkPacket ProcessPacket(DataLinkPacket packet)
        {
            if (packet.Payload.Length != 0)
            {
                _message.Reset(packet);
                _typeHandler[_message[0].Type]();
            }
            else
            {
                PackagePendingResponses();
            }

            return _response;
        }

        private void PackagePendingResponses()
        {
            _transportResponsePacket.Clear();
            if (_responses.TryDequeue(out var response))
            {
                _transportResponsePacket.AddMessage(response);
            }

            while (SendingEvents && _events.TryPeek(out response) &&
                   response.Size + 1 < _transportResponsePacket.NumberOfBytesCanPack)
            {
                _events.TryDequeue(out response);
                _transportResponsePacket.AddMessage(response);
            }
        }

        protected void QueueResponse(AppResponse response)
        {
            if (IsLinkUp)
            {
                _responses.Enqueue(response);
            }
        }

        protected void QueueEvent(AppResponse eventResponse)
        {
            while (_events.Count >= MaxEventQueueSize)
            {
                _events.TryDequeue(out _);
            }

            _events.Enqueue(eventResponse);
        }
    }
}