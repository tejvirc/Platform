namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Hardware.Contracts.Reel;
    using Hardware.Contracts.Reel.Capabilities;
    using Hardware.Contracts.Reel.ControlData;
    using Monaco.Common;

    /// <summary>
    ///     The MechanicalReelsLightAnimationTestViewModel class
    /// </summary>
    public class MechanicalReelsLightAnimationTestViewModel : INotifyPropertyChanged
    {
        private const string SampleLightShowName = "SampleLightShow";
        private const string AllTag = "ALL";

        private readonly IReelAnimationCapabilities _animationCapabilities;
        private readonly IReelController _reelController;

        private bool _testActive;

        /// <summary>
        ///     Instantiates a new instance of the MechanicalReelsLightAnimationTestViewModel class
        /// </summary>
        /// <param name="reelController">The reel controller</param>
        public MechanicalReelsLightAnimationTestViewModel(IReelController reelController)
        {
            _reelController =
                reelController ?? throw new ArgumentNullException(nameof(reelController));

            if (_reelController.HasCapability<IReelAnimationCapabilities>())
            {
                _animationCapabilities = _reelController.GetCapability<IReelAnimationCapabilities>();
            }
        }

        /// <summary>
        ///     Occurs when a property is changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Gets or sets a value indicating of the test is active
        /// </summary>
        public bool TestActive
        {
            get => _testActive;

            set
            {
                if (_testActive == value)
                {
                    return;
                }
                
                _testActive = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TestActive)));

                if (value)
                {
                    StartTest(SampleShows.ElementAt(SelectedLightShowIndex)).FireAndForget();
                }
                else
                {
                    CancelTest();
                }
            }
        }

        /// <summary>
        ///     Gets the collection of sample light shows
        /// </summary>
        // TODO: This needs updated when we create the test light show
        public IReadOnlyCollection<string> SampleShows { get; } = new[]
        {
            SampleLightShowName
        };

        /// <summary>
        ///     Get the selected light show index
        /// </summary>
        // TODO: This needs updated to a getter/setter when we have more light show, or removed is we only have one
        public int SelectedLightShowIndex => 0;

        /// <summary>
        ///     Cancels the light show test
        /// </summary>
        public void CancelTest()
        {
            if (!_reelController.Connected || _animationCapabilities is null)
            {
                return;
            }

            _animationCapabilities.StopAllLightShows();
            TestActive = false;
        }

        private async Task StartTest(string lightShowName)
        {
            if (!_reelController.Connected || _animationCapabilities is null)
            {
                return;
            }

            var lightShow = new LightShowData(-1, lightShowName, AllTag, ReelConstants.RepeatForever, -1);

            await _animationCapabilities.PrepareAnimation(lightShow);
            await _animationCapabilities.PlayAnimations();
        }
    }
}
