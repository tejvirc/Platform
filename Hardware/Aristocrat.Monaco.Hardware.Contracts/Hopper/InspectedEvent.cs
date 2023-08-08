namespace Aristocrat.Monaco.Hardware.Contracts.Hopper
{
    using System;
    using Properties;
   
    /// <summary>Definition of the Hopper InspectedEvent class.</summary>
    /// <remarks>
    ///     This event is posted by the Hopper Service when the Hopper is
    ///     connected and has responded with its information.
    /// </remarks>
    [Serializable]
    public class InspectedEvent : HopperBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="InspectedEvent" /> class.
        /// </summary>
        public InspectedEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InspectedEvent" /> class.Initializes a new instance of the
        ///     InspectedEvent class with the hopper's ID.
        /// </summary>
        /// <param name="hopperId">The associated hopper's ID.</param>
        public InspectedEvent(int hopperId)
            : base(hopperId)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{"Hopper"} {Resources.InspectionFailedText} {Resources.ClearedText}";
        }
    }
}
