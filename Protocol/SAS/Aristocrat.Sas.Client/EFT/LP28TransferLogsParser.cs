namespace Aristocrat.Sas.Client.EFT
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Eft;
    using Eft.Response;

    /// <summary>
    ///     This handles the send transfer log long poll
    /// </summary>
    /// <remarks>
    /// The command is as follows (Slot Accounting System version 5.02, Section 8.7):
    /// Field      Bytes        Value                                           Description
    /// Address    1            01-7F                                           Gaming Machine Address
    /// Command    1            28                                              Send Cash Out Ticket Information
    /// 
    /// Response
    /// Field       Bytes       Value                                           Description
    /// Address     1           01-7F                                           Gaming Machine Address
    /// Command     1           28                                              Send transaction log
    /// Transfers   45/varies   Cmd, Trans#, ACK, Status, Amount (5 bytes)      Last 5 logged transfers    
    ///                         Cmd, Trans#, ACK, Status, Amount (5 bytes)      sent most recent
    ///                         Cmd, Trans#, ACK, Status, Amount (5 bytes)      transaction first
    ///                         Cmd, Trans#, ACK, Status, Amount (5 bytes)      if less than five, send 9 '00' bytes
    ///                         Cmd, Trans#, ACK, Status, Amount (5 bytes)
    /// 
    /// CRC         2           0000-FFFF   16-bit CRC                          CCITT 16-bit CRC sent LSB first
    /// </remarks>
    [Sas(SasGroup.Eft)]
    public class LP28TransferLogsParser : SasLongPollParser<EftTransactionLogsResponse, LongPollData>
    {
        /// <summary>
        ///     Initializes a new instance of the LP28TransferLogsParser class.
        /// </summary>
        public LP28TransferLogsParser() : base(LongPoll.EftSendTransferLogs)
        {
        }

        /// <summary>
        ///     Return 5 most recent transfer logs, compensate with 0x00 if no enough 5 transfer logs
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            var response = Handler(Data);
            var result = command.ToList();
            result.AddRange(response.EftTransactionLogs.SelectMany(GetEftTransferLogBytes));
            var numOfMoreLogsToAppend = Math.Max(SasConstants.EftHistoryLogsSize - response.EftTransactionLogs.Length, 0);
            for(int i = 0; i < numOfMoreLogsToAppend; i++)
            {
                result.AddRange(Enumerable.Repeat(SasConstants.EftDefaultByteValue, SasConstants.EftBytesSizeOfSingleHistoryLog));
            }
            return result;
        }

        /// <summary>
        ///     Get the bytes format of eft history log
        /// </summary>
        /// <param name="aLog"></param>
        /// <returns>9 bytes array: Cmd, Trans#, ACK, Status, Amount (5 bytes)</returns>
        private byte[] GetEftTransferLogBytes(IEftHistoryLogEntry aLog)
        {
            var logBytes = new List<byte>();
            logBytes.Add((byte)aLog.Command);
            logBytes.Add((byte)aLog.TransactionNumber);
            logBytes.Add((byte)(aLog.Acknowledgement ? 1 : 0));
            logBytes.Add((byte)aLog.ReportedTransactionStatus);
            logBytes.AddRange(
                aLog.TransferType == EftTransferType.In
                    ? Utilities.ToBcd(aLog.RequestedTransactionAmount, SasConstants.Bcd10Digits)
                    : Utilities.ToBcd(aLog.ReportedTransactionAmount, SasConstants.Bcd10Digits));

            return logBytes.ToArray();
        }
    }
}