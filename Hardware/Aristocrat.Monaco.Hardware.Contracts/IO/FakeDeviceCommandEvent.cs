namespace Aristocrat.Monaco.Hardware.Contracts.IO
{
    using Gds;
    using Kernel;

    /// <summary>
    ///     Event for shuttling fake device messages to the fake communicator
    /// </summary>
    public class FakeDeviceMessageEvent : BaseEvent
    {
        /// <summary>
        ///     Message to shuttle
        /// </summary>
        public GdsSerializableMessage Message;
    }
}