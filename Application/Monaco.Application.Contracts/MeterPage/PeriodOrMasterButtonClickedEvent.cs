namespace Aristocrat.Monaco.Application.Contracts.MeterPage
{
    using Kernel;

    /// <summary>
    ///     Published when the Period/Master button is clicked in the Meters Audit Menu page
    /// </summary>
    public class PeriodOrMasterButtonClickedEvent : BaseEvent
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public PeriodOrMasterButtonClickedEvent(bool masterClicked)
        {
            MasterClicked = masterClicked;
        }

        /// <summary>
        ///     True for Master, False for Period
        /// </summary>
        public bool MasterClicked { get; }
    }
}
