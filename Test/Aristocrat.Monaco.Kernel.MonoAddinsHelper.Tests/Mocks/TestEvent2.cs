namespace Aristocrat.Monaco.Kernel.Tests.Mocks
{
    /// <summary>
    ///     Definition of the TestEvent2 class.
    /// </summary>
    public class TestEvent2 : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the TestEvent2 class with a specific id
        /// </summary>
        public TestEvent2()
            : this(0)
        {
            // DO NOTHING
        }

        /// <summary>
        ///     Initializes a new instance of the TestEvent2 class with a specific id
        /// </summary>
        /// <param name="testId">The id to use</param>
        public TestEvent2(int testId)
        {
            TestId = testId;
        }

        /// <summary>
        ///     Gets the id used for testing event morphing
        /// </summary>
        public int TestId { get; private set; }
    }
}