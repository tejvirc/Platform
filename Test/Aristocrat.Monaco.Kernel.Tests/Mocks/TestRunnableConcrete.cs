namespace Aristocrat.Monaco.Kernel.Tests.Mocks
{
    using System.Threading;

    /// <summary>
    ///     Definition of the TestRunnableConcrete class.
    /// </summary>
    public class TestRunnableConcrete : BaseRunnable
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TestRunnableConcrete" /> class.
        /// </summary>
        public TestRunnableConcrete()
        {
            ExitOnInitializeCall = true;
            ExitOnStop = true;
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the method OnStop was called
        /// </summary>
        public bool OnStopCalled { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the method OnRun was called
        /// </summary>
        public bool OnRunCalled { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the method OnInitialize was called
        /// </summary>
        public bool OnInitializeCalled { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the Runnable should exit its OnInitialize method
        /// </summary>
        public bool ExitOnInitializeCall { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the Runnable should exit its OnStop method
        /// </summary>
        public bool ExitOnStop { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the Runnable has been disposed or not
        /// </summary>
        public bool IsDisposed { get; set; }

        /// <summary>
        ///     Implemented by derived classes to execute behavior for the Runnable. It is assumed
        ///     that when this method exits the Runnable is then in the Stopped state and Stop will
        ///     be called.
        /// </summary>
        protected override void OnRun()
        {
            OnRunCalled = true;
        }

        /// <summary>
        ///     Implemented by derived classes to setup their run state
        /// </summary>
        protected override void OnInitialize()
        {
            OnInitializeCalled = true;
            while (!ExitOnInitializeCall)
            {
                Thread.Sleep(100);
            }
        }

        /// <summary>
        ///     Implemented by derived classes to execute any necessary custom stop behavior
        /// </summary>
        protected override void OnStop()
        {
            OnStopCalled = true;
            while (!ExitOnStop)
            {
                Thread.Sleep(100);
            }
        }

        /// <summary>
        ///     Disposes of this IDisposable
        /// </summary>
        /// <param name="disposing">Whether the object is being disposed</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            IsDisposed = true;
        }
    }
}