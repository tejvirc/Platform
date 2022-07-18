namespace Aristocrat.Sas.Client.Eft
{
    /// <summary>
    ///     (From section 8.EFT of the SAS v5.02 document)  -
    ///     https://confy.aristocrat.com/pages/viewpage.action?pageId=159599156
    ///     EFT Transaction statuses that should be included in all EFT transaction responses.
    /// </summary>
    public enum TransactionStatus
    {
        OperationSuccessful = 0x00,
        EgmDoorOpen = 0x02,
        TransferAmountExceeded = 0x04,
        InvalidTransactionNumber = 0x05,
        NoCreditSwitch = 0x06,
        EgmInTiltCondition = 0x07,
        InvalidAck = 0x08,
        InGamePlayMode = 0x09,
        ContainsNonBcdData = 0x0A,
        PreviouslyCompleted = 0x0B,
        EgmDisabled = 0x0C,
        EgmOutOfService = 0x0D,
        EgmBusy = 0x0E
    }
}