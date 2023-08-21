namespace Aristocrat.Monaco.Hardware.Contracts.TowerLight
{
    /// <summary>This event is posted when the tower light is turning off.</summary>
    public class TowerLightOffEvent : TowerLightBaseEvent
    {
        /// <summary>Initializes a new instance of the <see cref="TowerLightOffEvent" /> class.</summary>
        /// <param name="lightTier">The light tier of the tower light associated with the event.</param>
        /// <param name="flashState">The flash state of the tower light associated with the event.</param>
        public TowerLightOffEvent(LightTier lightTier, FlashState flashState)
            : base(lightTier, flashState)
        {
        }
    }
}