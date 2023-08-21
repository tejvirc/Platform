namespace Aristocrat.Monaco.Hhr.Client.Messages.Converters
{
    using System;
    using AutoMapper;
    using Data;

    /// <summary>
    ///     Converter for <see cref="GamePlayResponse" />
    /// </summary>
    public class GamePlayResponseConverter : IResponseConverter<GamePlayResponse>
    {
        private readonly IMapper _mapper;

        /// <summary>
        ///     Constructs the converter for <see cref="GamePlayResponse" />
        /// </summary>
        /// <param name="mapper">For translating fields between the two objects that represent the same message.</param>
        public GamePlayResponseConverter(IMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        ///     Convert byte data to <see cref="GamePlayResponse" />
        /// </summary>
        /// <param name="data">Data buffer representing <see cref="GamePlayResponse" /> </param>
        /// <returns>Game play response</returns>
        public GamePlayResponse Convert(byte[] data)
        {
            return _mapper.Map<GamePlayResponse>(MessageUtility.ConvertByteArrayToMessage<SMessageGameBonanza>(data));
        }
    }
}