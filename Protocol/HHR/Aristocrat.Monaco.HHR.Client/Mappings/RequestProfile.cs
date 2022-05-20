namespace Aristocrat.Monaco.Hhr.Client.Mappings
{
    using AutoMapper;
    using Data;
    using Messages;

    /// <summary>
    ///     Mapping configurations for Request messages.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public class RequestProfile : Profile
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RequestProfile" /> class.
        /// </summary>
        public RequestProfile()
        {
            CreateMap<GamePlayRequest, GMessageGamePlay>()
                .ForMember(d => d.ForceCheck, m => m.MapFrom(s => s.ForceCheck ? 1u : 0u))
                .ForMember(d => d.FreePlayMode, m => m.Ignore())
                .ForMember(d => d.ForcePattern, m => m.Ignore());

            CreateMap<ParameterRequest, MessageParameterRequest>()
                .ForMember(d => d.DeviceType, m => m.MapFrom(s => "GT"))
                .ForMember(d => d.ActivePlayer, m => m.Ignore())
                .ForMember(d => d.CardNo, m => m.Ignore());

            CreateMap<TransactionRequest, MessageTransaction>()
                .ForMember(d => d.TransType, m => m.MapFrom(s => (byte)s.TransactionType))
                .ForMember(d => d.PTAccountId, m => m.Ignore())
                .ForMember(d => d.LinesPlayed, m => m.Ignore())
                .ForMember(d => d.BetPerLine, m => m.Ignore())
                .ForMember(d => d.TotalCreditsBet, m => m.Ignore())
                .ForMember(d => d.PriorCashBalance, m => m.Ignore())
                .ForMember(d => d.PriorNonCashBalance, m => m.Ignore());

            CreateMap<HandpayCreateRequest, GMessageCreateHandPayItem>()
                .ForMember(d => d.BonusWin, m => m.Ignore())
                .ForMember(d => d.MachinePaid, m => m.Ignore())
                .ForMember(d => d.MaxBetValue, m => m.Ignore())
                .ForMember(d => d.CreditMeterKeyOff, m => m.Ignore())
                .ForMember(d => d.VoucherKeyOff, m => m.Ignore())
                .ForMember(d => d.RemoteKeyOff, m => m.Ignore());

            CreateMap<ConnectRequest, MessageConnect>()
                .ForMember(d => d.DeviceType, m => m.MapFrom(x => 2));

            CreateMap<GameInfoRequest, GMessageGameRequest>()
                .ForMember(d => d.GameId, m => m.MapFrom(x => x.GameId));

            CreateMap<RacePariRequest, GMessageRacePariRequest>();

            CreateMap<RaceStartRequest, GMessageRaceStart>();

            CreateMap<ProgressiveInfoRequest, GMessageProgRequest>();

            CreateMap<GameRecoveryRequest, GMessageGameRecover>();
        }
    }
}