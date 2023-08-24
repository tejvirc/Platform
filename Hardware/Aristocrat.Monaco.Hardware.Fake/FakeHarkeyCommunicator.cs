namespace Aristocrat.Monaco.Hardware.Fake
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts.Communicator;
    using Contracts.IO;
    using Contracts.Reel;
    using Contracts.Reel.ControlData;
    using Contracts.Reel.Events;
    using Contracts.SharedDevice;
    using Extensions.CommunityToolkit;
    using Kernel;
    using log4net;
    using Simulation.HarkeyReels;
    using Simulation.HarkeyReels.Controls;

    public class FakeHarkeyCommunicator : IHarkeyCommunicator
    {
        private const string DefaultBaseName = "Fake";
        private const int BrightnessPercent = 30;
        private const int StepsPerReel = 200;
        private const string GamesDirectory = "/Games";
        private const string PackagesDirectory = "/Packages";
        private const string SimWindowNamePartial = "ReelLayout_";
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private static readonly TimeSpan ResetDelay = TimeSpan.FromMilliseconds(500);
        private readonly IEventBus _eventBus;
        private readonly IPathMapper _pathMapper;
        private readonly string GamesPath;
        private readonly string PackagesPath;
        private bool _disposed;
        private int[] _reelOffsets;
        private int[] _homePositions;
        private int _id;

        /// <summary>
        ///     Construct a <see cref="FakeHarkeyCommunicator" />
        /// </summary>
        public FakeHarkeyCommunicator()
            : this(
                ServiceManager.GetInstance().GetService<IPathMapper>(),
                ServiceManager.GetInstance().GetService<IEventBus>())
        {
        }

        public FakeHarkeyCommunicator(IPathMapper pathMapper, IEventBus eventBus)
        {
            _pathMapper = pathMapper ?? throw new ArgumentNullException(nameof(pathMapper));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            GamesPath = _pathMapper.GetDirectory(GamesDirectory).FullName;
            PackagesPath = _pathMapper.GetDirectory(PackagesDirectory).FullName;

            Logger.Debug("Constructed");
        }

        public ReelSetWindow ReelSimWindow { get; private set; }

        public int ReelCount => ReelSimWindow?.ReelCount ?? 0;

        /// <inheritdoc />
        public event EventHandler<ReelStatusReceivedEventArgs> StatusesReceived;

        /// <inheritdoc />
        public event EventHandler<ReelEventArgs> ReelTilted;

        /// <inheritdoc />
        public event EventHandler<ReelSpinningEventArgs> ReelSpinning;

        /// <inheritdoc />
        public event EventHandler<ReelEventArgs> ReelStopped;

        /// <inheritdoc />
        public Task Initialize()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(FakeHarkeyCommunicator));
            }

            //prevent a double subscription for the FakeDeviceConnectedEvent in the case of Open->Close->Open
            _eventBus?.UnsubscribeAll(this);

            if (!Open() && !IsOpen)
            {
                return Task.CompletedTask;
            }

            Logger.Debug($"There are {ReelCount} reels");

            _reelOffsets = new int[ReelCount];
            _homePositions = new int[ReelCount];

            //We assume the device will be opened by default
            _eventBus?.Subscribe<FakeDeviceConnectedEvent>(this, HandleEvent);

            RequestDeviceStatuses();

            return Task.CompletedTask;
        }

        public Task RequestDeviceStatuses()
        {
            var statuses = new List<ReelStatus>();

            for (var i = 1; i <= ReelCount; i++)
            {
                statuses.Add(new ReelStatus { ReelId = i, Connected = true });
            }

            StatusesReceived?.Invoke(this, new ReelStatusReceivedEventArgs(statuses));

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<bool> SetReelOffsets(params int[] offsets)
        {
            if (_reelOffsets.Length != offsets.Length)
            {
                return Task.FromResult(false);
            }

            Array.Copy(offsets, _reelOffsets, offsets.Length);

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> TiltReels()
        {
            if (ReelSimWindow == null)
            {
                return Task.FromResult(false);
            }

            for (var i = 1; i <= ReelCount; i++)
            {
                ReelSimWindow.TiltReel(i);
            }

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> NudgeReels()
        {
            if (ReelSimWindow == null)
            {
                return Task.FromResult(false);
            }

            for (var i = 1; i <= ReelCount; i++)
            {
                ReelSimWindow.NudgeReel(i, true, 1);
            }

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> NudgeReels(NudgeReelData[] nudgeData)
        {
            if (ReelSimWindow == null)
            {
                return Task.FromResult(false);
            }

            foreach (var nudge in nudgeData)
            {
                Logger.Debug("Fake Harkey Reel adapter : Nudging reel {nudge.ReelId} to step {nudge.Step}");
                ReelSimWindow.NudgeReel(nudge.ReelId, nudge.Direction == SpinDirection.Backwards, nudge.Step);
            }

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> SpinReels()
        {
            if (ReelSimWindow == null)
            {
                return Task.FromResult(false);
            }

            for (var i = 1; i <= ReelCount; i++)
            {
                ReelSimWindow.SpinReelToStep(i, true, 1);
            }

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> SpinReels(params ReelSpinData[] spinReelData)
        {
            if (ReelSimWindow == null)
            {
                return Task.FromResult(false);
            }

            foreach (var spin in spinReelData)
            {
                Logger.Debug("Fake Harkey Reel adapter : Spinning reel {spin.ReelId} to step {spin.Step}");
                ReelSimWindow.SpinReelToStep(spin.ReelId,
                                    spin.Direction == SpinDirection.Backwards,
                                    spin.Step);
            }

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> SetReelSpeed(params ReelSpeedData[] speedData)
        {
            if (ReelSimWindow == null)
            {
                return Task.FromResult(false);
            }

            Logger.Debug($"{DeviceType}/{DefaultBaseName} Set reel speeds");
            foreach (var reelData in speedData)
            {
                Logger.Debug($"Set reel speed to {reelData.Speed} for reel {reelData.ReelId}");
                ReelSimWindow.SpinReelToStep(reelData.ReelId, true, reelData.Speed);
            }

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> SetLights(params ReelLampData[] ReelLampData)
        {
            if (ReelSimWindow == null)
            {
                return Task.FromResult(false);
            }

            foreach (var lampData in ReelLampData)
            {
                Logger.Debug($"{DeviceType}/{DefaultBaseName} Set lamp data {lampData.Id}");
                ReelSimWindow.SetLamp(
                                    lampData.Id,
                                    lampData.IsLampOn ? lampData.Color.R : (byte)0,
                                    lampData.IsLampOn ? lampData.Color.G : (byte)0,
                                    lampData.IsLampOn ? lampData.Color.B : (byte)0);
            }

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> SetBrightness(IReadOnlyDictionary<int, int> brightness)
        {
            for (var reel = 1; reel <= ReelCount; reel++)
            {
                Logger.Debug($"{DeviceType}/{DefaultBaseName} Set reel brightness {reel}/{brightness[reel]}");
                ReelSimWindow.SetReelBrightness(reel, brightness[reel]);
            }
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> SetBrightness(int brightness)
        {
            Logger.Debug($"{DeviceType}/{DefaultBaseName} Set reel brightness for all reels to {brightness}");
            for (var reel = 1; reel <= ReelCount; reel++)
            {
                ReelSimWindow.SetReelBrightness(reel, brightness);
            }
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public string Manufacturer => Firmware + DeviceType;

        /// <inheritdoc />
        public string Model => Firmware + DeviceType;

        /// <inheritdoc />
        public string Firmware { get; } = "Fake";

        /// <inheritdoc />
        public string SerialNumber => Firmware;

        /// <inheritdoc />
        public bool IsOpen { get; set; }

        /// <inheritdoc />
        public int VendorId { get; }

        /// <inheritdoc />
        public int ProductId { get; }

        /// <inheritdoc />
        public int ProductIdDfu { get; }

        /// <inheritdoc />
        public string Protocol => "Fake";

        /// <inheritdoc />
        public DeviceType DeviceType { get; set; }

        /// <inheritdoc />
        public IDevice Device { get; set; }

        /// <inheritdoc />
        public string FirmwareVersion => "1.0";

        /// <inheritdoc />
        public int FirmwareCrc => -1;

        /// <inheritdoc />
        public bool IsDfuCapable => false;

        /// <inheritdoc />
        public event EventHandler<EventArgs> DeviceAttached;

        /// <inheritdoc />
        public event EventHandler<EventArgs> DeviceDetached;

        /// <inheritdoc />
        public bool Close()
        {
            Logger.Debug($"Closing Fake Harkey Simulator.");
            if (DeviceType == DeviceType.ReelController)
            {
                Execute.OnUIThread(
                    () => { ReelSimWindow?.Close(); }
                );
            }

            IsOpen = false;
            return true;
        }

        /// <inheritdoc />
        public bool Open()
        {
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
                    ReelSimWindow = new ReelSetWindow(_id, GamesPath, 0, PackagesPath);
                    ReelSimWindow.ReelStateChanged += SimWindowReelStateChanged;
                    ReelSimWindow.Show();

                    Logger.Debug($"Game says: {ReelCount} reels");

                });

            while (ReelCount == 0)
            {
                Thread.Sleep(50);
            }

            Logger.Debug($"There are {ReelCount} reels");
            IsOpen = true;
            return true;
        }

        /// <inheritdoc />
        public bool Configure(IComConfiguration comConfiguration)
        {
            return true;
        }

        /// <inheritdoc />
        public void ResetConnection()
        {
            Logger.Debug($"Resetting connection for {VendorId:X}...");
            OnDeviceDetached();
            Task.Delay(ResetDelay);
            OnDeviceAttached();
        }

        /// <inheritdoc />
        public Task<bool> HomeReels()
        {
            Logger.Debug("Homing reels");

            if (ReelSimWindow == null)
            {
                return Task.FromResult(false);
            }

            for (var i = 1; i <= ReelCount; i++)
            {
                ReelSimWindow.HomeReel(i, 0);
            }

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<bool> HomeReel(int reelId, int stop, bool resetStatus = true)
        {
            if (ReelSimWindow == null)
            {
                return Task.FromResult(false);
            }

            var reelStop = Math.Max(stop, 0);
            Logger.Debug($"Homing reel {reelId}");
            _homePositions[reelId - 1] = reelStop;

            var offsetStep =
                (ushort)((_homePositions[reelId - 1] + _reelOffsets[reelId - 1] +
                          StepsPerReel) % StepsPerReel);
            ReelSimWindow.HomeReel(reelId, offsetStep);
            ReelSimWindow.SetReelBrightness(reelId, BrightnessPercent);

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
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

        /// <summary>Executes the <see cref="DeviceAttached" /> action.</summary>
        protected void OnDeviceAttached(ReelControllerFaultedEventArgs e)
        {
            DeviceAttached?.Invoke(this, e);
        }

        /// <summary>Executes the <see cref="DeviceAttached" /> action.</summary>
        protected void OnDeviceAttached()
        {
            DeviceAttached?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Executes the <see cref="DeviceDetached" /> action.</summary>
        protected void OnDeviceDetached()
        {
            DeviceDetached?.Invoke(this, EventArgs.Empty);
        }

        private void SimWindowReelStateChanged(object _, ReelDisplayEventArgs args)
        {
            Logger.Debug($"Received sim reel event {args.ReelId} {args.Step} {args.ReelState}");

            switch (args.ReelState)
            {
                case ReelState.Tilted:
                    ReelTilted?.Invoke(this, new ReelEventArgs(args.ReelId, args.Step));
                    break;
                case ReelState.Spinning:
                    ReelSpinning?.Invoke(this, new ReelSpinningEventArgs(args.ReelId));
                    break;
                case ReelState.Stopped:
                    ReelStopped?.Invoke(this, new ReelEventArgs(args.ReelId, args.Step));
                    break;
            }
        }

        /// <summary>Handle a <see cref="FakeDeviceConnectedEvent" />.</summary>
        /// <param name="fakeDeviceConnectedEvent">The <see cref="FakeDeviceConnectedEvent" /> to handle.</param>
        protected virtual void HandleEvent(FakeDeviceConnectedEvent fakeDeviceConnectedEvent)
        {
            Logger.Debug($"Fake Device Connected event.");

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
    }
}