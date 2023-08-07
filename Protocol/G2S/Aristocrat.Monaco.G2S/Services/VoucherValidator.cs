namespace Aristocrat.Monaco.G2S.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.TransferOut;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Data.Model;
    using Handlers;
    using Kernel;
    using log4net;
    using static Handlers.Voucher.VoucherExtensions;

    /// <summary>
    ///     An <see cref="IVoucherValidator" /> implementation
    /// </summary>
    public class VoucherValidator : IVoucherValidator, IService
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IG2SEgm _egm;
        private readonly IVoucherDataService _voucherDataService;
        private readonly IPropertiesManager _propertiesManager;

        private readonly TimeSpan _retryGetVoucherDataInterval = TimeSpan.FromSeconds(5);
        private readonly TimeSpan _retryGetVoucherDeviceInterval = TimeSpan.FromSeconds(5);

        /// <summary>
        ///     Initializes a new instance of the <see cref="VoucherValidator" /> class.
        /// </summary>
        /// <param name="egm">Egm</param>
        /// <param name="voucherDataService">Voucher data service.</param>
        /// <param name="propertiesManager">Properties Manager</param>
        public VoucherValidator(
            IG2SEgm egm,
            IVoucherDataService voucherDataService,
            IPropertiesManager propertiesManager)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _voucherDataService = voucherDataService ?? throw new ArgumentNullException(nameof(voucherDataService));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IVoucherValidator) };

        /// <inheritdoc />
        public void Initialize()
        {
            Logger.Info("Initializing the G2S voucher validator.");
        }

        /// <inheritdoc />
        public bool CanValidateVouchersIn => _voucherDataService?.HostOnline ?? false;

        /// <inheritdoc />
        public bool CanCombineCashableAmounts => _egm.GetDevice<IVoucherDevice>()?.CombineCashableOut ?? false;

        /// <inheritdoc />
        public bool HostOnline => _voucherDataService?.HostOnline ?? false;

        /// <inheritdoc />
        public bool ReprintFailedVoucher => false;

        /// <inheritdoc />
        public bool CanValidateVoucherOut(long amount, AccountType type)
        {
            // TODO when Non Cash can be added. Check if we can cashout this account type (ex. for Non Cash: allowNonCashOut and printNonCashOffLine)
            return (_voucherDataService?.HostOnline ?? false) ||
                   (_egm.GetDevice<IVoucherDevice>()?.PrintOffLine ?? false);
        }

        /// <inheritdoc />
        public Task<VoucherAmount> RedeemVoucher(VoucherInTransaction transaction)
        {
            return Task.Run(() => HandleVoucherIn(transaction));
        }

        /// <inheritdoc />
        public Task StackedVoucher(VoucherInTransaction transaction)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<VoucherOutTransaction> IssueVoucher(
            VoucherAmount amount,
            AccountType type,
            Guid transactionId,
            TransferOutReason reason)
        {
            return Task.Run(() => HandleVoucherOut(amount.Amount, type, reason));
        }

        /// <inheritdoc />
        public void CommitVoucher(VoucherInTransaction transaction)
        {
            Task.Run(() => _voucherDataService.SendCommitVoucher(transaction));
        }

        private static string GetMd5VerificationCode(MD5 md5Hash, string input)
        {
            var data = md5Hash.ComputeHash(Encoding.ASCII.GetBytes(input));

            var sBuilder = new StringBuilder();
            for (var i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2").ToUpper(CultureInfo.CurrentCulture));
                if ((i + 1) % 2 == 0 && i > 0 && i != data.Length - 1)
                {
                    sBuilder.Append('-');
                }
            }

            return sBuilder.ToString();
        }

        private string GetManualVerificationCode(VoucherData data, long amount)
        {
            var result = new StringBuilder(_egm.Id.PadRight(32, '0'));

            result.Append(data.ValidationId);
            result.Append(data.ValidationSeed.PadLeft(20, '0'));
            result.Append(amount.ToString().PadLeft(20, '0'));

            using (var md5Hash = MD5.Create())
            {
                return GetMd5VerificationCode(md5Hash, result.ToString().ToUpper(CultureInfo.CurrentCulture));
            }
        }

        private VoucherOutTransaction HandleVoucherOut(long amount, AccountType type, TransferOutReason reason)
        {
            var done = false;
            VoucherData voucherData;
            do
            {
                // Get the voucher data.
                voucherData = _voucherDataService.GetVoucherData();

                if (!string.IsNullOrEmpty(voucherData?.ValidationId))
                {
                    done = true;
                }
                else
                {
                    Logger.Warn(
                        $"HandleVoucherOut for {amount} null voucherData, retry in {_retryGetVoucherDataInterval}...");
                    Thread.Sleep(_retryGetVoucherDataInterval);
                }
            } while (!done);

            done = false;
            IVoucherDevice device;
            do
            {
                // Get the voucher device.
                device = _egm.GetDevice<IVoucherDevice>();
                if (device != null)
                {
                    done = true;
                }
                else
                {
                    Logger.Warn(
                        $"HandleVoucherOut for {amount} null voucher device, retry in {_retryGetVoucherDeviceInterval}...");
                    Thread.Sleep(_retryGetVoucherDeviceInterval);
                }
            } while (!done);

            var expirationDate = device.ExpireCashPromo != Constants.ExpirationNotSet
                ? device.ExpireCashPromo
                : _propertiesManager.GetValue(
                    AccountingConstants.VoucherOutExpirationDays,
                    AccountingConstants.DefaultVoucherExpirationDays);
            var result = new VoucherOutTransaction(
                device.Id,
                DateTime.UtcNow,
                amount,
                type,
                voucherData.ValidationId,
                expirationDate,
                !_voucherDataService.HostOnline ? GetManualVerificationCode(voucherData, amount) : string.Empty)
            {
                Reason = reason, HostOnline = _voucherDataService.HostOnline
            };
            return result;
        }

        private VoucherAmount HandleVoucherIn(VoucherInTransaction transaction)
        {
            var (authorizeVoucher, voucherLog) = _voucherDataService.SendRedeemVoucher(
                transaction.TransactionId,
                transaction.Barcode,
                transaction.LogSequence,
                0);

            if (authorizeVoucher != null)
            {
                transaction.VoucherSequence = authorizeVoucher.voucherSequence;
                transaction.Amount = authorizeVoucher.voucherAmt;
                transaction.TypeOfAccount = authorizeVoucher.creditType.ToAccountType();
            }

            if (voucherLog != null)
            {
                if (voucherLog.hostException != 0)
                {
                    transaction.Exception =
                        (int)((HostVoucherExceptions)voucherLog.hostException).ToVoucherInExceptionCode();
                }
                else if (voucherLog.egmException != 0)
                {
                    transaction.Exception =
                        (int)((EgmVoucherException)voucherLog.egmException).ToVoucherInExceptionCode();
                }
            }

            return null;
        }
    }
}