namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     Provides a mechanism to control VBD input for games.
    /// </summary>
    public interface IRuntimeFlagHandler
    {
        /// <summary>
        ///     Notifies GDKRuntimeHost of AFT state.
        /// </summary>
        /// <param name="value">Boolean indicating whether AFT lock is present</param>
        void SetFundsTransferring(bool value);

        /// <summary>
        ///     Notifies GDKRuntimeHost of CashingOut state.
        /// </summary>
        /// <param name="value">Boolean indicating whether Platform is cashing out or not.</param>
        void SetCashingOut(bool value);

        /// <summary>
        ///     Notifies GDKRuntimeHost of ValidatingBillNote state.
        /// </summary>
        /// <param name="value">Boolean indicating whether Platform is validating a bill or voucher.</param>
        void SetValidatingBillNote(bool value);

        /// <summary>
        ///     Notifies GDKRuntimeHost of DisplayingRGDialog state.
        /// </summary>
        /// <param name="value">Boolean indicating whether Platform is displaying a RG dialog.</param>
        void SetDisplayingRGDialog(bool value);

        /// <summary>
        ///     Notifies GDKRuntimeHost of DisplayingTimeRemaining state.
        /// </summary>
        /// <param name="value">Boolean indicating whether Clock is displaying Time Remaining in Responsible Gaming session.</param>
        void SetDisplayingTimeRemaining(bool value);

        /// <summary>
        ///     Notifies GDKRuntimeHost of Time Remaining in Responsible Gaming Session.
        /// </summary>
        /// <param name="timeRemainingText">Formatted Time Remaining</param>
        void SetTimeRemaining(string timeRemainingText);

        /// <summary>
        ///     Notifies GDKRuntimeHost of DisplayingVbdOverlay state.
        /// </summary>
        /// <param name="value">Boolean indicating whether Platform is displaying an overlay on VBD.</param>
        void SetDisplayingOverlay(bool value);

        /// <summary>
        ///     Notifies GDKRuntimeHost of AwaitingPlayerSelection state.
        /// </summary>
        /// <param name="value">Boolean indicating whether Platform is awaiting player's selection.</param>
        void SetAwaitingPlayerSelection(bool value);

        /// <summary>
        ///     Notifies GDKRuntimeHost of InPlayerMenu state.
        /// </summary>
        /// <param name="value">Boolean indicating whether the player menu is being displayed.</param>
        void SetInPlayerMenu(bool value);

        /// <summary>
        ///     Notifies GDKRuntimeHost of the service button state
        /// </summary>
        /// <param name="value">The state of the service button (On or Off)</param>
        void SetServiceRequested(bool value);
    }
}