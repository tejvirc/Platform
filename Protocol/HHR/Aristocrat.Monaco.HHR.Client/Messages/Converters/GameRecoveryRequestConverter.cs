namespace Aristocrat.Monaco.Hhr.Client.Messages.Converters
{
    using System;
    using AutoMapper;
    using Data;

    /// <summary>
    ///     Request converter for GameRecoveryRequest
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public class GameRecoveryRequestConverter : IRequestConverter<GameRecoveryRequest>
    {
        private readonly IMapper _mapper;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapper"></param>
        public GameRecoveryRequestConverter(IMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <inheritdoc />
        public byte[] Convert(GameRecoveryRequest message)
        {
            return MessageUtility.ConvertMessageToByteArray(_mapper.Map<GMessageGameRecover>(message));
        }
    }
}
