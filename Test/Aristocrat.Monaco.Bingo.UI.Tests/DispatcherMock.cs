namespace Aristocrat.Monaco.Bingo.UI.Tests
{
    using System;
    using Aristocrat.Monaco.Bingo.Common;
    using Moq;

    public static class DispatcherMock
    {
        /// <summary>
        ///     Gets the mock dispatcher object
        /// </summary>
        public static Mock<IDispatcher> Dispatcher { get; } = new Mock<IDispatcher>(MockBehavior.Strict);

        /// <summary>
        ///     Sets up the mock dispatcher
        /// </summary>
        public static void DispatcherSetup(Action action)
        {
            Dispatcher.Setup(x => x.Invoke(action))
                .Callback((Action a) => a());
            Dispatcher.Setup(x => x.BeginInvoke(action))
                .Callback((Action a) => a());
            Dispatcher.Setup(x => x.CheckAccess()).Returns(true);
        }
    }
}