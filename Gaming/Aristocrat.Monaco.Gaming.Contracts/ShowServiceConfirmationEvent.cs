namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     Definition of the ShowServiceConfirmationEvent class.
    /// </summary>
    [ProtoContract]
    public class ShowServiceConfirmationEvent : BaseEvent
    {
        /// <summary>
        ///     Determines whether we want to open or close the confirmation dialog.
        /// </summary>
        public bool Show = true;

        /// <inheritdoc />
        public override string ToString()
        {
            return GetType().Name + " " + Show;
        }
    }
}
