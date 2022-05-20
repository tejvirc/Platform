namespace Aristocrat.Monaco.Kernel.Tests.Mocks
{
    using System;

    /// <summary>Definition of the TestEvent class.</summary>
    [Serializable]
    public class TestEvent : BaseEvent
    {
        /// <summary>Gets or sets a number for testing purposes.</summary>
        public int ANumber { get; set; }
    }
}