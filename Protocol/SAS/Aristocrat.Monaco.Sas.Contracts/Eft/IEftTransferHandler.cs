namespace Aristocrat.Monaco.Sas.Contracts.Eft
{
    using System.Collections.Generic;
    using Aristocrat.Sas.Client;

    /// <summary>
    ///     (From section 8.EFT of the SAS v5.02 document)  -
    ///     https://confy.aristocrat.com/pages/viewpage.action?pageId=159599156
    ///     <para>
    ///         Interface for all EFT Transaction/Transfer Handlers to implement. This is meant for all U and D type LPs.
    ///         The core logic and phase workflow will be handled inside EftStateController.cs and the handlers are responsible
    ///         for providing processing functionality.
    ///     </para>
    ///     <para>
    ///         They will specify what is needed to be proceeded/done once everything is verified. The verification will be
    ///         done by the EftStateController class and once needed the controller will call on the handlers to process
    ///         accordingly.
    ///     </para>
    /// </summary>
    public interface IEftTransferHandler
    {
        /// <summary>
        /// LP commands supported by the handler
        /// </summary>
        List<LongPoll> Commands { get; }

        /// <summary>
        ///     This is the first phase processing that should occur once the initial host command has been verified and we are to
        ///     continue processing. Depending on which handler/command it is the CheckTransferAmount will do different things.
        /// </summary>
        /// <returns>
        ///     Amount that can be transferred, and a bool TransferAmountExceeded that states whether the amount was exceeded.
        /// </returns>
        (ulong Amount, bool TransferAmountExceeded) CheckTransferAmount(ulong amount);

        /// <summary>
        ///     This method returns whether the EFT Transfer should stop if disabled by the Host/Site-Controller.
        ///     D type LPs should stop the transfer and U type should continue with the transfer.
        /// </summary>
        /// <returns>True if EGM should stop the transfer if disabled by Host/Site-Controller, false otherwise.</returns>
        bool StopTransferIfDisabledByHost();

        /// <summary>
        ///     This method returns the message that will be displayed to the player while EFT
        ///     Transfer is in progress. Return Message will depend on whether it is U or D type LP.
        /// </summary>
        /// <returns>A message similar to EFT Credit Transfer In/Out is in progress.</returns>
        string GetDisableString();

        /// <summary>
        ///     The finalization command that will occur once the second phase has been completed and the ack command verified. If
        ///     all goes well, this method will either add or remove credits depending on command type
        /// </summary>
        /// <returns>True if the command was successful</returns>
        bool ProcessTransfer(ulong amountToBeTransferred, int transactionNumber);
    }
}