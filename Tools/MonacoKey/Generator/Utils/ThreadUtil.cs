namespace Generator.Utils
{
    using System;
    using System.Windows;
    using System.Windows.Threading;

    public static class ThreadUtil
    {
        /// <summary>Safely dispatch an action</summary>
        /// <param name="action">The action</param>
        public static void ExecuteOnUI( Action action)
        {
            Dispatcher dispatcher = Application.Current.Dispatcher;
            if (dispatcher.CheckAccess())
                action();
            else
                dispatcher.Invoke((Delegate)action);
        }
    }
}
