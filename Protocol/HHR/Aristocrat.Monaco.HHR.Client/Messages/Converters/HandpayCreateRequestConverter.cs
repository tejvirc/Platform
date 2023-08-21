namespace Aristocrat.Monaco.Hhr.Client.Messages.Converters
{
    using System;
    using AutoMapper;
    using Data;

    /// <summary>
    ///     Converter for <see cref="HandpayCreateRequest" />
    /// </summary>
    public class HandpayCreateRequestConverter : IRequestConverter<HandpayCreateRequest>
    {
        private readonly IMapper _mapper;

        /// <summary>
        ///     Constructs the converter for <see cref="HandpayCreateRequest" />
        /// </summary>
        /// <param name="mapper">For translating fields between the two objects that represent the same message.</param>
        public HandpayCreateRequestConverter(IMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <inheritdoc />
        public byte[] Convert(HandpayCreateRequest message)
        {
            return MessageUtility.ConvertMessageToByteArray(_mapper.Map<GMessageCreateHandPayItem>(message));
        }
    }
}