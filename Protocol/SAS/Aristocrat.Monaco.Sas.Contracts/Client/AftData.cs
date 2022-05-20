namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    using System;
    using System.Text;
    using Aristocrat.Monaco.Application.Contracts.Extensions;
    using Aristocrat.Sas.Client.LongPollDataClasses;

    /// <summary>Definition of the AftData class.</summary>
    public class AftData
    {
        /// <summary>Gets or sets the CashableAmount of the Aft transfer.</summary>
        public ulong CashableAmount { get; set; }

        /// <summary>Gets or sets the RestrictedAmount of the Aft transfer.</summary>
        public ulong RestrictedAmount { get; set; }

        /// <summary>Gets or sets the Non-restrictedAmount of the Aft transfer.</summary>
        public ulong NonRestrictedAmount { get; set; }

        /// <summary>Gets or sets the account balance of the patron before the transfer has completed.</summary>
        public ulong AccountBalance { get; set; }

        /// <summary>Gets or sets the TransactionId of the Aft transfer.</summary>
        public string TransactionId { get; set; }

        /// <summary>Gets or sets the date/time of the transaction</summary>
        public DateTime TransactionDateTime { get; set; }

        /// <summary>Gets or sets the data to be printed on an Aft receipt.</summary>
        public AftReceiptData ReceiptData { get; set; } = new AftReceiptData();

        /// <summary>Gets or sets the TransferCode of the Aft transfer.</summary>
        public AftTransferCode TransferCode { get; set; }

        /// <summary>Gets or sets the TransferStatus of the Aft transfer.</summary>
        public AftTransferStatusCode TransferStatus { get; set; }

        /// <summary>Gets or sets the ReceiptStatus of the Aft transfer.</summary>
        public byte ReceiptStatus { get; set; }

        /// <summary>Gets or sets the TransferType of the Aft transfer.</summary>
        public AftTransferType TransferType { get; set; }

        /// <summary>Gets or sets the TransferFlags of the Aft transfer.</summary>
        public AftTransferFlags TransferFlags { get; set; }

        /// <summary>Gets or sets the expiration of the Aft transfer.</summary>
        public uint Expiration { get; set; }

        /// <summary>Gets or sets the PoolId of the Aft transfer.</summary>
        public ushort PoolId { get; set; }

        /// <summary>Generates a human readable version of the class data</summary>
        /// <returns>The string representation of the class data</returns>
        public override string ToString()
        {
            var descriptiveStringBuilder = new StringBuilder();

            descriptiveStringBuilder.Append("AftData : [");
            descriptiveStringBuilder.AppendFormat($"CashableAmount = {CashableAmount}, ");
            descriptiveStringBuilder.AppendFormat($"RestrictedAmount = {RestrictedAmount}, ");
            descriptiveStringBuilder.AppendFormat($"NonRestrictedAmount = {NonRestrictedAmount}, ");
            descriptiveStringBuilder.AppendFormat($"AccountBalance = {AccountBalance}, ");
            descriptiveStringBuilder.AppendFormat($"TransactionId = {TransactionId}, ");
            descriptiveStringBuilder.AppendFormat($"TransactionDateTime = {TransactionDateTime}, ");
            descriptiveStringBuilder.AppendFormat($"TransferSource = {ReceiptData.TransferSource}, ");
            descriptiveStringBuilder.AppendFormat($"PatronName = {ReceiptData.PatronName}, ");
            descriptiveStringBuilder.AppendFormat($"PatronAccount = {ReceiptData.PatronAccount}, ");
            descriptiveStringBuilder.AppendFormat($"ReceiptTime = {ReceiptData.ReceiptTime}, ");
            descriptiveStringBuilder.AppendFormat($"TransferCode = {TransferCode}, ");
            descriptiveStringBuilder.AppendFormat($"TransferStatus = {TransferStatus}, ");
            descriptiveStringBuilder.AppendFormat($"ReceiptStatus = {ReceiptStatus}, ");
            descriptiveStringBuilder.AppendFormat($"TransferType = {TransferType}, ");
            descriptiveStringBuilder.AppendFormat($"TransferFlags = {TransferFlags}, ");
            descriptiveStringBuilder.AppendFormat($"Expiration = {Expiration}, ");
            descriptiveStringBuilder.AppendFormat($"PoolId = {PoolId}");
            descriptiveStringBuilder.Append("]");

            return descriptiveStringBuilder.ToString();
        }

        /// <summary>
        ///     Creates and AftData class from an AftTransferData class
        /// </summary>
        /// <param name="data">The aft response data to convert to an aft data</param>
        /// <returns>An AftData object based on the values in the AftResponseData</returns>
        public static AftData AftDataFromAftResponseData(AftResponseData data)
        {
            return new AftData
            {
                CashableAmount = data.CashableAmount,
                RestrictedAmount = data.RestrictedAmount,
                NonRestrictedAmount = data.NonRestrictedAmount,
                TransferCode = data.TransferCode,
                TransferType = data.TransferType,
                TransferStatus = data.TransferStatus,
                TransferFlags = data.TransferFlags,
                TransactionId = data.TransactionId,
                Expiration = data.Expiration,
                PoolId = data.PoolId,
                ReceiptData = data.ReceiptData,
                AccountBalance = data.ReceiptData.AccountBalance,
                TransactionDateTime = data.TransactionDateTime,
                ReceiptStatus = data.ReceiptStatus
            };
        }

        /// <summary>
        ///     Updates the passed in AftResponseData object with the latest values
        /// </summary>
        /// <param name="responseData">The response data to update</param>
        /// <returns>An AftData object based on the values in the AftResponseData</returns>
        public AftResponseData UpdateAftResponseData(AftResponseData responseData)
        {
            responseData.CashableAmount = (ulong)((long)CashableAmount).MillicentsToCents();
            responseData.RestrictedAmount = (ulong)((long)RestrictedAmount).MillicentsToCents();
            responseData.NonRestrictedAmount = (ulong)((long)NonRestrictedAmount).MillicentsToCents();
            responseData.ReceiptStatus = responseData.ReceiptStatus;
            responseData.TransactionDateTime = TransactionDateTime;

            return responseData;
        }
    }
}
