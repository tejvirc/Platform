namespace Aristocrat.Monaco.Hhr.Client.Mappings
{
    using System;
    using AutoMapper;
    using Data;
    using Messages;

    /// <summary>
    ///     AutoMapper profile for Response
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public class ResponseProfile : Profile
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ResponseProfile" /> class.
        /// </summary>
        public ResponseProfile()
        {
            CreateMap<SMessageGameBonanza, GamePlayResponse>()
                .ForMember(d => d.Command, m => m.Ignore())
                .ForMember(d => d.ReplyId, m => m.Ignore())
                .ForMember(d => d.MessageStatus, m => m.Ignore());

            CreateMap<SMessageGtParameter, ParameterResponse>()
                .ForMember(d => d.Command, m => m.Ignore())
                .ForMember(d => d.ReplyId, m => m.Ignore())
                .ForMember(d => d.MessageStatus, m => m.Ignore())
                .ForMember(d => d.EzBetFlag, m => m.MapFrom(s => s.EZBetFlag == 1))
                .ForMember(d => d.LastTransactionId, m => m.MapFrom(s => s.lastTransId))
                .ForMember(d => d.ManualHandicapMode, m => m.MapFrom(s => s.sysReservedInt2));


            CreateMap<SMessagePlayerRequestResponse, PlayerIdResponse>()
                .ForMember(d => d.Command, m => m.Ignore())
                .ForMember(d => d.ReplyId, m => m.Ignore())
                .ForMember(d => d.MessageStatus, m => m.Ignore())
                .ForMember(d => d.PlayerId, m => m.MapFrom(s => s.PlayerId));

            CreateMap<SMessageGameOpen, GameInfoResponse>()
                .ForMember(d => d.Command, m => m.Ignore())
                .ForMember(d => d.ReplyId, m => m.Ignore())
                .ForMember(d => d.MessageStatus, m => m.Ignore())
                .ForMember(d => d.PrizeLocations, m => m.Ignore());

            CreateMap<MessageCloseTranError, CloseTranErrorResponse>()
                .ForMember(d => d.Status, m => m.MapFrom(s => (Status)s.Status))
                .ForMember(d => d.RetryTime, m => m.MapFrom(s => TimeSpan.FromMilliseconds(s.RetryTime)))
                .ForMember(d => d.Command, m => m.Ignore())
                .ForMember(d => d.ReplyId, m => m.Ignore())
                .ForMember(d => d.MessageStatus, m => m.Ignore());

            CreateMap<MessageCloseTran, CloseTranResponse>()
                .ForMember(d => d.Status, m => m.MapFrom(s => (Status)s.Status))
                .ForMember(d => d.Command, m => m.Ignore())
                .ForMember(d => d.ReplyId, m => m.Ignore())
                .ForMember(d => d.MessageStatus, m => m.Ignore());

            CreateMap<SMessageCommand, CommandResponse>()
                .ForMember(d => d.ECommand, m => m.MapFrom(s => (GtCommand)s.ECommand))
                .ForMember(d => d.Command, m => m.Ignore())
                .ForMember(d => d.ReplyId, m => m.Ignore())
                .ForMember(d => d.MessageStatus, m => m.Ignore());

            CreateMap<GMessageRacePariResponse, RacePariResponse>()
                .ForMember(d => d.Command, m => m.Ignore())
                .ForMember(d => d.ReplyId, m => m.Ignore())
                .ForMember(d => d.MessageStatus, m => m.Ignore())
                .ForMember(d => d.TemplatePool, m => m.MapFrom(s => s.Data));

            CreateMap<SMessageProgressiveInfo, ProgressiveInfoResponse>()
                .ForMember(d => d.Command, m => m.Ignore())
                .ForMember(d => d.ReplyId, m => m.Ignore())
                .ForMember(d => d.MessageStatus, m => m.Ignore());

            CreateMap<SMessageProgressivePrize, GameProgressiveUpdate>()
                .ForMember(d => d.Command, m => m.Ignore())
                .ForMember(d => d.ReplyId, m => m.Ignore())
                .ForMember(d => d.MessageStatus, m => m.Ignore());

            CreateMap<GMessageGameRecoverResponse, GameRecoveryResponse>()
                .ForMember(d => d.Command, m => m.Ignore())
                .ForMember(d => d.ReplyId, m => m.Ignore())
                .ForMember(d => d.MessageStatus, m => m.Ignore());
        }
    }
}