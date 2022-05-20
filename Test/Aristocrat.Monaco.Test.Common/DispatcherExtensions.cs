namespace Aristocrat.Monaco.Test.Common
{
    using System;
    using System.Windows.Threading;

    /// <summary>
    ///     Used to unit test windows that need to have message pumps
    /// </summary>
    public static class DispatcherExtentions
    {
        public static void PumpUntilDry(this Dispatcher dispatcher)
        {
            DispatcherFrame frame = new DispatcherFrame();
            dispatcher.BeginInvoke(
                new Action(() => frame.Continue = false),
                DispatcherPriority.Background);
            Dispatcher.PushFrame(frame);
        }
    }
}