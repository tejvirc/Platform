namespace Aristocrat.Monaco.Hardware.Contracts.Reel
{
    /// <summary>
    ///     A light status
    /// </summary>
    public class LightStatus
    {
        /// <summary>
        ///     Creates an instance of <see cref="LightStatus"/>
        /// </summary>
        /// <param name="lightId">The light Id for this event</param>
        /// <param name="faulted">Is the light faulted?</param>
        public LightStatus(int lightId, bool faulted)
        {
            LightId = lightId;
            Faulted = faulted;
        }

        /// <summary>
        ///     Gets or sets the light id for the status
        /// </summary>
        public int LightId { get; set; }

        /// <summary>
        ///     Gets or sets whether or not the light is faulted
        /// </summary>
        public bool Faulted { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{nameof(LightStatus)} {{ {nameof(LightId)} = {LightId}, {nameof(Faulted)} = {Faulted} }}";
        }
    }
}
