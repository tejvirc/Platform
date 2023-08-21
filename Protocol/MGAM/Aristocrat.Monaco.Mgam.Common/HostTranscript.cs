namespace Aristocrat.Monaco.Mgam.Common
{
    using System;
    using Aristocrat.Mgam.Client.Routing;
    using MVVM.Model;

    /// <summary>
    ///     A message sent to or from the server.
    /// </summary>
    public class HostTranscript : BaseNotify
    {
        private DateTime _timestamp;
        private string _source;
        private string _destination;
        private string _name;
        private string _summary;
        private int? _responseCode;
        private bool _isRequest;
        private bool _isResponse;
        private bool _isCommand;
        private bool _isNotification;
        private bool _isHeartbeat;
        private string _rawData;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HostTranscript"/> class.
        /// </summary>
        /// <param name="message"></param>
        public HostTranscript(RoutedMessage message)
        {
            _timestamp = message.Timestamp.ToLocalTime();
            _source = message.Source;
            _destination = message.Destination;
            _name = message.Name;
            _summary = message.Name;
            _isRequest = message.IsRequest;
            _isResponse = message.IsResponse;
            _isCommand = message.IsCommand;
            _isNotification = message.IsNotification;
            _isHeartbeat = message.IsHeartbeat;
            _responseCode = message.ResponseCode;
            _rawData = message.RawData;
        }

        /// <summary>
        ///     Gets or sets the time the message was sent.
        /// </summary>
        public DateTime Timestamp
        {
            get => _timestamp;

            set => SetProperty(ref _timestamp, value, nameof(Timestamp));
        }

        /// <summary>
        ///     Gets or sets the source address.
        /// </summary>
        public string Source
        {
            get => _source;

            set => SetProperty(ref _source, value, nameof(Source));
        }

        /// <summary>
        ///     Gets or sets the destination address.
        /// </summary>
        public string Destination
        {
            get => _destination;

            set => SetProperty(ref _destination, value, nameof(Destination));
        }

        /// <summary>
        ///     Gets or sets the summary.
        /// </summary>
        public string Summary
        {
            get => _summary;

            set => SetProperty(ref _summary, value, nameof(Summary));
        }

        /// <summary>
        ///     Gets or sets the type of the message.
        /// </summary>
        public string Name
        {
            get => _name;

            set => SetProperty(ref _name, value, nameof(Name));
        }

        /// <summary>
        ///     Gets or sets the message response code.
        /// </summary>
        public int? ResponseCode
        {
            get => _responseCode;

            set => SetProperty(ref _responseCode, value, nameof(ResponseCode));
        }

        /// <summary>
        ///     Gets or sets the message response code.
        /// </summary>
        public string RawData
        {
            get => _rawData;

            set => SetProperty(ref _rawData, value, nameof(RawData));
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether the message is a request.
        /// </summary>
        public bool IsRequest
        {
            get => _isRequest;

            set => SetProperty(ref _isRequest, value, nameof(IsRequest));
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether the message is a response.
        /// </summary>
        public bool IsResponse
        {
            get => _isResponse;

            set => SetProperty(ref _isResponse, value, nameof(IsResponse));
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether the message is a command.
        /// </summary>
        public bool IsCommand
        {
            get => _isCommand;

            set => SetProperty(ref _isCommand, value, nameof(IsCommand));
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether the message is a notification.
        /// </summary>
        public bool IsNotification
        {
            get => _isNotification;

            set => SetProperty(ref _isNotification, value, nameof(IsNotification));
        }

        /// <summary>
        ///     Gets or sets a value that indicates whether the message is the keep-alive message.
        /// </summary>
        public bool IsHeartbeat
        {
            get => _isHeartbeat;

            set => SetProperty(ref _isHeartbeat, value, nameof(IsHeartbeat));
        }
    }
}
