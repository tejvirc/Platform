namespace Aristocrat.Monaco.Mgam.Mappings
{
    using Aristocrat.Mgam.Client.Messaging;
    using AutoMapper;
    using Common.Data.Models;
    using BeginSessionWithSessionId = Aristocrat.Mgam.Client.Messaging.BeginSessionWithSessionId;

    /// <summary>
    ///     Mapping configurations for sessions.
    /// </summary>
    public class SessionProfile : Profile
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SessionProfile" /> class.
        /// </summary>
        public SessionProfile()
        {
            CreateMap<Commands.EscrowCash, EscrowCash>()
                .ForMember(d => d.InstanceId, m => m.Ignore())
                .ForMember(d => d.Amount, m => m.MapFrom(s => s.Value));

            CreateMap<Commands.ValidateVoucher, ValidateVoucher>()
                .ForMember(d => d.InstanceId, m => m.Ignore())
                .ForMember(d => d.VoucherBarcode, m => m.MapFrom(s => s.Barcode));

            CreateMap<BeginSessionResponse, Session>()
                .ForMember(d => d.SessionId, m => m.MapFrom(s => s.SessionId))
                .ForMember(d => d.OfflineVoucherBarcode, m => m.MapFrom(s => s.OffLineVoucherBarcode))
                .ForMember(d => d.OfflineVoucherPrinted, m => m.Ignore())
                .ForMember(d => d.Id, m => m.Ignore());
            
            CreateMap<EndSessionResponse, Voucher>()
                .ForMember(d => d.VoucherBarcode, m => m.MapFrom(s => s.VoucherBarcode))
                .ForMember(d => d.CasinoName, m => m.MapFrom(s => s.CasinoName))
                .ForMember(d => d.CasinoAddress, m => m.MapFrom(s => s.CasinoAddress))
                .ForMember(d => d.VoucherType, m => m.MapFrom(s => s.VoucherType))
                .ForMember(d => d.CashAmount, m => m.MapFrom(s => s.CashAmount))
                .ForMember(d => d.CouponAmount, m => m.MapFrom(s => s.CouponAmount))
                .ForMember(d => d.TotalAmount, m => m.MapFrom(s => s.TotalAmount))
                .ForMember(d => d.AmountLongForm, m => m.MapFrom(s => s.AmountLongForm))
                .ForMember(d => d.Date, m => m.MapFrom(s => s.Date))
                .ForMember(d => d.Time, m => m.MapFrom(s => s.Time))
                .ForMember(d => d.Expiration, m => m.MapFrom(s => s.Expiration))
                .ForMember(d => d.DeviceId, m => m.MapFrom(s => s.Id))
                .ForMember(d => d.OfflineReason, m => m.Ignore())
                .ForMember(d => d.Id, m => m.Ignore());

            CreateMap<Commands.BeginSession, BeginSession>()
                .ForMember(d => d.InstanceId, m => m.Ignore());

            CreateMap<Commands.BeginSessionWithSessionId, BeginSessionWithSessionId>()
                .ForMember(d => d.InstanceId, m => m.Ignore())
                .ForMember(d => d.ExistingSessionId, m => m.MapFrom(s => s.SessionId))
                .ForMember(d => d.VoucherPrintedOffLine, m => m.MapFrom(s => s.VoucherPrintedOffline))
                .ForMember(d => d.PrintedOffLineVoucherBarcode, m => m.MapFrom(s => s.Barcode));

            CreateMap<Commands.CreditCash, CreditCash>()
                .ForMember(d => d.InstanceId, m => m.Ignore())
                .ForMember(d => d.SessionId, m => m.Ignore())
                .ForMember(d => d.LocalTransactionId, m => m.Ignore())
                .ForMember(d => d.Amount, m => m.MapFrom(s => s.Amount));

            CreateMap<Commands.CreditVoucher, CreditVoucher>()
                .ForMember(d => d.InstanceId, m => m.Ignore())
                .ForMember(d => d.SessionId, m => m.Ignore())
                .ForMember(d => d.LocalTransactionId, m => m.Ignore())
                .ForMember(d => d.VoucherBarcode, m => m.MapFrom(s => s.Barcode));

            CreateMap<Commands.RequestPlay, RequestPlay>()
                .ForMember(d => d.InstanceId, m => m.Ignore())
                .ForMember(d => d.SessionId, m => m.Ignore())
                .ForMember(d => d.SessionCashBalance, m => m.MapFrom(s => s.CashBalance))
                .ForMember(d => d.SessionCouponBalance, m => m.MapFrom(s => s.CouponBalance))
                .ForMember(d => d.PayTableIndex, m => m.MapFrom(s => s.PaytableIndex))
                .ForMember(d => d.NumberOfCredits, m => m.MapFrom(s => s.NumberOfCredits))
                .ForMember(d => d.Denomination, m => m.MapFrom(s => s.Denomination))
                .ForMember(d => d.GameUpcNumber, m => m.MapFrom(s => s.GameUpcNumber))
                .ForMember(d => d.LocalTransactionId, m => m.Ignore());

            CreateMap<RequestPlayVoucherResponse, Voucher>()
                .ForMember(d => d.VoucherBarcode, m => m.MapFrom(s => s.VoucherBarcode))
                .ForMember(d => d.CasinoName, m => m.MapFrom(s => s.CasinoName))
                .ForMember(d => d.CasinoAddress, m => m.MapFrom(s => s.CasinoAddress))
                .ForMember(d => d.VoucherType, m => m.MapFrom(s => s.VoucherType))
                .ForMember(d => d.CashAmount, m => m.MapFrom(s => s.CashAmount))
                .ForMember(d => d.CouponAmount, m => m.MapFrom(s => s.CouponAmount))
                .ForMember(d => d.TotalAmount, m => m.MapFrom(s => s.TotalAmount))
                .ForMember(d => d.AmountLongForm, m => m.MapFrom(s => s.AmountLongForm))
                .ForMember(d => d.Date, m => m.MapFrom(s => s.Date))
                .ForMember(d => d.Time, m => m.MapFrom(s => s.Time))
                .ForMember(d => d.Expiration, m => m.MapFrom(s => s.Expiration))
                .ForMember(d => d.DeviceId, m => m.MapFrom(s => s.Id))
                .ForMember(d => d.OfflineReason, m => m.Ignore())
                .ForMember(d => d.Id, m => m.Ignore());

            CreateMap<Commands.VoucherPrinted, VoucherPrinted>()
                .ForMember(d => d.InstanceId, m => m.Ignore())
                .ForMember(d => d.VoucherBarcode, m => m.MapFrom(s => s.Barcode));
        }
    }
}