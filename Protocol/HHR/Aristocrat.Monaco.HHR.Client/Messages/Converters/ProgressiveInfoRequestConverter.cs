namespace Aristocrat.Monaco.Hhr.Client.Messages.Converters
{
    using System;
    using AutoMapper;
    using Data;

    /// <summary>
    ///     Converter for <see cref="ProgressiveInfoRequest" />
    /// </summary>
    public class ProgressiveInfoRequestConverter : IRequestConverter<ProgressiveInfoRequest>
    {
        private readonly IMapper _mapper;

        /// <summary>
        ///     Constructs the converter for <see cref="ProgressiveInfoRequest" />
        /// </summary>
        /// <param name="mapper">For translating fields between the two objects that represent the same message.</param>
        public ProgressiveInfoRequestConverter(IMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <inheritdoc />
        public byte[] Convert(ProgressiveInfoRequest message)
        {
            return MessageUtility.ConvertMessageToByteArray(_mapper.Map<ProgressiveInfoRequest, GMessageProgRequest>(message));
        }
    }
}
