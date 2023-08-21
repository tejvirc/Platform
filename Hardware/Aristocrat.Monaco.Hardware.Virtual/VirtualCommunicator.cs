namespace Aristocrat.Monaco.Hardware.Virtual
{
    using System;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts.Communicator;
    using Contracts.Gds;
    using Contracts.Gds.CardReader;
    using Contracts.IdReader;
    using Contracts.SharedDevice;
    using log4net;
    using NoteAcceptorFailureStatus = Contracts.Gds.NoteAcceptor.FailureStatus;
    using PrinterFailureStatus = Contracts.Gds.Printer.FailureStatus;
    using CardReaderFailureStatus = Contracts.Gds.CardReader.FailureStatus;
    using ReelControllerFailureStatus = Contracts.Gds.Reel.FailureStatus;

    /// <summary>A virtual ID reader.</summary>
    public class VirtualCommunicator : IGdsCommunicator
    {
        /// <summary>0.1 second</summary>
        protected const int TenthSecond = 100;

        /// <summary>1 second</summary>
        protected const int OneSecond = 1000;

        /// <summary>Version 1</summary>
        protected const int VersionOne = 1;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private bool _disposed;
        private byte _transactionId;

        /// <summary>Is enabled.</summary>
        protected bool IsEnabled { get; set; }

        /// <summary>Base name is used to fake out various identification strings (overrideable).</summary>
        protected virtual string BaseName => "Virtual";

        /// <inheritdoc/>
        public virtual string Protocol => BaseName;

        /// <inheritdoc />
        public DeviceType DeviceType { get; set; }

        /// <inheritdoc />
        public string FirmwareVersion => $"{BaseName}1.0";

        /// <inheritdoc />
        public string FirmwareRevision => $"{BaseName}001";

        /// <inheritdoc />
        public int FirmwareCrc => -1;

        /// <inheritdoc />
        public string BootVersion => string.Empty;

        /// <inheritdoc />
        public string VariantName => string.Empty;

        /// <inheritdoc />
        public string VariantVersion => string.Empty;

        /// <inheritdoc />
        public bool IsDfuCapable => false;

        /// <inheritdoc/>
        public virtual string Manufacturer => BaseName + DeviceType;

        /// <inheritdoc/>
        public virtual string Model => BaseName + DeviceType;

        /// <inheritdoc/>
        public virtual string Firmware => BaseName;

        /// <inheritdoc/>
        public virtual string SerialNumber => BaseName;

        /// <inheritdoc/>
        public IDevice Device { get; set; }

        /// <inheritdoc/>
        public virtual bool IsOpen { get; protected set; }

        /// <inheritdoc/>
        public int VendorId { get; set; }

        /// <inheritdoc/>
        public int ProductId { get; set; }

        /// <inheritdoc/>
        public int ProductIdDfu { get; set; }

        /// <inheritdoc/>
        public event EventHandler<EventArgs> DeviceAttached;

        /// <inheritdoc/>
        public event EventHandler<EventArgs> DeviceDetached;

        /// <inheritdoc/>
        public event EventHandler<GdsSerializableMessage> MessageReceived;

        /// <inheritdoc/>
        public virtual void SendMessage(GdsSerializableMessage message, CancellationToken token)
        {
            Logger.Debug($"{DeviceType}/{BaseName} Got message {message}");
            switch (message.ReportId)
            {
                // Common to all GDS devices
                case GdsConstants.ReportId.Ack:
                    if (message is Ack ack && ack.Resync)
                    {
                        _transactionId = ack.TransactionId;
                    }
                    break;
                case GdsConstants.ReportId.Enable:
                    IsEnabled = true;
                    OnMessageReceived(new DeviceState { Enabled = true });
                    break;
                case GdsConstants.ReportId.Disable:
                    IsEnabled = false;
                    OnMessageReceived(new DeviceState { Disabled = true });
                    break;
                case GdsConstants.ReportId.SelfTest:
                    switch (DeviceType)
                    {
                        case DeviceType.NoteAcceptor:
                            OnMessageReceived(new NoteAcceptorFailureStatus()); // no errors
                            break;
                        case DeviceType.Printer:
                            OnMessageReceived(new PrinterFailureStatus()); // no errors
                            break;
                        case DeviceType.IdReader:
                            OnMessageReceived(new CardReaderFailureStatus()); // no errors
                            break;
                        case DeviceType.ReelController:
                            OnMessageReceived(new ReelControllerFailureStatus());
                            break;
                    }
                    break;
                case GdsConstants.ReportId.RequestGatReport:
                    if (!IsEnabled)
                    {
                        var data = $"{Manufacturer},{Model},{Firmware}";
                        OnMessageReceived(new GatData { Data = data, Length = data.Length });
                    }
                    break;
                case GdsConstants.ReportId.CalculateCrc:
                    if (message is CrcRequest crcRequest)
                    {
                        OnMessageReceived(new CrcData { Result = IsEnabled ? 0 : int.MaxValue ^ (int)crcRequest.Seed });
                    }
                    break;

                // For GDS card readers
                case GdsConstants.ReportId.CardReaderGetBehavior:
                    OnMessageReceived(new CardReaderBehavior
                    {
                        SupportedTypes = IdReaderTypes.None,
                        VirtualReportedType = IdReaderTypes.None,
                        IsPhysical = false,
                        IsEgmControlled = GetType() != typeof(VirtualCommunicator),
                        ValidationMethod = GetType() == typeof(VirtualCommunicator) ? IdValidationMethods.Host : IdValidationMethods.Self
                    });
                    break;
                case GdsConstants.ReportId.CardReaderGetConfig:
                    OnMessageReceived(new CardConfiguration
                    {
                        Track1 = true,
                        Track2 = true,
                        Track3 = true
                    });
                    break;
                case GdsConstants.ReportId.CardReaderReadCardData:
                    OnMessageReceived(new CardData
                    {
                        Type = VersionOne,
                        Data = BaseName
                    });
                    break;
                case GdsConstants.ReportId.CardReaderReleaseLatch:
                    OnMessageReceived(new CardStatus { Removed = true });
                    break;
                case GdsConstants.ReportId.CardReaderGetCountStatus:
                    OnMessageReceived(new CountStatus()); // contents are N/A
                    break;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public virtual bool Close()
        {
            IsOpen = false;
            Logger.Debug("Closed communication port");
            return true;
        }

        /// <inheritdoc/>
        public virtual bool Open()
        {
            IsOpen = true;
            Logger.Debug("Opened fake communication port");
            return true;
        }

        /// <inheritdoc/>
        public virtual bool Configure(IComConfiguration comConfiguration)
        {
            comConfiguration.PortName = "Virtual";
            return true;
        }

        /// <inheritdoc/>
        public virtual void ResetConnection()
        {
            Logger.Debug($"Resetting connection for {VendorId:X}...");
            Task.Run(() =>
            {
                OnDeviceDetached();
                Task.Delay(500);
                OnDeviceAttached();
            });
        }

        /// <summary>
        /// Releases the unmanaged resources used by the Aristocrat.Monaco.Hardware.Fake.FakeCommunicator and optionally
        /// releases the managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged
        /// resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // Nothing to do
            }

            _disposed = true;
        }

        /// <summary>Raises the <see cref="DeviceAttached"/> event.</summary>
        protected virtual void OnDeviceAttached()
        {
            var invoker = DeviceAttached;
            invoker?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Raises the <see cref="DeviceDetached"/> event.</summary>
        protected virtual void OnDeviceDetached()
        {
            var invoker = DeviceDetached;
            invoker?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Raises the <see cref="MessageReceived"/> event.</summary>
        /// <param name="message">The message to send back.</param>
        protected virtual void OnMessageReceived(GdsSerializableMessage message)
        {
            Logger.Debug($"{DeviceType}/{BaseName} Respond with {message.ReportId}");
            var invoker = MessageReceived;
            invoker?.Invoke(this, message);
        }

        /// <summary>Get next transaction ID.</summary>
        /// <returns>Next transaction ID.</returns>
        protected byte GetNextTransactionId()
        {
            if (_transactionId == byte.MaxValue)
            {
                _transactionId = 0;
            }

            return _transactionId++;
        }
    }
}
