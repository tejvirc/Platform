namespace Aristocrat.Monaco.Hhr.UI.Consumers
{
    using System;
    using Gaming.Contracts.Events;
    using Kernel;
    using Services;
    using Menu;
    using Views;
    using System.Windows;
    using Gaming.Contracts;

    /// <summary>
    ///     Consumer for the <see cref="AdditionalInfoButtonPressedEvent" /> event.
    /// </summary>
    public class RaceInfoButtonConsumer : Consumes<AdditionalInfoButtonPressedEvent>
    {
        private HHRHostPageView _raceInfoView;
        private readonly IEventBus _eventBus;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RaceInfoButtonConsumer" /> class.
        /// </summary>
        public RaceInfoButtonConsumer(
            IEventBus eventBus,
            IMenuAccessService raceInfoView)
            : base(eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            _eventBus.Subscribe<GameProcessExitedEvent>(this, Handle);
            _eventBus.Subscribe<GameInitializationCompletedEvent>(this, Handle);

            Application.Current.Dispatcher.Invoke(() => { _raceInfoView = new HHRHostPageView(raceInfoView); });
        }

        private void Handle(GameProcessExitedEvent theEvent)
        {
            UiProperties.GameLoaded = false;
            Application.Current.Dispatcher.Invoke(() => { _raceInfoView.Hide(); });
        }

        private void Handle(GameInitializationCompletedEvent theEvent)
        {
            UiProperties.GameLoaded = true;
            Application.Current.Dispatcher.Invoke(() => { _raceInfoView.UnHide(); });
        }

        /// <inheritdoc />
        public override void Consume(AdditionalInfoButtonPressedEvent theEvent)
        {
            Application.Current.Dispatcher.Invoke(() => { _raceInfoView.Show(Command.PreviousResults); });
        }
    }
}