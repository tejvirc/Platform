namespace Aristocrat.Monaco.Gaming.Contracts.Events
{
    using System.Collections.Generic;
    using Kernel;

    /// <summary>
    ///     The <see cref="PresentationOverrideDataChangedEvent" /> is posted when presentation override data is updated
    /// </summary>
    public class PresentationOverrideDataChangedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PresentationOverrideDataChangedEvent" /> class.
        /// </summary>
        public PresentationOverrideDataChangedEvent(IList<PresentationOverrideData> presentationOverrideData)
        {
            PresentationOverrideData = presentationOverrideData;
        }

        /// <summary>
        ///     The collection of presentation override data
        /// </summary>
        public IList<PresentationOverrideData> PresentationOverrideData { get; }
    }
}
