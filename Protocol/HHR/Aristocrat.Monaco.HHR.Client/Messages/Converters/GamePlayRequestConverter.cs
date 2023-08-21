namespace Aristocrat.Monaco.Hhr.Client.Messages.Converters
{
    using System;
    using AutoMapper;
    using Data;

    /// <summary>
    ///     Converter for <see cref="GamePlayRequest" />
    /// </summary>
    public class GamePlayRequestConverter : IRequestConverter<GamePlayRequest>
    {
        private readonly IMapper _mapper;

        /// <summary>
        ///     Constructs the converter for <see cref="GamePlayRequest" />
        /// </summary>
        /// <param name="mapper">For translating fields between the two objects that represent the same message.</param>
        public GamePlayRequestConverter(IMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        ///     Convert <see cref="GamePlayRequest" /> to byte representation
        /// </summary>
        /// <param name="message">Game play request message</param>
        /// <returns>Byte representation for the message</returns>
        public byte[] Convert(GamePlayRequest message)
        {
            return MessageUtility.ConvertMessageToByteArray(_mapper.Map<GMessageGamePlay>(message));
        }
    }
}