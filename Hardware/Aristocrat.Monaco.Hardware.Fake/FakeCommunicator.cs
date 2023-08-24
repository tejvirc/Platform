namespace Aristocrat.Monaco.Hardware.Fake
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Extensions.CommunityToolkit;
    using Contracts.Communicator;
    using Contracts.Gds;
    using Contracts.Gds.CardReader;
    using Contracts.Gds.Printer;
    using Contracts.Gds.Reel;
    using Contracts.IdReader;
    using Contracts.IO;
    using Contracts.Reel;
    using Contracts.SharedDevice;
    using Kernel;
    using log4net;
    using Simulation.HarkeyReels;
    using Simulation.HarkeyReels.Controls;
    using Virtual;
    using PrinterMetrics = Contracts.Gds.Printer.Metrics;

    /// <summary>A fake communicator.</summary>
    public class FakeCommunicator : VirtualCommunicator
    {
        private const string DefaultBaseName = "Fake";
        private const string SimWindowNamePartial = "ReelLayout_";
        private const string GamesPath = "/Games";
        private const string PackagesPath = "/Packages";
        private const int LightsPerReel = 3;
        private const int StepsPerReel = 200;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly IPathMapper _pathMapper;
        private readonly Queue<GdsSerializableMessage> _simQueue = new();

        private string _fakeCardData;
        private bool _disposed;
        private string _baseName = DefaultBaseName;
        private ReelSetWindow _simWindow;
        private int _id;
        private volatile int _reelCount;
        private int[] _reelOffsets;
        private int[] _homePositions;
        private int _homeCommandCounter;

        /// <summary>
        ///     Construct a <see cref="FakeCommunicator" />
        /// </summary>
        public FakeCommunicator()
            : this(
                ServiceManager.GetInstance().GetService<IPathMapper>(),
                ServiceManager.GetInstance().GetService<IEventBus>())
        {
        }

        public FakeCommunicator(IPathMapper pathMapper, IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _pathMapper = pathMapper ?? throw new ArgumentNullException(nameof(pathMapper));

            Logger.Debug("Constructed");
        }

        /// <summary>Base name is used to fake out various identification strings (overrideable).</summary>
        protected override string BaseName => _baseName;

        /// <inheritdoc />
        public override bool Open()
        {
            //prevent a double subscription for the FakeDeviceConnectedEvent in the case of Open->Close->Open
            _eventBus?.UnsubscribeAll(this);

            switch (DeviceType)
            {
                case DeviceType.IdReader:
                    _eventBus?.Subscribe<FakeCardReaderEvent>(this, HandleEvent);
                    break;

                case DeviceType.Printer:
                    _eventBus?.Subscribe<FakePrinterEvent>(this, HandleEvent);
                    break;

                case DeviceType.ReelController:
                    OpenReelSimulatorWindow();

                    Logger.Debug($"There are {_reelCount} reels");

                    _reelOffsets = new int[_reelCount];
                    _homePositions = new int[_reelCount];
                    _homeCommandCounter = 0;

                    break;
            }

            //We assume the device will be opened by default
            _eventBus?.Subscribe<FakeDeviceConnectedEvent>(this, HandleEvent);

            _eventBus?.Subscribe<FakeDeviceMessageEvent>(this, HandleEvent);

            return base.Open();
        }

        /// <inheritdoc />
        public override bool Close()
        {
            _eventBus?.UnsubscribeAll(this);
            _eventBus?.Subscribe<FakeDeviceConnectedEvent>(this, HandleEvent);

            if (DeviceType == DeviceType.ReelController)
            {
                Execute.OnUIThread(() =>
                    {
                        _simWindow?.Close();
                    }
                );
            }

            return base.Close();
        }

        /// <inheritdoc />
        public override void SendMessage(GdsSerializableMessage message, CancellationToken token)
        {
            Logger.Debug($"{DeviceType}/{BaseName} Got message {message}");

            if (!IsOpen)
            {
                Logger.Debug($"{DeviceType}/{BaseName} Throw away message because this fake communicator is closed.");
                return;
            }

            switch (message.ReportId)
            {
                // Common messages to all GDS devices: defer to base class (see below)

                // For GDS card readers, mostly defer to base class
                case GdsConstants.ReportId.CardReaderGetBehavior:
                    OnMessageReceived(
                        new CardReaderBehavior
                        {
                            SupportedTypes = IdReaderTypes.MagneticCard,
                            VirtualReportedType = IdReaderTypes.MagneticCard,
                            IsPhysical = false,
                            IsEgmControlled = true,
                            ValidationMethod = IdValidationMethods.Host
                        });
                    break;
                case GdsConstants.ReportId.CardReaderReadCardData:
                    OnMessageReceived(
                        new CardData
                        {
                            Data = _fakeCardData, Length = _fakeCardData.Length, Type = (byte)IdReaderTracks.Track1
                        });
                    break;

                // For GDS printers
                case GdsConstants.ReportId.PrinterDefineRegion:
                    OnMessageReceived(new TransferStatus { StatusCode = GdsConstants.Success, RegionCode = true });
                    break;
                case GdsConstants.ReportId.PrinterDefineTemplate:
                    OnMessageReceived(new TransferStatus { StatusCode = GdsConstants.Success, TemplateCode = true });
                    break;
                case GdsConstants.ReportId.PrinterPrintTicket:
                case GdsConstants.ReportId.PrinterFormFeed:
                    Thread.Sleep(OneSecond);
                    OnMessageReceived(
                        new TicketPrintStatus
                        {
                            TransactionId = GetNextTransactionId(), FieldOfInterest1 = true, PrintComplete = true
                        });
                    if (message is PrintTicket printTicket)
                    {
                        _eventBus.Publish(new PrintFakeTicketEvent(printTicket.Data));
                    }

                    break;
                case GdsConstants.ReportId.PrinterRequestMetrics:
                    Thread.Sleep(TenthSecond);
                    OnMessageReceived(new PrinterMetrics { Data = string.Empty });
                    break;
                case GdsConstants.ReportId.PrinterGraphicTransferSetup:
                case GdsConstants.ReportId.PrinterFileTransfer:
                    Thread.Sleep(TenthSecond);
                    OnMessageReceived(new TransferStatus { StatusCode = GdsConstants.Success, GraphicCode = true });
                    break;
                case GdsConstants.ReportId.PrinterTicketRetract:
                    OnMessageReceived(new PrinterStatus { TransactionId = GetNextTransactionId(), TopOfForm = true });
                    break;

                // For ReelControllers
                case GdsConstants.ReportId.ReelControllerHomeReel:
                case GdsConstants.ReportId.ReelControllerNudge:
                case GdsConstants.ReportId.ReelControllerSpinReels:
                case GdsConstants.ReportId.ReelControllerTiltReels:
                case GdsConstants.ReportId.ReelControllerSetReelBrightness:
                case GdsConstants.ReportId.ReelControllerSetOffsets:
                case GdsConstants.ReportId.ReelControllerSetReelSpeed:
                case GdsConstants.ReportId.ReelControllerSetLights:
                    if (_simWindow == null)
                    {
                        _simQueue.Enqueue(message);
                    }
                    else
                    {
                        while (_simQueue.Count > 0)
                        {
                            var msg = _simQueue.Dequeue();
                            ProcessReelsDeviceMessage(msg);
                        }

                        ProcessReelsDeviceMessage(message);
                    }

                    break;
                case GdsConstants.ReportId.ReelControllerGetLightIds:
                    Logger.Debug($"{DeviceType}/{BaseName} Get light IDs");
                    var response = new ReelLightIdentifiersResponse { StartId = 1, EndId = _reelCount * LightsPerReel };
                    OnMessageReceived(response);
                    break;

                // Otherwise
                case GdsConstants.ReportId.RequestGatReport:
                    base.SendMessage(message, token);
                    if (!IsEnabled && DeviceType == DeviceType.ReelController)
                    {
                        Logger.Debug($"{DeviceType}/{BaseName} sim has {_reelCount} reels");
                        for (var reelNum = 0; reelNum < _reelCount; reelNum++)
                        {
                            OnMessageReceived(new GdsReelStatus { ReelId = reelNum + 1, Connected = true });
                            OnMessageReceived(new ReelSpinningStatus { ReelId = reelNum + 1, IdleAtStop = true });
                        }

                        OnMessageReceived(new ControllerInitializedStatus());
                    }

                    break;
                default:
                    base.SendMessage(message, token);
                    break;
            }
        }

        /// <inheritdoc />
        public override bool Configure(IComConfiguration comConfiguration)
        {
            if (comConfiguration.Name == "Fake 5 Reel")
            {
                _baseName = "Fake5";
                _reelCount = 5;
            }

            if (comConfiguration.Name == "Fake 3 Reel")
            {
                _baseName = "Fake3";
                _reelCount = 3;
            }

            comConfiguration.PortName = DefaultBaseName;
            return true;
        }

        public void ProcessReelsDeviceMessage(GdsSerializableMessage message)
        {
            try
            {
                switch (message.ReportId)
                {
                    case GdsConstants.ReportId.ReelControllerHomeReel:
                        if (message is HomeReel homeReel)
                        {
                            homeReel.Stop = Math.Max(homeReel.Stop, 0);
                            Logger.Debug(
                                $"{DeviceType}/{BaseName} Homing reel {homeReel.ReelId} to step {homeReel.Stop}");
                            _homePositions[homeReel.ReelId - 1] = Math.Max(0, homeReel.Stop);
                            _homeCommandCounter++;

                            if (_homeCommandCounter >= _reelCount)
                            {
                                _homeCommandCounter = 0;
                            }

                            var offsetStep =
                                (ushort)((_homePositions[homeReel.ReelId - 1] + _reelOffsets[homeReel.ReelId - 1] +
                                          StepsPerReel) % StepsPerReel);
                            _simWindow.HomeReel(homeReel.ReelId, offsetStep);
                        }

                        break;
                    case GdsConstants.ReportId.ReelControllerNudge:
                        if (message is Nudge nudgeReels)
                        {
                            foreach (var nudge in nudgeReels.NudgeReelData)
                            {
                                Logger.Debug(
                                    $"{DeviceType}/{BaseName} Nudging reel {nudge.ReelId} to step {nudge.Step}");
                                _simWindow.NudgeReel(
                                    nudge.ReelId,
                                    nudge.Direction == SpinDirection.Backwards,
                                    nudge.Step);
                            }
                        }

                        break;
                    case GdsConstants.ReportId.ReelControllerSpinReels:
                        if (message is SpinReels spinReels)
                        {
                            foreach (var spin in spinReels.ReelSpinData)
                            {
                                Logger.Debug(
                                    $"{DeviceType}/{BaseName} Spinning reel {spin.ReelId} to step {spin.Step}");
                                _simWindow.SpinReelToStep(
                                    spin.ReelId,
                                    spin.Direction == SpinDirection.Backwards,
                                    spin.Step);
                            }
                        }

                        break;
                    case GdsConstants.ReportId.ReelControllerTiltReels:
                        Logger.Debug($"{DeviceType}/{BaseName} Tilt reels");
                        for (var id = 1; id <= _reelCount; id++)
                        {
                            _simWindow.TiltReel(id);
                        }

                        OnMessageReceived(new TiltReelsResponse());
                        break;
                    case GdsConstants.ReportId.ReelControllerSetReelBrightness:
                        if (message is SetBrightness brightness)
                        {
                            Logger.Debug(
                                $"{DeviceType}/{BaseName} Set reel brightness {brightness.ReelId}/{brightness.Brightness}");
                            if (brightness.ReelId == 0) // 0 applies to all reels
                            {
                                for (var reel = 1; reel <= _reelCount; reel++)
                                {
                                    _simWindow.SetReelBrightness(reel, brightness.Brightness);
                                }
                            }
                            else
                            {
                                if (brightness.ReelId <= _reelCount)
                                {
                                    _simWindow.SetReelBrightness(brightness.ReelId, brightness.Brightness);
                                }
                            }
                        }

                        OnMessageReceived(new ReelLightResponse { LightsUpdated = true });
                        break;
                    case GdsConstants.ReportId.ReelControllerSetOffsets:
                        if (message is SetOffsets offsets)
                        {
                            Logger.Debug($"{DeviceType}/{BaseName} Set reel offsets");
                            _reelOffsets = offsets.ReelOffsets.Take(_reelCount).ToArray();
                        }

                        break;
                    case GdsConstants.ReportId.ReelControllerSetReelSpeed:
                        if (message is SetSpeed speeds)
                        {
                            Logger.Debug($"{DeviceType}/{BaseName} Set reel speeds");
                            foreach (var reelData in speeds.ReelSpeedData)
                            {
                                Logger.Debug($"Set reel speed to {reelData.Speed} for reel {reelData.ReelId}");
                                _simWindow.SetReelSpeed(reelData.ReelId, reelData.Speed);
                            }
                        }

                        break;
                    case GdsConstants.ReportId.ReelControllerSetLights:
                        if (message is SetLamps setLamps)
                        {
                            foreach (var lampData in setLamps.ReelLampData)
                            {
                                Logger.Debug($"{DeviceType}/{BaseName} Set lamp data {lampData.LampId}");
                                _simWindow.SetLamp(
                                    lampData.LampId,
                                    lampData.IsLampOn ? lampData.RedIntensity : (byte)0,
                                    lampData.IsLampOn ? lampData.GreenIntensity : (byte)0,
                                    lampData.IsLampOn ? lampData.BlueIntensity : (byte)0);
                            }
                        }

                        OnMessageReceived(new ReelLightResponse { LightsUpdated = true });
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Simulator error", ex);
            }
        }

        /// <summary>Handle a <see cref="FakeCardReaderEvent" />.</summary>
        /// <param name="fakeCardReaderEvent">The <see cref="FakeCardReaderEvent" /> to handle.</param>
        protected virtual void HandleEvent(FakeCardReaderEvent fakeCardReaderEvent)
        {
            _fakeCardData = fakeCardReaderEvent.CardValue;
            OnMessageReceived(
                new CardStatus
                {
                    CardPresent = fakeCardReaderEvent.Action,
                    Inserted = fakeCardReaderEvent.Action,
                    Track1 = fakeCardReaderEvent.Action,
                    Removed = !fakeCardReaderEvent.Action
                });
        }

        /// <summary>Handle a <see cref="FakePrinterEvent" />.</summary>
        /// <param name="fakePrinterEvent">The <see cref="FakePrinterEvent" /> to handle.</param>
        protected virtual void HandleEvent(FakePrinterEvent fakePrinterEvent)
        {
            OnMessageReceived(
                new PrinterStatus
                {
                    PaperInChute = fakePrinterEvent.PaperInChute,
                    PaperEmpty = fakePrinterEvent.PaperEmpty,
                    PaperLow = fakePrinterEvent.PaperLow,
                    PaperJam = fakePrinterEvent.PaperJam,
                    TopOfForm = fakePrinterEvent.TopOfForm,
                    PrintHeadOpen = fakePrinterEvent.PrintHeadOpen,
                    ChassisOpen = fakePrinterEvent.ChassisOpen,
                    TransactionId = GetNextTransactionId()
                });
        }

        /// <summary>Handle a <see cref="FakeDeviceConnectedEvent" />.</summary>
        /// <param name="fakeDeviceConnectedEvent">The <see cref="FakeDeviceConnectedEvent" /> to handle.</param>
        protected virtual void HandleEvent(FakeDeviceConnectedEvent fakeDeviceConnectedEvent)
        {
            if (fakeDeviceConnectedEvent.Type != DeviceType)
            {
                return;
            }

            if (!fakeDeviceConnectedEvent.Connected)
            {
                Close();
            }
            else
            {
                Open();
            }

            //The relevant events should be published by anyone using this event.
        }

        /// <summary>Handle a <see cref="FakeDeviceMessageEvent" />.</summary>
        /// <param name="fakeMessageEvent">The <see cref="FakeDeviceMessageEvent" /> to handle.</param>
        protected virtual void HandleEvent(FakeDeviceMessageEvent fakeMessageEvent)
        {
            OnMessageReceived(fakeMessageEvent.Message);
        }

        /// <summary>
        ///     Releases the unmanaged resources used by the Aristocrat.Monaco.Hardware.Fake.FakeCommunicator and optionally
        ///     releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     True to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus?.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void OpenReelSimulatorWindow()
        {
            var reelController = ServiceManager.GetInstance().TryGetService<IReelController>();
            if (reelController == null)
            {
                Logger.Error("Cannot open reel simulator; there is no reel controller.");
                return;
            }

            var gamesPath = _pathMapper.GetDirectory(GamesPath).FullName;
            var packagesPath = _pathMapper.GetDirectory(PackagesPath).FullName;
            var knownReels = reelController.ConnectedReels.Count;
            Logger.Debug($"Known reels: {knownReels}");

            // Avoid using same ID as any other already-running simulators
            var usedIds = new List<int> { 0 };
            var usedTitles = Process.GetProcesses()
                .Where(process => process.MainWindowTitle.Contains(SimWindowNamePartial))
                .Select(process => process.MainWindowTitle.Substring(process.MainWindowTitle.IndexOf('_') + 1));
            usedIds.AddRange(usedTitles.ToList().Select(int.Parse).ToList());
            _id = 1 + usedIds.Max();

            Execute.OnUIThread(
                () =>
                {
                    Simulation.HarkeyReels.Logger.Log += SimulatorLog;
                    _simWindow = new ReelSetWindow(_id, gamesPath, knownReels, packagesPath);
                    Logger.Debug($"Game says: {_reelCount} reels");
                    _simWindow.ReelStateChanged += SimWindowReelStateChanged;
                    _simWindow.Show();
                    _reelCount = _simWindow.ReelCount;
                });

            while (_reelCount == 0)
            {
                Task.Delay(50);
            }
        }

        private void SimulatorLog(object sender, LoggingEventArgs e)
        {
            if (e.IsError)
            {
                Logger.Error($"{sender}: {e.Text}");
            }
            else
            {
                Logger.Debug($"{sender}: {e.Text}");
            }
        }

        private void SimWindowReelStateChanged(object _, ReelDisplayEventArgs args)
        {
            Logger.Debug($"Received sim reel event {args.ReelId} {args.Step} {args.ReelState}");
            OnMessageReceived(
                new ReelSpinningStatus
                {
                    ReelId = args.ReelId,
                    IdleAtStop = args.ReelState == ReelState.Stopped,
                    Step = args.Step,
                    Spinning = args.ReelState != ReelState.Stopped
                }
            );
        }
    }
}