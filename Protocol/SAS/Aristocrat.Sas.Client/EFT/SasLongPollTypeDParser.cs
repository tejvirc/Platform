namespace Aristocrat.Sas.Client.EFT
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Eft;
    using log4net;

    /// <summary>
    /// SasLongPollParser for Type D long poll request
    /// </summary>
    public abstract class SasLongPollTypeDParser : SasLongPollParser<EftTransactionResponse, EftTransferData>
    {
        protected static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <inheritdoc />
        protected SasLongPollTypeDParser(LongPoll command)
            : base(command)
        {

        }

        /// <inheritdoc />
        public override IReadOnlyCollection<byte> Parse(IReadOnlyCollection<byte> command)
        {
            Logger.Debug($"Begin generating response for command {Command}");

            var (request, error) = GetRequest(command);
            var response = error == default ? Handler(request) : new EftTransactionResponse { Status = error.Value, TransferAmount = 0 };
            Handlers = response.Handlers;
            var data = BuildResponse(command, response);

            Logger.Debug($"End generating response for command {Command}");
            return data;
        }

        /// <summary>
        /// Validate and create EftTransferData given the byte array from SAS HOST, if validate passed, it will return EftTransferDate object with null TransactionStatus.
        /// Otherwise, it will return null EftTransferDate and a TransactionStatus as error code.
        /// 
        ///                         Type D Long Poll Format
        ///+-------------------+----------+----------+--------------------------------------------+
        ///|       Field       |  Bytes   |  Value   |                Description                 |
        ///+-------------------+----------+----------+--------------------------------------------+
        ///| Address           | 1 binary | 01-7F    | Address of gaming machine to poll          |
        ///| Command           | 1 binary | 62,63,69 | Transfer from the gaming machine           |
        ///| TransactionNumber | 1 binary | 1-FF     | Message Transaction Number                 |
        ///| ACK               | 1 binary | 0-1      | Acknowledgement flag                       |
        ///| Transfer Amount   | 4 BCD    | XXXX     | Amount transferred to the gaming machine   |
        ///+-------------------+----------+----------+--------------------------------------------+
        /// 
        ///                     Gaming Machine Response for Type D
        ///+--------------------+----------+----------+--------------------------------------------+
        ///|       Field        |  Bytes   |  Value   |                Description                 |
        ///+--------------------+----------+----------+--------------------------------------------+
        ///| Address            | 1 binary | 01-7F    | Address of gaming machine responding       |
        ///| Command            | 1 binary | 62,63,69 | Transfer from the gaming machine           |
        ///| Transaction Number | 1 binary | 1-FF     | Message transaction number                 |
        ///| ACK                | 1 binary | 0-1      | Acknowledgement flag                       |
        ///| Status             | 1 binary | 00-0E    | Gaming machine transaction status          |
        ///| Transfer amount    | 4 BCD    | XXXX     | Amount accepted by the gaming machine      |
        ///+--------------------+----------+----------+--------------------------------------------+
        /// </summary>
        protected virtual (EftTransferData, TransactionStatus?) GetRequest(IReadOnlyCollection<byte> command)
        {
            var lpCommand = command.ElementAt(1);

            var transactionNumber = command.ElementAt(2);
            if (transactionNumber < 0x01 || transactionNumber > 0xff)
            {
                return (default, TransactionStatus.InvalidTransactionNumber);
            }

            var ack = command.ElementAt(3);
            if (ack != 0x00 && ack != 0x01)
            {
                return (default, TransactionStatus.InvalidAck);
            }

            var (amount, validation) = Utilities.FromBcdWithValidation(command.ToArray(), 4, SasConstants.Bcd8Digits);
            if (!validation)
            {
                return (default, TransactionStatus.ContainsNonBcdData);
            }

            var request = new EftTransferData
            {
                Command = (LongPoll)lpCommand,
                TransferType = EftTransferType.In,
                TransferAmount = amount,
                Acknowledgement = ack == 1,
                TransactionNumber = transactionNumber
            };

            return (request, default);
        }

        /// <summary>
        /// Return an array of bytes to SAS HOST after processing.
        /// </summary>
        protected virtual IReadOnlyCollection<byte> BuildResponse(IReadOnlyCollection<byte> command, EftTransactionResponse responseData)
        {
            var result = new List<byte>(9);
            result.AddRange(command.Take(4));
            result.Add((byte)responseData.Status);
            result.AddRange(Utilities.ToBcd(responseData.TransferAmount, SasConstants.Bcd8Digits));
            return result;
        }
    }
}