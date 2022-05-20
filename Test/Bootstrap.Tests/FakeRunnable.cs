namespace Aristocrat.Monaco.Kernel
{
    using System.Threading;

    /// <summary>
    ///     Definition of the FakeRunnable class.
    /// </summary>
    public class FakeRunnable : BaseRunnable
    {
        /// <summary>
        ///     Called when runnable is initialized
        /// </summary>
        protected override void OnInitialize()
        {
            // Intentionally empty.
        }

        /// <summary>
        ///     Called when runnable is started
        /// </summary>
        protected override void OnRun()
        {
            while (RunState == RunnableState.Running)
            {
                Thread.Sleep(0);
            }
        }

        /// <summary>
        ///     Called when runnable is stopped
        /// </summary>
        protected override void OnStop()
        {
            // Intentionally empty.
        }
    }
}