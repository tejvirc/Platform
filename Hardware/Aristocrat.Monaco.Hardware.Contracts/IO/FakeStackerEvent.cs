namespace Aristocrat.Monaco.Hardware.Contracts.IO
{
    using Kernel;

    /// <summary>
    ///     Event for simulating fake stacker device
    /// </summary>
    public class FakeStackerEvent : BaseEvent
    {
        /// <summary></summary>
        public bool Fault;

        /// <summary></summary>
        public bool Jam;

        /// <summary></summary>
        public bool Full;

        /// <summary></summary>
        public bool Disconnect;
    }
}