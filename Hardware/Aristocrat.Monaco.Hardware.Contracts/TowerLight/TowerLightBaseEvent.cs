namespace Aristocrat.Monaco.Hardware.Contracts.TowerLight
{
    using System.Globalization;
    using Kernel;

    /// <summary>Definition of the TowerLightBaseEvent class.</summary>
    public abstract class TowerLightBaseEvent : BaseEvent
    {
        /// <summary>Initializes a new instance of the <see cref="TowerLightBaseEvent" /> class./// </summary>
        /// <param name="lightTier">The light tier of the tower light associated with the event.</param>
        /// <param name="flashState">The flash state of the tower light associated with the event.</param>
        protected TowerLightBaseEvent(LightTier lightTier, FlashState flashState)
        {
            LightTier = lightTier;
            FlashState = flashState;
        }

        /// <summary>Gets a value of the light tier associated with the event.</summary>
        public LightTier LightTier { get; }

        /// <summary>Gets a value of the flash status associated with the event.</summary>
        public FlashState FlashState { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} [LightTier={1}, FlashState={2}]",
                GetType().Name,
                LightTier,
                FlashState);
        }
    }
}