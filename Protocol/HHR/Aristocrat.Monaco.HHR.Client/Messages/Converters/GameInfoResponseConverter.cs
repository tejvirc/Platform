namespace Aristocrat.Monaco.Hhr.Client.Messages.Converters
{
    using System;
    using AutoMapper;
    using Data;

    /// <summary>
    ///     Converter for <see cref="GameInfoResponse" />
    /// </summary>
    public class GameInfoResponseConverter : IResponseConverter<GameInfoResponse>
    {
        private readonly IMapper _mapper;

        /// <summary>
        ///     Constructs the converter for <see cref="GameInfoResponse" />
        /// </summary>
        /// <param name="mapper">For translating fields between the two objects that represent the same message.</param>
        public GameInfoResponseConverter(IMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }


        /// <inheritdoc />
        public GameInfoResponse Convert(byte[] data)
        {
            return _mapper.Map<SMessageGameOpen, GameInfoResponse>(
                MessageUtility.ConvertByteArrayToMessage<SMessageGameOpen>(data));
        }
    }
}
