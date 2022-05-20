namespace Aristocrat.Monaco.Application.Tests
{
    using Contracts.OperatorMenu;
    using Kernel;
    using Vgt.Client12.Application.OperatorMenu;

    /// <summary>
    ///     A test operator menu addin implementation for use in the
    ///     OperatorMenuLauncherUnitTest.
    /// </summary>
    public class TestOperatorMenu : IOperatorMenu
    {
        public int ActivateCount { get; set; }
        public int CloseCount { get; set; }
        public int ShowCount { get; set; }
        public bool IsShowing { get; set; }

        /// <summary>
        ///     Command to display the operator menu now.
        /// </summary>
        public void Show()
        {
            IsShowing = true;
            ShowCount++;
            ServiceManager.GetInstance()
                .GetService<IEventBus>()
                .Publish(new OperatorMenuEnteredEvent());
        }

        /// <summary>
        ///     Command to hide the operator menu now.
        /// </summary>
        public void Hide()
        {
        }

        /// <summary>
        ///     Command to take down the operator menu now.
        /// </summary>
        public void Close()
        {
            IsShowing = false;
            CloseCount++;
        }

        public void Activate()
        {
            ActivateCount++;
        }
    }
}