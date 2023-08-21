namespace Aristocrat.Monaco.G2S.Handlers.Voucher
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Services;
    using ITransaction = Accounting.Contracts.ITransaction;

    /// <summary>
    ///     Handles the v21.getVoucherLog G2S message
    /// </summary>
    public class GetVoucherLog : ICommandHandler<voucher, getVoucherLog>
    {
        private readonly IG2SEgm _egm;
        private readonly ITransactionReferenceProvider _references;
        private readonly ITransactionHistory _transactionHistory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetVoucherLog" /> class.
        /// </summary>
        public GetVoucherLog(
            IG2SEgm egm,
            ITransactionHistory transactionHistory,
            ITransactionReferenceProvider references)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
            _references = references ?? throw new ArgumentNullException(nameof(references));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<voucher, getVoucherLog> command)
        {
            return await Sanction.OwnerAndGuests<IVoucherDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<voucher, getVoucherLog> command)
        {
            var device = _egm.GetDevice<IVoucherDevice>(command.IClass.deviceId);
            var response = command.GenerateResponse<voucherLogList>();

            var voucherOutTransactions = _transactionHistory.RecallTransactions<VoucherOutTransaction>();
            var voucherInTransactions = _transactionHistory.RecallTransactions<VoucherInTransaction>();

            var allLogs = new List<ITransaction>(voucherOutTransactions);
            allLogs.AddRange(voucherInTransactions);

            var sorted = allLogs.OrderBy(o => o.TransactionId);

            response.Command.voucherLog = sorted
                .TakeTransactions(command.Command.lastSequence, command.Command.totalEntries)
                .Select(
                    transaction => GetLog(transaction, device)).ToArray();

            await Task.CompletedTask;
        }

        private voucherLog GetLog(ITransaction transaction, IDevice device)
        {
            switch (transaction)
            {
                case VoucherOutTransaction outTransaction:
                    return outTransaction.ToLog(_references);
                case VoucherInTransaction inTransaction:
                    return VoucherExtensions.GetVoucherLog(inTransaction, device.PrefixedDeviceClass());
            }

            return null;
        }
    }
}