﻿namespace Aristocrat.Monaco.Bingo.Common
{
    using System;
    using System.Windows;
    using System.Windows.Threading;

    /// <summary>
    ///     Wraps the Application dispatcher behavior to allow unit tests to
    ///     mock the dispatcher so we don't require an Application to run unit tests.
    /// </summary>
    public class DispatcherWrapper : IDispatcher
    {
        private readonly Dispatcher _dispatcher;

        public DispatcherWrapper()
        {
            _dispatcher = Application.Current.Dispatcher;
        }

        /// <inheritdoc />
        public void Invoke(Action callback)
        {
            _dispatcher.Invoke(callback);
        }

        /// <inheritdoc />
        public void BeginInvoke(Action action)
        {
            _dispatcher.BeginInvoke(action);
        }

        /// <inheritdoc />
        public bool CheckAccess()
        {
            return _dispatcher.CheckAccess();
        }
    }
}