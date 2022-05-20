namespace Aristocrat.Monaco.Application.UI.Models
{
    using System;
    using Contracts.Localization;
    using Hardware.Contracts.Reel;
    using Kernel;
    using Monaco.Localization.Properties;
    using MVVM.ViewModel;

    /// <summary>
    ///     Definition of the ReelInfoItem class.
    /// </summary>
    [CLSCompliant(false)]
    public class ReelInfoItem : BaseEntityViewModel
    {
        private const int MaximumReelSteps = 199;

        private const int MaximumReelOffset = 199; // 200 & 0 are considered the same position

        private readonly IEventBus _eventBus;

        private int _id;
        private bool _connected;
        private bool _enabled;
        private string _state;
        private string _step;
        private int _offsetSteps;
        private int _spinStep;
        private int _nudgeSteps;
        private SpinDirection _directionToSpin = SpinDirection.Forward;
        private SpinDirection _directionToNudge = SpinDirection.Forward;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ReelInfoItem" /> class.
        /// </summary>
        /// <param name="id">The reel id</param>
        /// <param name="isConnected">Indicates whether or not the reel is connected</param>
        /// <param name="isEnabled">Indicates whether or not the reel is enabled</param>
        /// <param name="state">The state of the reel</param>
        /// <param name="offset">The offset for this reel</param> 
        public ReelInfoItem(int id, bool isConnected, bool isEnabled, string state, int offset)
        {
            _eventBus = ServiceManager.GetInstance().TryGetService<IEventBus>();

            _id = id;
            _connected = isConnected;
            _enabled = isEnabled;
            _state = state;
            _spinStep = 0;
            _nudgeSteps = 1;
            _offsetSteps = offset;
        }

        public int MaxReelOffset => MaximumReelOffset;

        public int MinReelOffset => MaximumReelOffset * -1;

        public int MaxReelStep => MaximumReelSteps;

        /// <summary>
        ///     Gets or sets the id of the reel
        /// </summary>
        public int Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    RaisePropertyChanged(nameof(Name));
                }
            }
        }

        /// <summary>
        ///     Gets the name of the reel
        /// </summary>
        public string Name => string.Format(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ReelLabel), Id);

        /// <summary>
        ///     Gets or sets whether the reel is connected
        /// </summary>
        public bool Connected
        {
            get => _connected;
            set
            {
                if (_connected != value)
                {
                    _connected = value;
                    RaisePropertyChanged(nameof(Connected));
                }
            }
        }

        /// <summary>
        ///     Gets or sets whether the reel is enabled
        /// </summary>
        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    RaisePropertyChanged(nameof(Enabled));
                }
            }
        }

        /// <summary>
        ///     Gets or sets the state of the reel
        /// </summary>
        public string State
        {
            get => _state;

            set
            {
                _state = value;
                RaisePropertyChanged(nameof(State));
            }
        }

        /// <summary>
        ///     Gets or sets the step position of the reel
        /// </summary>
        public string Step
        {
            get => _step;

            set
            {
                _step = value;
                RaisePropertyChanged(nameof(Step));
            }
        }
        
        /// <summary>
        ///     Gets the number of steps to offset
        /// </summary>
        public int OffsetSteps
        {
            get => _offsetSteps;
            set
            {
                if (_offsetSteps == value)
                {
                    return;
                }

                _offsetSteps = value;
                RaisePropertyChanged(nameof(OffsetSteps));
                _eventBus?.Publish(new PropertyChangedEvent(nameof(OffsetSteps)));
            }
        }
        
        /// <summary>
        ///     Gets the step to spin to
        /// </summary>
        public int SpinStep
        {
            get => _spinStep;

            set
            {
                _spinStep = value;
                RaisePropertyChanged(nameof(SpinStep));
            }
        }

        /// <summary>
        ///     Gets or sets the direction to spin the reel
        /// </summary>
        public bool DirectionToSpin
        {
            get => _directionToSpin == SpinDirection.Forward;

            set
            {
                _directionToSpin = value ? SpinDirection.Forward : SpinDirection.Backwards;

                RaisePropertyChanged(nameof(DirectionToSpin));
                RaisePropertyChanged(nameof(DirectionToSpinText));
            }
        }

        /// <summary>
        ///     Gets the direction to spin text
        /// </summary>
        public string DirectionToSpinText => DirectionToSpin ?
            Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Forward) : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Backward);

        /// <summary>
        ///     Gets the number of steps to nudge
        /// </summary>
        public int NudgeSteps
        {
            get => _nudgeSteps;

            set
            {
                _nudgeSteps = value;
                RaisePropertyChanged(nameof(NudgeSteps));
            }
        }

        /// <summary>
        ///     Gets or sets the direction to nudge the reel
        /// </summary>
        public bool DirectionToNudge
        {
            get => _directionToNudge == SpinDirection.Forward;

            set
            {
                _directionToNudge = value ? SpinDirection.Forward : SpinDirection.Backwards;

                RaisePropertyChanged(nameof(DirectionToNudge));
                RaisePropertyChanged(nameof(DirectionToNudgeText));
            }
        }

        /// <summary>
        ///     Gets the direction to nudge text
        /// </summary>
        public string DirectionToNudgeText => DirectionToNudge ?
            Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Forward) : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Backward);

        /// <summary>
        ///     Gets or sets whether this reel is homing
        /// </summary>
        public bool IsHoming { get; set; }

        /// <summary>
        ///     Gets or sets whether this reel is spinning
        /// </summary>
        public bool IsSpinning { get; set; }

        /// <summary>
        ///     Gets or sets whether this reel is nudging
        /// </summary>
        public bool IsNudging { get; set; }
    }
}