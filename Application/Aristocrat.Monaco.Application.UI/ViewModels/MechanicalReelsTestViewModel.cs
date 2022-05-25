namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Contracts.HardwareDiagnostics;
    using Contracts.Localization;
    using Hardware.Contracts.Reel;
    using Kernel;
    using log4net;
    using Models;
    using Monaco.Localization.Properties;
    using MVVM.Command;
    using Simulation.HarkeyReels;

    [CLSCompliant(false)]
    public class MechanicalReelsTestViewModel : INotifyPropertyChanged
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const int DefaultSpinSpeed = 1; // TODO: Get this value from the Adapter
        private const int DefaultNudgeDelay = 0;

        private readonly IReelController _reelController;
        private readonly int _maxSupportedReels;
        private readonly IEventBus _eventBus;
        private readonly Action _updateScreenCallback;

        private bool _homeEnabled = true;
        private bool _spinEnabled;
        private bool _nudgeEnabled;
        private bool _allReelsIdle;
        private bool _allReelsIdleUnknown;
        private bool _checkHasFault = true;

        private ObservableCollection<ReelInfoItem> _reelInfo;

        public MechanicalReelsTestViewModel(
            IReelController reelController,
            IEventBus eventBus,
            int maxSupportedReels,
            ObservableCollection<ReelInfoItem> reelInfo,
            Action updateScreenCallback)
        {
            _reelController = reelController;
            _eventBus = eventBus;
            _reelInfo = reelInfo;
            _updateScreenCallback = updateScreenCallback;
            _maxSupportedReels = maxSupportedReels;

            HomeCommand = new ActionCommand<object>(_ => HomeReels());
            SpinCommand = new ActionCommand<object>(_ => SpinReels());
            NudgeCommand = new ActionCommand<object>(_ => NudgeReels());
        }

        public IReelDisplayControl ReelsSimulation { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand HomeCommand { get; }

        public bool HomeEnabled
        {
            get => _homeEnabled && (AllReelsIdle || AllReelsIdleUnknown)
                   || HasFault
                   || _reelController.LogicalState == ReelControllerState.Disabled;

            set
            {
                if (_homeEnabled == value)
                {
                    return;
                }

                _homeEnabled = value;
                RaisePropertyChanged(nameof(HomeEnabled));
            }
        }

        public ICommand NudgeCommand { get; }

        public bool NudgeEnabled
        {
            get => _nudgeEnabled && AllReelsIdle && !HasFault;

            set
            {
                if (_nudgeEnabled == value)
                {
                    return;
                }

                _nudgeEnabled = value;
                RaisePropertyChanged(nameof(NudgeEnabled));
            }
        }

        public ObservableCollection<ReelInfoItem> ReelInfo
        {
            get => _reelInfo;

            set
            {
                _reelInfo = value;
                RaisePropertyChanged(nameof(ReelInfo));
            }
        }

        public ICommand SpinCommand { get; }

        public bool SpinEnabled
        {
            get => _spinEnabled && AllReelsIdle && !HasFault;

            set
            {
                if (_spinEnabled == value)
                {
                    return;
                }

                _spinEnabled = value;
                RaisePropertyChanged(nameof(SpinEnabled));
            }
        }

        public bool HasFault => _reelController.LogicalState == ReelControllerState.Tilted && _checkHasFault;

        public bool AnyReelEnabled { get; set; }

        public void UpdateScreen()
        {
            RaisePropertyChanged(nameof(ReelInfo));

            AllReelsIdle = ReelInfo.All(x => x.State == Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Idle));
            AllReelsIdleUnknown = ReelInfo.All(x => x.State == Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ReelController_IdleUnknown));
            AnyReelEnabled = ReelInfo.Any(x => x.Enabled);

            RaisePropertyChanged(nameof(HomeEnabled));
            RaisePropertyChanged(nameof(NudgeEnabled));
            RaisePropertyChanged(nameof(SpinEnabled));
        }

        private async void ExecuteSpinCommand(IEnumerable<ISpinData> spinData)
        {
            HomeEnabled = false;
            NudgeEnabled = false;
            SpinEnabled = false;
            AllReelsIdle = false;
            AllReelsIdleUnknown = false;

            ClearReelsSteps();

            await Task.Run(() =>
            {
                if (spinData is IEnumerable<NudgeReelData> nudges)
                {
                    var nudgeReelData = nudges.ToArray();
                    if (AnyReelEnabled)
                    {
                        _reelController.NudgeReel(nudgeReelData);
                    }

                    for (var i = 1; i <= nudgeReelData.Length; i++)
                    {
                        var nudgeData = nudgeReelData[i - 1];
                        try
                        {
                            ReelsSimulation.NudgeReel(nudgeData.ReelId, nudgeData.Direction == SpinDirection.Backwards, nudgeData.Step);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"Simulator error: {ex.Message}");
                        }
                    }
                }
                else if (spinData is IEnumerable<ReelSpinData> spins)
                {
                    var reelSpinData = spins.ToArray();
                    if (AnyReelEnabled)
                    {
                        _reelController.SpinReels(reelSpinData);
                    }

                    for (var i = 1; i <= reelSpinData.Length; i++)
                    {
                        var spin = reelSpinData[i - 1];
                        try
                        {
                            ReelsSimulation.SpinReelToStep(spin.ReelId, spin.Direction == SpinDirection.Backwards, spin.Step);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"Simulator error: {ex.Message}");
                        }
                    }
                }

                HomeEnabled = true;
                SpinEnabled = true;
                NudgeEnabled = true;
                _updateScreenCallback();
            });
        }

        private ReelInfoItem GetActiveReel(int reel) => ReelInfo.First(o => o.Id == reel);

        private bool AllReelsIdle
        {
            get => _allReelsIdle;
            set
            {
                if (_allReelsIdle == value)
                {
                    return;
                }

                _allReelsIdle = value;
                RaisePropertyChanged(nameof(NudgeEnabled));
                RaisePropertyChanged(nameof(SpinEnabled));
                RaisePropertyChanged(nameof(HomeEnabled));
            }
        }

        private bool AllReelsIdleUnknown
        {
            get => _allReelsIdleUnknown;
            set
            {
                if (_allReelsIdleUnknown == value)
                {
                    return;
                }

                _allReelsIdleUnknown = value;
                RaisePropertyChanged(nameof(HomeEnabled));
            }
        }

        private async void HomeReels()
        {
            if (!_reelController.Connected)
            {
                return;
            }

            _eventBus.Publish(new HardwareDiagnosticTestStartedEvent(HardwareDiagnosticDeviceCategory.MechanicalReels));

            for (var i = 1; i <= _maxSupportedReels; ++i)
            {
                if (IsReelActive(i))
                {
                    var activeReel = GetActiveReel(i);
                    if (activeReel.Enabled && activeReel.Connected)
                    {
                        activeReel.IsHoming = true;
                        activeReel.IsSpinning = false;
                        activeReel.IsNudging = false;
                    }
                }
            }

            _checkHasFault = false;
            HomeEnabled = false;
            SpinEnabled = false;
            NudgeEnabled = false;
            AllReelsIdle = false;
            AllReelsIdleUnknown = false;

            ClearReelsSteps();
            var tasks = new List<Task>
            {
                Task.Run(() => _reelController.HomeReels()),
                Task.Run(
                    () =>
                    {
                        for (var i = 1; i <= ReelsSimulation.ReelCount; i++)
                        {
                            try
                            {
                                ReelsSimulation.HomeReel(i, _reelController.ReelHomeStops[i]);
                            }
                            catch (Exception ex)
                            {
                                Logger.Error($"Simulator error: {ex.Message}");
                            }
                        }
                    })
            };

            await Task.WhenAll(tasks);

            HomeEnabled = true;
            SpinEnabled = true;
            NudgeEnabled = true;
            _checkHasFault = true;

            _updateScreenCallback();

            _eventBus.Publish(
                new HardwareDiagnosticTestFinishedEvent(HardwareDiagnosticDeviceCategory.MechanicalReels));
        }

        private bool IsReelActive(int reel) => ReelInfo.Any(o => o.Id == reel);

        private void NudgeReels()
        {
            var data = new List<NudgeReelData>();

            for (var i = 1; i <= _maxSupportedReels; ++i)
            {
                if (!IsReelActive(i))
                {
                    continue;
                }

                var activeReel = GetActiveReel(i);
                if (!activeReel.Enabled || !activeReel.Connected || activeReel.NudgeSteps <= 0)
                {
                    continue;
                }

                data.Add(
                    new NudgeReelData(
                        i,
                        activeReel.DirectionToNudge ? SpinDirection.Forward : SpinDirection.Backwards,
                        activeReel.NudgeSteps,
                        delay: DefaultNudgeDelay));

                activeReel.IsHoming = false;
                activeReel.IsSpinning = false;
                activeReel.IsNudging = true;
            }

            ExecuteSpinCommand(data);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RaisePropertyChanged(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                OnPropertyChanged(propertyName);
            }
        }

        private void SpinReels()
        {
            var data = new List<ReelSpinData>();

            for (var i = 1; i <= _maxSupportedReels; ++i)
            {
                if (!IsReelActive(i))
                {
                    continue;
                }

                var activeReel = GetActiveReel(i);
                if (!activeReel.Enabled || !activeReel.Connected)
                {
                    continue;
                }

                data.Add(
                    new ReelSpinData(
                        i,
                        activeReel.DirectionToSpin ? SpinDirection.Forward : SpinDirection.Backwards,
                        DefaultSpinSpeed,
                        activeReel.SpinStep));

                activeReel.IsHoming = false;
                activeReel.IsSpinning = true;
                activeReel.IsNudging = false;
            }

            UpdateScreen();
            ExecuteSpinCommand(data);
        }

        private void ClearReelsSteps()
        {
            for (var i = 1; i <= _maxSupportedReels; ++i)
            {
                if (IsReelActive(i))
                {
                    var activeReel = GetActiveReel(i);
                    if (activeReel.Enabled && activeReel.Connected)
                    {
                        activeReel.Step = string.Empty;
                    }
                }
            }

            RaisePropertyChanged(nameof(ReelInfo));
        }
    }
}