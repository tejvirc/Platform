namespace Aristocrat.Monaco.Hhr.Client.Messages.Converters
{
    using System;
    using AutoMapper;
    using Data;

    /// <summary>
    ///     Converter for <see cref="RaceStartRequest" />
    /// </summary>
    public class RaceStartRequestConverter : IRequestConverter<RaceStartRequest>
    {
        private readonly IMapper _mapper;

        /// <summary>
        ///     Constructs the converter for <see cref="RaceStartRequest" />
        /// </summary>
        /// <param name="mapper">For translating fields between the two objects that represent the same message.</param>
        public RaceStartRequestConverter(IMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Convert <see cref="RaceStartRequest" />  to byte representation
        /// </summary>
        /// <param name="message">Race start request message</param>
        /// <returns>Byte representation for the message</returns>
        public byte[] Convert(RaceStartRequest message)
        {
            return MessageUtility.ConvertMessageToByteArray(_mapper.Map<GMessageRaceStart>(message));
        }
    }
}