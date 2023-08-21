namespace Aristocrat.Sas.Client.LPParsers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using LongPollDataClasses;

    /// <summary>
    ///     This handles the Redeem Ticket Command
    /// </summary>
    /// <remarks>
    /// The command is as follows:
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           71        Redeem Ticket
    /// Length           1         01-2D       Length of bytes following, not including the CRC
    /// Transfer code    1         table       See Table 15.12c in the SAS Protocol Spec on page 15-21
    /// Transfer Amt   5 BCD                   Ticket transfer amount in cents
    /// Parsing code     1          00         The validation number is 9 BCD/18 digit decimal code. The first two digits are
    ///                                          a 2-digit system ID code indicating how to interpret the following 16 digits.
    ///                                          ID code 00 indicates that the following 16 digits represent a SAS secure enhanced
    ///                                          validation number. Other system ID codes and parsing codes will be
    ///                                          assigned by IGT as needed.
    /// Validation data  18       ???          Up to 32 bytes of validation data. (normally it will be 18 bytes)
    /// Restricted expire 4 BCD                Expiration data in MMDDYYYY format or 0000NNNN days format
    /// Pool Id          2       0000-FFFF     Restricted pool id
    /// Target ID Length 1        nn           Length of Target ID data following
    /// Target ID        x ASCII  ???          Target ID of targeted funds
    /// CRC              2       0000-FFFF     16-bit CRC
    ///
    /// Response
    /// Field          Bytes       Value       Description
    /// Address          1         01-7F       Gaming Machine Address
    /// Command          1           71        Redeem Ticket
    /// Length           1         01-27       Length of bytes following, not including the CRC
    /// Machine status   1         table       See Table 15.12d in the SAS Protocol Spec on page 15-22
    /// Transfer Amt   5 BCD                   Ticket transfer amount in cents (all zeros if no amount available
    /// Parsing code     1          00         See parsing code above
    /// Validation data  x       ???           Up to 32 bytes of validation data. (normally it will be 18 bytes)
    /// CRC              2       0000-FFFF     16-bit CRC
    /// </remarks>
    [Sas(SasGroup.Validation)]
    public class LP71RedeemTicketParser : SasLongPollParser<RedeemTicketResponse, RedeemTicketData>
    {
        private const int DataStartIndex = 2;
        private const int MinReceivedBytes = 21;
        private const int InterrogateStatusLength = 6;
        private const int MinRestrictedExpirationBytes = 6;
        private const int MinDataLen = 16;
        private const int TransferAmountBCDLength = 5;
        private const int RestrictedExpirationBCDLength = 4;
        private const int PoolIdSize = 2;
        private const int BarcodeSize = 9;

        /// <inheritdoc />
        public LP71RedeemTicketParser()
            : base(LongPoll.RedeemTicket)
        {
        }

        /// <inheritdoc />
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var receivedBytes = command.ToArray();
            var index = DataStartIndex;
            var dataLen = receivedBytes[index++];
            var data = new RedeemTicketData { TransferCode = (TicketTransferCode)receivedBytes[index++] };
            if (receivedBytes.Length >= MinReceivedBytes && dataLen >= MinDataLen)
            {
                var (transferAmount, validAmount) = Utilities.FromBcdWithValidation(receivedBytes, (uint)index, TransferAmountBCDLength);
                if (!validAmount)
                {
                    Logger.Debug("Redeem Ticket: Transfer Amount is not a valid BCD number");
                    return NackLongPoll(command);
                }

                data.TransferAmount = transferAmount;
                index += TransferAmountBCDLength;
                data.ParsingCode = (ParsingCode)receivedBytes[index++];
                data.Barcode = BitConverter.ToString(receivedBytes, index, BarcodeSize).Replace("-", "");
                index += BarcodeSize;

                // If it is not a restricted ticket.
                data.PoolId = RedeemTicketData.NilPoolId;
                data.RestrictedExpiration = 0;

                if (receivedBytes.Length >= (MinReceivedBytes + MinRestrictedExpirationBytes) &&
                    dataLen >= (MinDataLen + MinRestrictedExpirationBytes))
                {
                    var (expiration, validExpiration) = Utilities.FromBcdWithValidation(receivedBytes, (uint)index, RestrictedExpirationBCDLength);
                    if (!validExpiration)
                    {
                        Logger.Debug("Redeem Ticket: Expiration is not a valid BCD number");
                        return NackLongPoll(command);
                    }

                    data.RestrictedExpiration = (long)expiration;
                    index += RestrictedExpirationBCDLength;
                    data.PoolId = (int)Utilities.FromBinary(receivedBytes, (uint)index, PoolIdSize);
                    index += PoolIdSize;

                    if (receivedBytes.Length > (MinReceivedBytes + MinRestrictedExpirationBytes))
                    {
                        var targetLength = receivedBytes[index++];
                        if ((receivedBytes.Length - SasConstants.NumberOfCrcBytes) < (targetLength + index))
                        {
                            Logger.Debug("Invalid Target ID length");
                            return NackLongPoll(command);
                        }

                        data.TargetId = Encoding.ASCII.GetString(receivedBytes, index, targetLength);
                    }
                }
            }
            else if (receivedBytes.Length != InterrogateStatusLength)
            {
                Logger.Debug("Invalid Command Length");
                return NackLongPoll(command);
            }

            var response = Handler(data);
            if (response == null)
            {
                // not supporting it; shouldn't send anything and let Host ignore it in 20 ms.
                return null;
            }

            Handlers = response.Handlers;

            var result = command.Take(DataStartIndex).ToList();

            var responseData = new List<byte> { (byte)response.MachineStatus };
            if (response.MachineStatus != RedemptionStatusCode.NotCompatibleWithCurrentRedemptionCycle &&
                response.MachineStatus != RedemptionStatusCode.NoValidationInfoAvailable)
            {
                // Per the SAS spec this does not sent when we respond with not compatible with the current redemption cycle
                responseData.AddRange(Utilities.ToBcd(response.TransferAmount, TransferAmountBCDLength));
                responseData.Add((byte)response.ParsingCode);
                responseData.AddRange(Utilities.AsciiStringToBcd(response.Barcode, true, BarcodeSize));
            }

            result.Add((byte)responseData.Count);
            result.AddRange(responseData);

            return result;
        }
    }
}
