namespace Aristocrat.Monaco.Hhr.Client.Messages.Converters
{
    using System;
    using AutoMapper;
    using Data;

    /// <summary>
    ///     Converter for <see cref="GameInfoRequest" />
    /// </summary>
    public class GameInfoRequestConverter : IRequestConverter<GameInfoRequest>
    {
        private readonly IMapper _mapper;

        /// <summary>
        ///     Constructs the converter for <see cref="GameInfoRequest" />
        /// </summary>
        /// <param name="mapper">For translating fields between the two objects that represent the same message.</param>
        public GameInfoRequestConverter(IMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <inheritdoc />
        public byte[] Convert(GameInfoRequest message)
        {
            return MessageUtility.ConvertMessageToByteArray(_mapper.Map<GameInfoRequest, GMessageGameRequest>(message));
        }
    }
}
