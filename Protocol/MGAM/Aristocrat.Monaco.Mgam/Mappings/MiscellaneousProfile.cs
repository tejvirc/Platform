namespace Aristocrat.Monaco.Mgam.Mappings
{
    using AutoMapper;
    using Commands;

    /// <summary>
    ///     Mapping configurations for "miscellaneous" messages (per protocol spec).
    /// </summary>
    public class MiscellaneousProfile : Profile
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MiscellaneousProfile" /> class.
        /// </summary>
        public MiscellaneousProfile()
        {
            CreateMap<BillAcceptorMeterReport, Aristocrat.Mgam.Client.Messaging.BillAcceptorMeterReport>()
                .ForMember(d => d.InstanceId, m => m.Ignore())
                .ForMember(d => d.CashBox, m => m.MapFrom(s => s.CashBox))
                .ForMember(d => d.CashBoxOnes, m => m.MapFrom(s => s.CashBoxOnes))
                .ForMember(d => d.CashBoxTwos, m => m.MapFrom(s => s.CashBoxTwos))
                .ForMember(d => d.CashBoxFives, m => m.MapFrom(s => s.CashBoxFives))
                .ForMember(d => d.CashBoxTens, m => m.MapFrom(s => s.CashBoxTens))
                .ForMember(d => d.CashBoxTwenties, m => m.MapFrom(s => s.CashBoxTwenties))
                .ForMember(d => d.CashBoxFifties, m => m.MapFrom(s => s.CashBoxFifties))
                .ForMember(d => d.CashBoxHundreds, m => m.MapFrom(s => s.CashBoxHundreds))
                .ForMember(d => d.CashBoxVouchers, m => m.MapFrom(s => s.CashBoxVouchers))
                .ForMember(d => d.CashBoxVouchersTotal, m => m.MapFrom(s => s.CashBoxVouchersTotal));
        }
    }
}