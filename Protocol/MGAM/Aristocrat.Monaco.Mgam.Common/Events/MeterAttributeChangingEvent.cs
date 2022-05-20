namespace Aristocrat.Monaco.Mgam.Common.Events
{
    using Kernel;

    /// <summary>
    ///     Event when meter attribute is changing and needs to be reported to host.
    /// </summary>
    public class MeterAttributeChangingEvent : BaseEvent
    {
        /// <summary>
        ///     Create <see cref="MeterAttributeChangingEvent"/>
        /// </summary>
        /// <param name="name">Attribute name</param>
        /// <param name="amount">Attribute amount</param>
        public MeterAttributeChangingEvent(string name, long amount)
        {
            AttributeName = name;
            AttributeAmount = amount;
        }

        /// <summary>
        ///     Get the attribute name.
        /// </summary>
        public string AttributeName { get; }

        /// <summary>
        ///     Get the attribute amount
        /// </summary>
        public long AttributeAmount { get; }
    }
}
