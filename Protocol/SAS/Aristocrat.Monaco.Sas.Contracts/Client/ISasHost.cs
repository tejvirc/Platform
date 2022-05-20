namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Hardware.Contracts.SerialPorts;
    using Kernel;
    using Protocol.Common.Storage.Entity;

    /// <summary>
    ///     Enum containing the types of raw communication logging supported through the operator menu.
    /// </summary>
    public enum RawCommunicationDiagnosticsType : byte
    {
        /// <summary>
        ///     Raw Communication logging is disabled.
        /// </summary>
        Disabled,

        /// <summary>
        ///     Raw communication logging is enabled with no filtering.
        /// </summary>
        EnabledFull,

        /// <summary>
        ///     Raw communication logging is enabled but filtering out general poll requests and 0x00 responses.
        /// </summary>
        EnabledIgnoreGeneralPoll
    }

    /// <summary>
    ///     Interface for interacting with the SasClient component.
    /// </summary>
    public interface ISasHost : IDisposable
    {
        /// <summary>
        ///     Gets or sets a value indicating whether Client1 handles general control messages
        /// </summary>
        bool Client1HandlesGeneralControl { get; }

        /// <summary>
        ///     Register the handlers
        /// </summary>
        /// <param name="exceptionHandler">An instance of <see cref="ISasExceptionHandler"/></param>
        /// <param name="longPollHandlers">The list of long poll handlers</param>
        /// <param name="validationHandler">The validation handler</param>
        /// <param name="ticketPrintedHandler">The ticket printed handler</param>
        /// <param name="aftRegistrationProvider">The AFT Registration Provider</param>
        /// <param name="sasHandPayCommittedHandler">The Handpay committed handler</param>
        /// <param name="aftTransferProvider">The Aft Transfer Provider</param>
        /// <param name="sasVoucherInProvider">The Sas Voucher in Provider</param>

        void RegisterHandlers(
            ISasExceptionHandler exceptionHandler,
            IReadOnlyCollection<ISasLongPollHandler> longPollHandlers,
            IValidationHandler validationHandler,
            ISasTicketPrintedHandler ticketPrintedHandler,
            IAftRegistrationProvider aftRegistrationProvider,
            ISasHandPayCommittedHandler sasHandPayCommittedHandler,
            IAftTransferProvider aftTransferProvider,
            ISasVoucherInProvider sasVoucherInProvider);

        /// <summary>
        ///     Get whether or not the host is online for the requested SasGroup type
        /// </summary>
        /// <param name="sasGroup">The group to check the host against</param>
        /// <returns>Whether or not the requested host is online</returns>
        bool IsHostOnline(SasGroup sasGroup);

        /// <summary>
        ///     Initializes the SasBridge object.
        /// </summary>
        /// <param name="disableProvider">The disable provider</param>
        /// <param name="unitOfWorkFactory">The persistence manager</param>
        void Initialize(ISasDisableProvider disableProvider, IUnitOfWorkFactory unitOfWorkFactory);

        /// <summary>
        ///     Inject dependencies into SasHost
        /// </summary>
        /// <param name="propertiesManager">The properties manager</param>
        /// <param name="eventBus">The event bus</param>
        /// <param name="serialPortService">The serial port service</param>
        void InjectDependencies(
            IPropertiesManager propertiesManager,
            IEventBus eventBus,
            ISerialPortsService serialPortService);

        /// <summary>
        ///     Sets configuration options for Sas, obtained from SasPropertiesProvider.
        /// </summary>
        /// <param name="configuration">Configuration object.</param>
        void SetConfiguration(SasSystemConfiguration configuration);

        /// <summary>
        ///     Starts the blue sas event system.  This must be called in order
        ///     for the engine to process events.
        /// </summary>
        /// <returns>Whether starting the event system was successful.</returns>
        bool StartEventSystem();

        /// <summary>
        ///     Stops the blue sas event system.  This should be called on system shutdown.
        /// </summary>
        void StopEventSystem();

        /// <summary>
        ///     Turns Legacy Bonusing off or on.
        /// </summary>
        /// <param name="isEnabled">Whether or not legacy bonusing should be enabled.</param>
        void SetLegacyBonusEnabled(bool isEnabled);

        /// <summary>
        ///     Sets the receipt status of the current aft transfer.
        /// </summary>
        /// <param name="receiptStatus">The current aft transfer's receipt status.</param>
        void SetAftReceiptStatus(ReceiptStatus receiptStatus);

        /// <summary>
        ///     Determines if the ticket redemption is currently enabled.
        /// </summary>
        /// <returns>True is the ticket redemption is enabled, false otherwise.</returns>
        bool IsRedemptionEnabled();

        /// <summary>
        ///     Sends a ticket in validation request to Sas (exception 67).
        /// </summary>
        /// <param name="transaction">The ticket transaction.</param>
        Task<TicketInInfo> ValidateTicketInRequest(VoucherInTransaction transaction);

        /// <summary>
        ///     Sends a ticket transfer complete to Sas (exception 68).
        /// </summary>
        /// <param name="accountType">The account type of the accepted voucher.</param>
        void TicketTransferComplete(AccountType accountType);

        /// <summary>
        ///     Sends a ticket transfer complete to Sas (exception 68) and sets the
        ///     ticket status to rejected
        /// </summary>
        /// <param name="barcode">Barcode on ticket.</param>
        /// <param name="exceptionCode">Failure exception code.</param>
        /// <param name="transactionId">Transaction Id of ticket.</param>
        void TicketTransferFailed(string barcode, int exceptionCode, long transactionId);

        /// <summary>
        ///     Notifies SAS that the AFT transfer has completed successfully
        /// </summary>
        /// <param name="data">The Aft data for the current pending transaction.</param>
        void AftTransferComplete(AftData data);

        /// <summary>
        ///     Notifies SAS that the AFT transfer has completed with a failure
        /// </summary>
        /// <param name="data">The Aft data for the current pending transaction.</param>
        /// <param name="errorCode">The failure code</param>
        void AftTransferFailed(AftData data, AftTransferStatusCode errorCode);

        /// <summary>
        ///     Turns Aft Out off or on
        /// </summary>
        /// <param name="isEnabled">Whether or not Aft Out should be enabled.</param>
        void SetAftOutEnabled(bool isEnabled);

        /// <summary>
        ///     Turns Aft In off or on
        /// </summary>
        /// <param name="isEnabled">Whether or not Aft In should be enabled.</param>
        void SetAftInEnabled(bool isEnabled);

        /// <summary>
        ///     Sends an Aft transfer complete (exception 69) for Aft bonus to Sas.
        /// </summary>
        /// <param name="data">The Aft data for the current pending transaction.</param>
        void AftBonusAwarded(AftData data);

        /// <summary>
        ///     Checks whether or not SAS can validate the ticket out request
        /// </summary>
        /// <param name="amount">The amount to check if we can validate</param>
        /// <param name="ticketType">The ticket type to check if we can validate</param>
        /// <returns>Whether or not SAS can validate the ticket out request</returns>
        bool CanValidateTicketOutRequest(ulong amount, TicketType ticketType);

        /// <summary>
        ///     Sends a ticket out validation request to Sas (exception 57 for system validation).
        /// </summary>
        /// <param name="amount">Amount on ticket.</param>
        /// <param name="ticketType">Type of ticket for this request.</param>
        Task<TicketOutInfo> ValidateTicketOutRequest(ulong amount, TicketType ticketType);

        /// <summary>
        ///     Sends the handpay information.
        /// </summary>
        /// <param name="amount">The amount that will be designated as handpay.</param>
        /// <param name="type">The type of handpay.</param>
        Task<TicketOutInfo> ValidateHandpayRequest(ulong amount, HandPayType type);

        /// <summary>
        ///     Makes Sas reset the ticket out information because VoucherOutHandler has given up.
        /// </summary>
        void VoucherOutCanceled();

        /// <summary>
        ///     Sends a ticket printed to Sas (exception 3D (as in hexadecimal, not
        ///     as in number of dimensions)).
        /// </summary>
        void TicketPrinted();

        /// <summary>
        ///     Informs Sas that a handpay has been validated and a receipt will not be printed
        ///     (this will send an exception 3E).
        /// </summary>
        void HandPayValidated();

        /// <summary>
        ///     Called when the client has calculated the signature for the ROM.
        /// </summary>
        /// <param name="signature">The ROM signature.</param>
        /// <param name="clientNumber">The client number for this calculated signature</param>
        void RomSignatureCalculated(ushort signature, byte clientNumber);

        /// <summary>
        ///     Called when the platform has completed an Aft lock
        /// </summary>
        void AftLockCompleted();

        /// <summary>
        ///     Provides SAS client diagnostics
        /// </summary>
        /// <param name="clientNumber"></param>
        /// <returns>Client number for Diagnostics</returns>
        SasDiagnostics GetSasClientDiagnostics(int clientNumber);
    }
}