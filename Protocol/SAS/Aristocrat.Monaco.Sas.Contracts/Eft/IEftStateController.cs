namespace Aristocrat.Monaco.Sas.Contracts.Eft
{
    using Aristocrat.Sas.Client.Eft;

    /// <summary>
    ///     (From section 8.EFT of the SAS v5.02 document)  -
    ///     https://confy.aristocrat.com/pages/viewpage.action?pageId=159599156
    ///     Interface for the Controller class that will handle all the multi-phase handshake logic for the EFT Transaction
    ///     handler classes through a state machine.
    ///     The EftStateController class is responsible for the workflow control off all the D and U type Long Polls.
    ///     All the D and U type LPs will interact with this singleton controller.
    ///     It uses a state machine to keep track of which phase of the 2 part handshake it is with the host and as well as
    ///     handle any validation logic.
    ///     The details of how to actually handle the incoming command will be handled by the specific handlers themselves.
    ///     This interface and class was written based on EFT SAS requirements v5.02.
    /// </summary>
    public interface IEftStateController
    {
        /// <summary>
        ///     Handles the EFT transaction command that the SAS host has requested this EGM.
        /// </summary>
        /// <param name="data">The SAS EFT Transaction host command to handle.</param>
        /// <param name="handler">The Handler handling this command and making this call.</param>
        /// <returns>Response to be sent back to the host.</returns>
        EftTransactionResponse Handle(EftTransferData data, IEftTransferHandler handler);

        /// <summary>
        /// On start-up, EFT LP of type D and U handlers trigger recovery from state controller.
        /// </summary>
        /// <param name="handler">The Handler triggering the recovery.</param>
        void RecoverIfRequired(IEftTransferHandler handler);
    }
}