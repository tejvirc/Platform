namespace Aristocrat.Monaco.Hhr.Client.Messages.Converters
{
    using System;
    using AutoMapper;
    using Data;

    /// <summary>
    ///     Converter for <see cref="ParameterRequest" />
    /// </summary>
    public class ParameterRequestConverter : IRequestConverter<ParameterRequest>
    {
        private readonly IMapper _mapper;

        /// <summary>
        ///     Constructs the converter for <see cref="ParameterRequest" />
        /// </summary>
        /// <param name="mapper">For translating fields between the two objects that represent the same message.</param>
        public ParameterRequestConverter(IMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <inheritdoc />
        public byte[] Convert(ParameterRequest message)
        {
            return MessageUtility.ConvertMessageToByteArray(_mapper.Map<MessageParameterRequest>(message));
        }
    }
}