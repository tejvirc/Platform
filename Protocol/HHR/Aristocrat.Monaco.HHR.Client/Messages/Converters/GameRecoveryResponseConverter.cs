namespace Aristocrat.Monaco.Hhr.Client.Messages.Converters
{
    using System;
    using AutoMapper;
    using Data;

    /// <summary>
    ///     Response converter for GameRecoveryResponse
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public class GameRecoveryResponseConverter : IResponseConverter<GameRecoveryResponse>
    {
        private readonly IMapper _mapper;

        /// <summary>
        ///     
        /// </summary>
        /// <param name="mapper"></param>
        public GameRecoveryResponseConverter(IMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        /// <inheritdoc />
        public GameRecoveryResponse Convert(byte[] data)
        {
            return _mapper.Map<GameRecoveryResponse>(MessageUtility.ConvertByteArrayToMessage<GMessageGameRecoverResponse>(data));
        }
    }
}
