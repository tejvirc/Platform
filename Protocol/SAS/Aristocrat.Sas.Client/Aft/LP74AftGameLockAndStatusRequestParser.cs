namespace Aristocrat.Sas.Client.Aft
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the AFT Game Lock and Status Request Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           74        AFT lock and status request
    /// Lock Code        1           nn        00 - Request lock
    ///                                        80 - Cancel lock or pending lock request
    ///                                        FF - Interrogate current status only
    /// Transfer Condition 1       00-FF       Bit  For bit = 1, lock when condition true
    ///                                         0   Transfer to gaming machine OK
    ///                                         1   Transfer from gaming machine OK
    ///                                         2   Transfer to printer OK
    ///                                         3   Bonus award to gaming machine OK
    ///                                         4   Leave as 0
    ///                                        7~5  TBD (leave as 0)
    /// Lock Timeout    2 BCD    0000-9999     Lock expiration time in hundredths of a second.
    /// CRC             2        0000-FFFF     16-bit CRC
    ///
    /// ===============================================================================================================================
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           74        AFT lock and status request
    /// Length           1           23        Number of bytes following, not including CRC
    /// Asset Number     4        nnnnnnnn     Gaming machine asset number or house ID
    /// Game Lock Status 1           nn        00 - Game locked
    ///                                        40 - Game lock pending
    ///                                        FF - Game not locked
    /// Available Transfers 1      00-FF       Bit  Description
    ///                                         0   1 = Transfer to gaming machine OK
    ///                                         1   1 = Transfer from gaming machine OK
    ///                                         2   1 = Transfer to printer OK
    ///                                         3   1 = Win amount pending cashout to host
    ///                                         4   1 = Bonus award to gaming machine OK
    ///                                        6~5  TBD (leave as 0)
    ///                                         7   Lock After Transfer request supported (deprecated)
    /// Host Cashout Status 1      00-FF       Bit  Description
    ///                                         0   0 = Cashout to host forced by gaming machine, 1 = Cashout to host controllable by host
    ///                                         1   0 = Cashout to host currently disabled, 1 = Cashout to host currently enabled
    ///                                         2   0 = Host cashout mode currently soft, 1 = Host cashout mode currently hard (only valid if cashout to host is enabled)
    ///                                        7~3  TBD (leave as 0)
    /// AFT Status       1         00-FF       Bit  Description
    ///                                         0   1 = Printer available for transaction receipts
    ///                                         1   1 = Transfer to host of less than full available amount allowed
    ///                                         2   1 = Custom ticket data supported
    ///                                         3   1 = AFT registered
    ///                                         4   1 = In-house transfers enabled
    ///                                         5   1 = Bonus transfers enabled
    ///                                         6   1 = Debit transfers enabled
    ///                                         7   1 = Any AFT enabled
    /// Max Buffer Index 1         46-7F       Maximum transactions in history buffer
    /// Current Cashable Amount 5 BCD XXXXX    Current cashable amount on gaming machine, in cents
    /// Current Restricted Amount 5 BCD XXXXX  Current restricted amount on gaming machine, in cents
    /// Current Non-restricted Amount 5 BCD XXXXX Current nonrestricted amount on gaming machine, in cents
    /// Gaming Machine Transfer Limit 5 BCD XXXXX Maximum amount that may currently be transferred to the credit meter, in cents
    /// Restricted Expiration 5 BCD XXXXX      Current restricted expiration date in MMDDYYYY format or 0000NNNN days format, if restricted amount non-zero
    /// Restricted Pool ID  1    0000-FFFF     Current restricted pool ID, if restricted non-zero
    /// CRC              2       0000-FFFF     16-bit CRC
    /// </remarks>
    [Sas(SasGroup.Aft)]
    public class LP74AftGameLockAndStatusRequestParser : SasLongPollParser<AftGameLockAndStatusResponseData, AftGameLockAndStatusData>
    {

        private const int LockCodeOffset = 2;
        private const int TransferConditionOffset = 3;
        private const int LockTimeoutOffset = 4;
        private const byte AssetNumberLength = 4;
        private const byte RestrictedPoolIdLength = 2;
        private const byte TransferConditionActiveBitMask = 0x0F;

        /// <inheritdoc />
        public LP74AftGameLockAndStatusRequestParser()
            : base(LongPoll.AftGameLockAndStatusRequest)
        {
        }

        /// <inheritdoc />
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var longPoll = command.ToArray();
            var data = new AftGameLockAndStatusData();
            if (!GetAftGameLockAndStatusRequestDataFromCommand(longPoll, ref data))
            {
                return NackLongPoll(command);
            }

            var responseData = Handler(data);
            return GenerateLongPollResponse(command, responseData);
        }

        private bool GetAftGameLockAndStatusRequestDataFromCommand(byte[] longPoll, ref AftGameLockAndStatusData data)
        {
            data.LockCode = (AftLockCode)longPoll[LockCodeOffset];

            if (data.LockCode == AftLockCode.InterrogateCurrentStatusOnly)
            {
                // Interrogate only, so ignore the rest
                return true;
            }

            data.TransferConditions =
                (AftTransferConditions)(longPoll[TransferConditionOffset] & TransferConditionActiveBitMask);

            var (lockTimeout, validLockTimeout) = Utilities.FromBcdWithValidation(longPoll, LockTimeoutOffset, SasConstants.Bcd4Digits);
            if (!validLockTimeout)
            {
                Logger.Debug("AFT Game Lock Expiration is not a valid BCD number");
                return false;
            }

            data.LockTimeout = (int)lockTimeout;

            return true;
        }

        private Collection<byte> GenerateLongPollResponse(IReadOnlyCollection<byte> command, AftGameLockAndStatusResponseData responseData)
        {
            return new Collection<byte>()
            {
                command.Take(2),
                responseData.Length,
                Utilities.ToBinary((uint)responseData.AssetNumber, AssetNumberLength),
                responseData.GameLockStatus,
                responseData.AvailableTransfers,
                responseData.HostCashoutStatus,
                responseData.AftStatus,
                responseData.MaxBufferIndex,
                Utilities.ToBcd(responseData.CurrentCashableAmount, SasConstants.Bcd10Digits),
                Utilities.ToBcd(responseData.CurrentRestrictedAmount, SasConstants.Bcd10Digits),
                Utilities.ToBcd(responseData.CurrentNonRestrictedAmount, SasConstants.Bcd10Digits),
                Utilities.ToBcd(responseData.CurrentGamingMachineTransferLimit, SasConstants.Bcd10Digits),
                Utilities.ToBcd(responseData.RestrictedExpiration, SasConstants.Bcd8Digits),
                Utilities.ToBinary(responseData.RestrictedPoolId, RestrictedPoolIdLength)
            };
        }
    }
}