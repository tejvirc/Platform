namespace Aristocrat.Monaco.Hhr.Client.Messages.Converters
{
    using System;
    using AutoMapper;
    using Data;

    /// <summary>
    ///     Converter for <see cref="GamePlayResponse" />
    /// </summary>
    public class GameProgressiveUpdateResponseConverter : IResponseConverter<GameProgressiveUpdate>
    {
        private readonly IMapper _mapper;

        /// <summary>
        ///     Constructs the converter for <see cref="GamePlayResponse" />
        /// </summary>
        /// <param name="mapper"></param>
        public GameProgressiveUpdateResponseConverter(IMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        ///     Convert byte data to <see cref="GamePlayResponse" />
        /// </summary>
        /// <param name="data">Data buffer representing <see cref="GamePlayResponse" /> </param>
        /// <returns>Game play response</returns>
        public GameProgressiveUpdate Convert(byte[] data)
        {
            return _mapper.Map<GameProgressiveUpdate>(MessageUtility.ConvertByteArrayToMessage<SMessageProgressivePrize>(data));
        }
    }
}
