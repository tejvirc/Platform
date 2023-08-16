namespace Aristocrat.Monaco.Hardware.Gds.CardReader
{
    using System;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Contracts.CardReader;
    using Contracts.Communicator;
    using Contracts.Gds;
    using Contracts.Gds.CardReader;
    using Contracts.IdReader;
    using Contracts.SharedDevice;
    using log4net;

    /// <summary>A GDS card reader.</summary>
    /// <seealso cref="T:Aristocrat.Monaco.Hardware.GdsDeviceBase"/>
    public class CardReaderGds : GdsDeviceBase, IIdReaderImplementation
    {
        private const byte AtrType = 0;
        private const byte Track1Type = 1;
        private const byte Track2Type = 2;
        private const byte Track3Type = 3;

        // Sentinels from MAGTEK Magnetic Strip Card Standards
        private const char StartSentinelTrack1 = '%';
        private const char StartSentinelTrack2And3 = ';';
        private const char EndSentinel = '?';

        /// <summary>The default flash duration.</summary>
        public const int DefaultFlashDuration = 500;

        [Flags]
        private enum UnsupportedCommands
        {
            None = 0,
            DeviceStateReport,
            GatReportCommand,
            GetCountStatusCommand,
            LatchModeCommand,
            ReleaseLatchCommand,
            UicLightControl
        }

        [Flags]
        private enum ColorFlags
        {
            None = 0,
            Red = 1,
            Green = 2,
            Yellow = 4
        }

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private bool _inserted;
        private bool _isPhysical = true;
        private IdReaderTypes _idReaderType = IdReaderTypes.MagneticCard;
        private IdValidationMethods _validationMethod = IdValidationMethods.Host;
        private UnsupportedCommands _unsupported = UnsupportedCommands.None;

        /// <summary>
        /// Initializes a new instance of the Aristocrat.Monaco.Hardware.CardReader.CardReaderGds class.
        /// </summary>
        public CardReaderGds()
        {
            DeviceType = DeviceType.IdReader;
            RegisterCallback<CardReaderBehavior>(BehaviorReported);
            RegisterCallback<FailureStatus>(FailureReported);
            RegisterCallback<CardConfiguration>(ConfigurationReported);
            RegisterCallback<CardStatus>(CardStatusReported);
            RegisterCallback<CardData>(CardDataReported);
            RegisterCallback<ErrorData>(ErrorReported);
            RegisterCallback<CountStatus>(CountReported);
        }

        /// <inheritdoc/>
        public virtual int IdReaderId { get; set; }

        /// <inheritdoc/>
        public virtual bool IsEgmControlled { get; protected set; } = true;

        /// <inheritdoc/>
        public virtual TrackData TrackData { get; protected set; }

        /// <inheritdoc/>
        public virtual IdReaderTypes IdReaderType
        {
            get => _idReaderType;
            set
            {
                if (!SupportedTypes.HasFlag(value))
                {
                    return;
                }

                _idReaderType = value;
            }
        }

        /// <inheritdoc/>
        public virtual IdReaderTypes SupportedTypes { get; protected set; } = IdReaderTypes.MagneticCard | IdReaderTypes.SmartCard;

        /// <inheritdoc/>
        public virtual IdReaderTracks IdReaderTrack { get; set; } = IdReaderTracks.Track1 | IdReaderTracks.Track2;

        /// <inheritdoc/>
        public virtual IdReaderTracks SupportedTracks { get; protected set; }

        /// <inheritdoc/>
        public virtual IdValidationMethods ValidationMethod
        {
            get => _validationMethod;
            set
            {
                if (!SupportedValidation.HasFlag(value))
                {
                    return;
                }

                _validationMethod = value;
            }
        }

        /// <inheritdoc/>
        public virtual IdValidationMethods SupportedValidation { get; set; } = IdValidationMethods.Host | IdValidationMethods.Self;

        /// <summary>Gets a value indicating whether a card is inserted.</summary>
        /// <value>True if inserted, false if not.</value>
        public bool Inserted
        {
            get => _inserted;
            protected set
            {
                if (value == _inserted)
                {
                    return;
                }

                _inserted = value;
                if (value)
                {
                    OnIdPresented();
                }
                else
                {
                    OnIdCleared();
                }
            }
        }

        /// <summary>Gets the available tracks.</summary>
        /// <value>The available tracks.</value>
        public IdReaderTracks AvailableTracks { get; protected set; }

        /// <summary>Gets the faults.</summary>
        /// <value>The faults.</value>
        public IdReaderFaultTypes Faults { get; protected set; }

        /// <summary>Gets a value indicating whether the latch mechanism is supported.</summary>
        /// <value>True if the latch mechanism is supported, false if not.</value>
        public bool IsLatchSupported { get; protected set; }

        /// <summary>Gets a value indicating whether the extended light control is supported.</summary>
        /// <value>True if the latch extended light control is supported, false if not.</value>
        public bool IsExtendedLightSupported { get; protected set; }

        /// <summary>Gets or sets the number of card inserted.</summary>
        /// <value>The number of card inserted.</value>
        public int CardInsertedCount { get; protected set; }

        /// <summary>Gets or sets the number of track 1 errors.</summary>
        /// <value>The number of track 1 errors.</value>
        public int Track1ErrorCount { get; protected set; }

        /// <summary>Gets or sets the number of track 2 errors.</summary>
        /// <value>The number of track 2 errors.</value>
        public int Track2ErrorCount { get; protected set; }

        /// <summary>Gets or sets the number of track 3 errors.</summary>
        /// <value>The number of track 3 errors.</value>
        public int Track3ErrorCount { get; protected set; }

        /// <summary>Gets or sets the number of times an ICC card has been inserted.</summary>
        /// <value>The number of times an ICC card has been inserted.</value>
        public int IccInsertedCount { get; protected set; }

        /// <summary>Gets or sets the number of ICC errors.</summary>
        /// <value>The number of ICC errors.</value>
        public int IccErrorCount { get; protected set; }

        /// <inheritdoc/>
        public event EventHandler<FaultEventArgs> FaultCleared;

        /// <inheritdoc/>
        public event EventHandler<FaultEventArgs> FaultOccurred;

        /// <inheritdoc/>
        public event EventHandler<EventArgs> IdPresented;

        /// <inheritdoc/>
        public event EventHandler<EventArgs> IdCleared;

        /// <inheritdoc/>
        public event EventHandler<ValidationEventArgs> IdValidationRequested;

        /// <inheritdoc/>
        public event EventHandler<EventArgs> ReadError;

        /// <inheritdoc/>
        public override bool Close()
        {
            Eject(); // clear device buffer so it is ready to read data
            return base.Close();
        }

        /// <inheritdoc/>
        public override async Task<bool> Initialize(IGdsCommunicator communicator)
        {
            var result = await base.Initialize(communicator);

            if (result)
            {
                SendCommand<CardReaderBehavior>(new GdsSerializableMessage(GdsConstants.ReportId.CardReaderGetBehavior));
                var report = await WaitForReport<CardReaderBehavior>();
                if (report == null)
                {
                    return false;
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public virtual void Eject()
        {
            ReleaseLatch();
            ClearBuffer();

            TrackData = null;

            // Don't set Inserted to false because the card is still inserted but in error
            //Inserted = false;
        }

        /// <inheritdoc/>
        public void ValidationComplete()
        {
            if(Inserted)
            {
                // Validation has been completed without errors, so set reader light to green
                Task.Run(() => { SetBezelState(BezelState.SolidGreen); });
            }
        }

        /// <inheritdoc/>
        public void ValidationFailed()
        {
            if (Inserted)
            {
                Task.Run(() => { SetBezelState(BezelState.FlashingRed); });
            }
        }

        /// <inheritdoc/>
        public virtual Task<bool> SetIdNumber(string idNumber)
        {
            if (IsEgmControlled)
            {
                return Task.FromResult(false);
            }

            if (_isPhysical)
            {
                throw new NotSupportedException("GDS does not support remote setting of ID at this time");
            }

            // assume the ID is validated since it is coming from the host - so do not request validation
            Inserted = true;

            TrackData = new TrackData { Track1 = idNumber };
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public override void UpdateConfiguration(IDeviceConfiguration internalConfiguration)
        {
            // These values are swapped
            internalConfiguration.FirmwareId = internalConfiguration.FirmwareRevision;
            internalConfiguration.FirmwareRevision = string.Empty;            
        }

        /// <summary>Sets bezel state.</summary>
        /// <param name="state">The state.</param>
        public async void SetBezelState(BezelState state)
        {
            if (!IsInitialized)
            {
                return; // do not modify light state while initializing
            }

            switch (state)
            {
                case BezelState.Off:
                    LightControl(false, false, false, 0);
                    break;
                case BezelState.SolidRed:
                    LightControl(true, false, false, 0);
                    break;
                case BezelState.FlashingRed:
                    // For some reason if going from Red to FlashingRed the reader can't keep up, so we need a short delay
                    await Task.Delay(500);
                    if (Inserted) // did the card get removed since waiting?
                    {
                        LightControl(true, false, false, DefaultFlashDuration);
                    }
                    break;
                case BezelState.SolidGreen:
                    LightControl(false, true, false, 0);
                    break;
                case BezelState.FlashingGreen:
                    if (Inserted)
                    {
                        LightControl(false, true, false, DefaultFlashDuration);
                    }
                    break;
                case BezelState.SolidOrange:
                    LightControl(true, true, false, 0);
                    break;
                case BezelState.FlashingOrange:
                    LightControl(true, true, false, DefaultFlashDuration);
                    break;
                case BezelState.SolidYellow:
                    LightControl(true, true, true, 0);
                    break;
                case BezelState.FlashingYellow:
                    LightControl(true, true, true, DefaultFlashDuration);
                    break;
            }
        }

        /// <inheritdoc/>
        public override Task<bool> Enable()
        {
            if (_unsupported.HasFlag(UnsupportedCommands.DeviceStateReport))
            {
                // UIC does not respond to Enable command properly so just assume it succeeded
                SendCommand(new GdsSerializableMessage(GdsConstants.ReportId.Enable));
                IsEnabled = true;
                return Task.FromResult(true);
            }

            return base.Enable();
        }

        /// <inheritdoc/>
        public override Task<bool> Disable()
        {
            if (_unsupported.HasFlag(UnsupportedCommands.DeviceStateReport))
            {
                // UIC does not respond to Disable command properly so just assume it succeeded
                SendCommand(new GdsSerializableMessage(GdsConstants.ReportId.Disable));
                IsEnabled = false;
                return Task.FromResult(true);
            }

            return base.Disable();
        }

        /// <inheritdoc/>
        public override Task<string> RequestGatReport(int milliseconds = DefaultResponseTimeout)
        {
            if (_unsupported.HasFlag(UnsupportedCommands.GatReportCommand))
            {
                return Task.FromResult(string.Empty);
            }

            return base.RequestGatReport();
        }

        /// <inheritdoc/>
        public override async Task<bool> SelfTest(bool nvm)
        {
            if (IsEnabled) // this command is only allowed when the device is disabled
            {
                return false;
            }

            SendCommand(
                new SelfTest
                {
                    Nvm = nvm ? 1 : 0
                });
            var report = await WaitForReport<FailureStatus>(ExtendedResponseTimeout);
            if (report == null)
            {
                return false;
            }

            if (report.FirmwareError || report.IccPowerFail || report.DiagnosticCode)
            {
                return false;
            }

            return true;
        }

        /// <summary>Gets the configuration.</summary>
        /// <returns>An asynchronous result that yields the configuration.</returns>
        public virtual async Task<bool> GetConfiguration()
        {
            if (IsEnabled) // this command is only allowed when the device is disabled
            {
                return false;
            }

            SendCommand<CardConfiguration>(new GdsSerializableMessage(GdsConstants.ReportId.CardReaderGetConfig));
            SupportedTracks = IdReaderTracks.None;
            var report = await WaitForReport<CardConfiguration>();
            return report != null;
        }

        /// <summary>Reads card data.</summary>
        /// <param name="track1">True to track 1.</param>
        /// <param name="track2">True to track 2.</param>
        /// <param name="track3">True to track 3.</param>
        /// <returns>An asynchronous result that yields the card data.</returns>
        public virtual async Task<string> ReadCardData(bool track1, bool track2, bool track3)
        {
            if (!IsEnabled) // this command is only allowed when the device is enabled
            {
                return string.Empty;
            }

            // can only be exactly one track read at a time
            if (!ValidateTrackReads(track1, track2, track3))
            {
                return string.Empty;
            }

            SendCommand(
                new ReadCardData
                {
                    Track1 = track1,
                    Track2 = track2,
                    Track3 = track3
                });

            byte trackType = 0;
            if (track1)
            {
                trackType = Track1Type;
            }

            if (track2)
            {
                trackType = Track2Type;
            }

            if (track3)
            {
                trackType = Track3Type;
            }

            return await WaitForCardData(trackType);
        }

        /// <summary>Gets answer to reset.</summary>
        /// <returns>An asynchronous result that yields the ATR response.</returns>
        public virtual async Task<string> GetAtr()
        {
            if (!IsEnabled) // this command is only allowed when the device is enabled
            {
                return string.Empty;
            }

            SendCommand<CardData>(new GdsSerializableMessage(GdsConstants.ReportId.CardReaderGetAnswerToReset));
            return await WaitForCardData(AtrType);
        }

        /// <summary>Releases the latch.</summary>
        public virtual void ReleaseLatch()
        {
            if (_unsupported.HasFlag(UnsupportedCommands.ReleaseLatchCommand))
            {
                return;
            }

            SendCommand(new GdsSerializableMessage(GdsConstants.ReportId.CardReaderReleaseLatch));
        }

        /// <summary>This command is used to control the LEDs of the lighted bezel of a reader.</summary>
        /// <param name="red">True if red LED should be turned on; otherwise false.</param>
        /// <param name="green">True to green LED should be turned on; otherwise false.</param>
        /// <param name="yellow">True to yellow LED should be turned on; otherwise false.</param>
        /// <param name="timeout">The timeout in milliseconds.</param>
        public virtual void LightControl(bool red, bool green, bool yellow, int timeout)
        {
            var interval = timeout / 100;
            if (interval < 0 || interval > byte.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout));
            }

            if (_unsupported.HasFlag(UnsupportedCommands.UicLightControl))
            {
                LightControl((red ? ColorFlags.Red : ColorFlags.None) |
                    (green ? ColorFlags.Green : ColorFlags.None) |
                    (yellow ? ColorFlags.Red : ColorFlags.None), (byte)interval);
                return;
            }

            SendCommand(
                new LightControl
                {
                    Red = red,
                    Green = green,
                    Yellow = yellow,
                    TimerInterval = (byte)interval
                });
        }

        /// <summary>This command is used to control the LEDs of the lighted bezel of a reader.</summary>
        /// <param name="red">The red LED intensity.</param>
        /// <param name="green">The green LED intensity.</param>
        /// <param name="blue">The blue LED intensity.</param>
        /// <param name="timeout">The timeout in milliseconds.</param>
        public virtual void LightControl(byte red, byte green, byte blue, int timeout)
        {
            var interval = timeout / 10;
            if (interval < 0 || interval > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout));
            }

            if (_unsupported.HasFlag(UnsupportedCommands.UicLightControl))
            {
                LightControl(Scale16(red), Scale16(green), Scale16(blue), (byte)(interval / 10));
                return;
            }

            SendCommand(
                new ExtendedLightControl
                {
                    Red = red,
                    Green = green,
                    Blue = blue,
                    TimerInterval = (ushort)interval
                });
        }

        /// <summary>
        /// The host sends this command to instruct the card reader to place latch in Lock or Release position upon card
        /// being fully inserted.
        /// </summary>
        /// <param name="lock">True to lock, false to unlock.</param>
        /// <param name="release">True to release.</param>
        public virtual void LatchMode(bool @lock, bool release)
        {
            if (_unsupported.HasFlag(UnsupportedCommands.LatchModeCommand))
            {
                return;
            }

            SendCommand(
                new LatchMode
                {
                    Lock = @lock,
                    Release = release
                });
        }

        /// <summary>Clears the buffer.</summary>
        public virtual void ClearBuffer()
        {
            if (!IsEnabled) // this command is only allowed when the device is enabled
            {
                return;
            }

            SendCommand(new GdsSerializableMessage(GdsConstants.ReportId.CardReaderClearBuffer));
        }

        /// <summary>Gets count status.</summary>
        /// <returns>An asynchronous result that yields the count status.</returns>
        public virtual async Task<bool> GetCountStatus()
        {
            if (_unsupported.HasFlag(UnsupportedCommands.GetCountStatusCommand))
            {
                return true;
            }

            SendCommand<CountStatus>(new GdsSerializableMessage(GdsConstants.ReportId.CardReaderGetCountStatus));
            var report = await WaitForReport<CountStatus>();
            return report != null;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Eject();
                // turn off light when closing
                LightControl(false, false, false, 0);
            }

            base.Dispose(disposing);
        }


        /// <inheritdoc/>
        protected override async Task<bool> Reset()
        {
            var resetResponse = await InternalReset();
            if (resetResponse)
                OnSuccessReset();
            else
                OnFailedReset();

            return resetResponse;
        }

        private async Task<bool> InternalReset()
        {
            switch (Model)
            {
                case "MSR241U Card Reader ":
                    SupportedTypes = IdReaderTypes.MagneticCard;
                    SupportedValidation = IdValidationMethods.Host;
                    _unsupported = UnsupportedCommands.DeviceStateReport |
                                   UnsupportedCommands.UicLightControl |
                                   UnsupportedCommands.GatReportCommand |
                                   UnsupportedCommands.GetCountStatusCommand |
                                   UnsupportedCommands.ReleaseLatchCommand;
                    break;
            }

            IsInitialized = false;

            if (!await Disable())
            {
                return false; // first disable the device
            }

            // green light to indicate that the device is in initialization state
            LightControl(false, true, false, 0);

            LatchMode(true, false); // lock the latch

            if (await CalculateCrc(GdsConstants.DefaultSeed) == 0)
            {
                return false;
            }

            if (await RequestGatReport() == null)
            {
                return false;
            }

            if (!await GetConfiguration())
            {
                return false;
            }

            if (!await GetCountStatus())
            {
                return false;
            }

            // release latch to allow player to remove player card
            ReleaseLatch();

            // turn off light to indicate that the device initialization is done
            LightControl(false, false, false, 0);

            Eject(); // clear device buffer so it is ready to read data
            RequiresReset = false;
            AvailableTracks = IdReaderTracks.None;
            IsInitialized = true;
            return true;
        }

        /// <summary>Reads the card.</summary>
        /// <exception cref="NotSupportedException">Thrown when the requested operation is not supported.</exception>
        /// <returns>The ID number on the card.</returns>
        protected virtual TrackData ReadCard()
        {
            if (!Inserted)
            {
                return null;
            }

            TrackData data = new TrackData();
            try
            {
                var tracks = AvailableTracks;
                if ((tracks & IdReaderTracks.Icc) > 0)
                {
                    throw new NotSupportedException("ICC not supported yet");
                }

                string track1Data = string.Empty;
                string track2Data = string.Empty;
                string track3Data = string.Empty;

                if ((tracks & IdReaderTracks.Track1) > 0 && (tracks & IdReaderTrack) > 0)
                {
                    track1Data = ReadCardData(true, false, false)
                        .Result
                        .TrimStart(StartSentinelTrack1)
                        .TrimEnd(EndSentinel);
                }

                if ((tracks & IdReaderTracks.Track2) > 0 && (tracks & IdReaderTrack) > 0)
                {
                    track2Data = ReadCardData(false, true, false)
                        .Result
                        .TrimStart(StartSentinelTrack2And3)
                        .TrimEnd(EndSentinel);
                }

                if ((tracks & IdReaderTracks.Track3) > 0 && (tracks & IdReaderTrack) > 0)
                {
                    track3Data = ReadCardData(false, false, true)
                        .Result
                        .TrimStart(StartSentinelTrack2And3)
                        .TrimEnd(EndSentinel);
                }

                data.Track1 = track1Data;
                data.Track2 = track2Data;
                data.Track3 = track3Data;

                return string.IsNullOrEmpty(data.IdNumber) ? null : data;
            }
            finally
            {
                if (string.IsNullOrEmpty(data.IdNumber))
                {
                    OnReadError();
                }
                else
                {
                    OnIdValidationRequested(new ValidationEventArgs { TrackData = data });
                }
            }
        }

        /// <summary>
        /// Updates the fault flags for this device.
        /// </summary>
        /// <param name="fault">The fault.</param>
        /// <param name="clear">True to clear; otherwise fault will be set.</param>
        protected virtual void SetFault(IdReaderFaultTypes fault, bool clear)
        {
            if (clear)
            {
                var cleared = Faults & fault;
                if (cleared == IdReaderFaultTypes.None)
                {
                    return;  // no updates
                }

                Faults &= ~fault;
                OnFaultCleared(new FaultEventArgs
                {
                    Fault = cleared
                });
            }
            else
            {
                var set = Faults & fault;
                if (set == fault)
                {
                    return; // no updates
                }

                Faults |= fault;
                OnFaultOccurred(new FaultEventArgs { Fault = set });
            }
        }

        /// <summary>Wait for card data.</summary>
        /// <param name="validTypes">A variable-length parameters list containing valid types.</param>
        /// <returns>An asynchronous result that yields a string.</returns>
        protected async Task<string> WaitForCardData(params byte[] validTypes)
        {
            var builder = new StringBuilder();
            while (true)
            {
                var report = await WaitForReport<CardData>();
                if (report == null)
                {
                    return string.Empty; // invalid data
                }

                if (Array.IndexOf(validTypes, report.Type) < 0)
                {
                    return string.Empty; // invalid data
                }

                builder.Append(report.Data);
                if (report.Length < report.MaxPacketSize)
                {
                    break;
                }
            }

            return builder.ToString();
        }

        /// <inheritdoc/>
        protected override void OnDisabled()
        {
            SetBezelState(BezelState.Off);
            base.OnDisabled();
        }

        /// <inheritdoc/>
        protected override void OnEnabled()
        {
            SetBezelState(BezelState.SolidRed);
            base.OnEnabled();
        }

        /// <summary>Raises the <see cref="FaultCleared"/> event.</summary>
        /// <param name="e">Event information to send to registered event handlers.</param>
        protected virtual void OnFaultCleared(FaultEventArgs e)
        {
            var invoker = FaultCleared;
            invoker?.Invoke(this, e);
        }

        /// <summary>Raises the <see cref="FaultOccurred"/> event.</summary>
        /// <param name="e">Event information to send to registered event handlers.</param>
        protected virtual void OnFaultOccurred(FaultEventArgs e)
        {
            var invoker = FaultOccurred;
            invoker?.Invoke(this, e);
        }

        /// <summary>Raises the <see cref="IdPresented"/> event.</summary>
        protected virtual void OnIdPresented()
        {
            Task.Run(() => { SetBezelState(BezelState.FlashingGreen); });

            var invoker = IdPresented;
            invoker?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Raises the <see cref="IdCleared"/> event.</summary>
        protected virtual void OnIdCleared()
        {
            TrackData = null;

            SetBezelState(IsEnabled ? BezelState.SolidRed : BezelState.Off);
            var invoker = IdCleared;
            invoker?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Raises the <see cref="IdValidationRequested"/> event.</summary>
        /// <param name="e">Event information to send to registered event handlers.</param>
        protected virtual void OnIdValidationRequested(ValidationEventArgs e)
        {
            var invoker = IdValidationRequested;
            invoker?.Invoke(this, e);
        }

        /// <summary>Raises the <see cref="ReadError"/> event.</summary>
        protected virtual void OnReadError()
        {
            if (Inserted)
            {
                SetBezelState(BezelState.FlashingRed);
            }

            var invoker = ReadError;
            invoker?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Called when a behavior report is received.</summary>
        /// <param name="report">The report.</param>
        protected virtual void BehaviorReported(CardReaderBehavior report)
        {
            SupportedTypes = report.SupportedTypes;
            _isPhysical = report.IsPhysical;
            if (!_isPhysical)
            {
                IdReaderType = report.VirtualReportedType;
            }
            IsEgmControlled = report.IsEgmControlled;
            ValidationMethod = report.ValidationMethod;

            PublishReport(report);
        }

        /// <summary>Called when a card configuration report is received.</summary>
        /// <param name="report">The report.</param>
        protected virtual void ConfigurationReported(CardConfiguration report)
        {
            Logger.Debug($"ConfigurationReported: {report}");
            SupportedTracks = IdReaderTracks.None;
            if (report.Icc)
            {
                SupportedTracks |= IdReaderTracks.Icc;
            }

            if (report.Track1)
            {
                SupportedTracks |= IdReaderTracks.Track1;
            }

            if (report.Track2)
            {
                SupportedTracks |= IdReaderTracks.Track2;
            }

            if (report.Track3)
            {
                SupportedTracks |= IdReaderTracks.Track3;
            }

            IsLatchSupported = report.Latch;
            IsExtendedLightSupported = report.ExtendedLightSupport;

            PublishReport(report);
        }

        /// <summary>Called when a card data report is received.</summary>
        /// <param name="report">The report.</param>
        protected virtual void CardDataReported(CardData report)
        {
            Logger.Debug($"CardDataReported: {report}");
            if (!IsEnabled)
            {
                Logger.Warn("CardDataReported: IdReader not enabled");
                return;
            }

            PublishReport(report);
        }

        /// <summary>Called when a card status report is received.</summary>
        /// <param name="report">The report.</param>
        protected virtual void CardStatusReported(CardStatus report)
        {
            Logger.Debug($"CardStatusReported: {report}");
            if (!IsEnabled)
            {
                Logger.Warn("CardStatusReported: IdReader not enabled");
                return; // not ready yet
            }

            Inserted = report.Inserted;
            AvailableTracks = IdReaderTracks.None;
            if (!Inserted)
            {
                TrackData = null;
                return; // no available tracks
            }

            if (report.Icc)
            {
                AvailableTracks |= IdReaderTracks.Icc;
            }

            if (report.Track1)
            {
                AvailableTracks |= IdReaderTracks.Track1;
            }

            if (report.Track2)
            {
                AvailableTracks |= IdReaderTracks.Track2;
            }

            if (report.Track3)
            {
                AvailableTracks |= IdReaderTracks.Track3;
            }

            TrackData = ReadCard();
        }

        /// <summary>Called when a count status report is received.</summary>
        /// <param name="report">The report.</param>
        protected virtual void CountReported(CountStatus report)
        {
            Logger.Debug($"CountReported: {report}");

            CardInsertedCount = report.CardInsertedCount;
            Track1ErrorCount = report.Track1ErrorCount;
            Track2ErrorCount = report.Track2ErrorCount;
            Track3ErrorCount = report.Track3ErrorCount;
            IccInsertedCount = report.IccInsertedCount;
            IccErrorCount = report.IccErrorCount;

            PublishReport(report);
        }

        /// <summary>Called when a error data report is received.</summary>
        /// <param name="report">The report.</param>
        protected virtual void ErrorReported(ErrorData report)
        {
            PublishReport(report);
        }

        /// <summary>Called when a failure status report is received.</summary>
        /// <param name="report">The report.</param>
        protected virtual void FailureReported(FailureStatus report)
        {
            Logger.Debug($"FailureReported: {report}");

            if (report.FirmwareError)
            {
                Logger.Error("FailureReported: Firmware failed");
            }

            if (report.DiagnosticCode)
            {
                Logger.Error("FailureReported: ICC power failed");
            }

            if (report.DiagnosticCode)
            {
                Logger.Error($"FailureReported: Diagnostic code {report.ErrorCode}");
            }

            SetFault(IdReaderFaultTypes.FirmwareFault, report.FirmwareError);
            SetFault(IdReaderFaultTypes.PowerFail, report.IccPowerFail);
            SetFault(IdReaderFaultTypes.OtherFault, report.DiagnosticCode);

            PublishReport(report);
        }

        private void LightControl(ColorFlags flags, byte interval)
        {
            byte red = 0;
            byte green = 0;
            byte blue = 0;
            switch (flags)
            {
                case ColorFlags.None:
                    break;
                case ColorFlags.Red:
                    red = 0xff;
                    break;
                case ColorFlags.Green:
                    green = 0xff;
                    break;
                case ColorFlags.Yellow:
                    red = 0xff;
                    green = 0xff;
                    break;
                case ColorFlags.Red | ColorFlags.Green:
                    red = 0xff;
                    green = 0xff;
                    break;
                case ColorFlags.Red | ColorFlags.Yellow:
                    red = 0xff;
                    green = 0x80;
                    break;
                case ColorFlags.Green | ColorFlags.Yellow:
                    green = 0xff;
                    blue = 0x80;
                    break;
                case ColorFlags.Red | ColorFlags.Green | ColorFlags.Yellow:
                    red = 0xff;
                    green = 0xff;
                    blue = 0xff;
                    break;
            }

            LightControl(Scale16(red), Scale16(green), Scale16(blue), interval);
        }

        private void LightControl(ushort red, ushort green, ushort blue, byte interval)
        {
            SendCommand(
                new UicLightControl
                {
                    Red = red,
                    Green = green,
                    Blue = blue,
                    Red2 = 0,
                    Green2 = 0,
                    Blue2 = 0,
                    TimerInterval = interval
                });
        }

        private static ushort Scale16(byte input)
        {
            return (ushort)(input * ushort.MaxValue / byte.MaxValue);
        }

        private static bool ValidateTrackReads(params bool[] tracks)
        {
            var count = 0;
            foreach (var item in tracks)
            {
                if (item && ++count > 1)
                {
                    return false;
                }
            }

            return count == 1;
        }
    }
}
