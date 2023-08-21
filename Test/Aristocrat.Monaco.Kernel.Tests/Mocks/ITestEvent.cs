namespace Aristocrat.Monaco.Kernel.Tests.Mocks
{
    /// <summary>
    ///     Definition of the ITestEvent interface.
    /// </summary>
    public interface ITestEvent : IEvent
    {
        /// <summary>
        ///     Gets or sets a number for testing purposes.
        /// </summary>
        int ANumber { get; set; }
    }
}