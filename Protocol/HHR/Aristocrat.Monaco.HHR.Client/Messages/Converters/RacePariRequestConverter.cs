namespace Aristocrat.Monaco.Hhr.Client.Messages.Converters
{
    using System;
    using AutoMapper;
    using Data;

    /// <summary>
    ///     Converter for <see cref="RacePariRequest" />
    /// </summary>
    public class RacePariRequestConverter : IRequestConverter<RacePariRequest>
    {
        private readonly IMapper _mapper;

        /// <summary>
        ///     Constructs the converter for <see cref="RacePariRequest" />
        /// </summary>
        /// <param name="mapper">For translating fields between the two objects that represent the same message.</param>
        public RacePariRequestConverter(IMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <inheritdoc />
        public byte[] Convert(RacePariRequest message)
        {
            return MessageUtility.ConvertMessageToByteArray(_mapper.Map<GMessageRacePariRequest>(message));
        }
    }
}