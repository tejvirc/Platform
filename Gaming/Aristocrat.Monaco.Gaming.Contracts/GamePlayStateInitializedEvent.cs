namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System.Globalization;
    using Kernel;

    /// <summary>
    ///     Indicates GamePlayState initialized.
    /// </summary>
    public class GamePlayStateInitializedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes new Instance of the <see cref="GamePlayStateInitializedEvent" /> class.
        /// </summary>
        /// <param name="playState">State with which GamePlay state is initialized.</param>
        public GamePlayStateInitializedEvent(PlayState playState)
        {
            CurrentState = playState;
        }

        /// <summary>
        ///     Gets the current state.
        /// </summary>
        public PlayState CurrentState { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                $"{GetType().Name} [Timestamp={Timestamp}, CurrentState={CurrentState}]");
        }
    }
}