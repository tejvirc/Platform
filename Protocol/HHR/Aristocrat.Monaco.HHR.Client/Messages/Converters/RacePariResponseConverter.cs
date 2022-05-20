namespace Aristocrat.Monaco.Hhr.Client.Messages.Converters
{
    using System;
    using AutoMapper;
    using Data;

    /// <summary>
    ///     Converter for <see cref="RacePariResponse" />
    /// </summary>
    public class RacePariResponseConverter : IResponseConverter<RacePariResponse>
    {
        private readonly IMapper _mapper;

        /// <summary>
        ///     Constructs the converter for <see cref="RacePariResponse" />
        /// </summary>
        /// <param name="mapper">For translating fields between the two objects that represent the same message.</param>
        public RacePariResponseConverter(IMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <inheritdoc />
        public RacePariResponse Convert(byte[] data)
        {
            return _mapper.Map<RacePariResponse>(
                MessageUtility.ConvertByteArrayToMessage<GMessageRacePariResponse>(data));
        }
    }
}