namespace Aristocrat.Monaco.Hhr.Client.Messages.Converters
{
    using System;
    using AutoMapper;
    using Data;

    /// <summary>
    ///     Converter for <see cref="TransactionRequest" />
    /// </summary>
    public class TransactionRequestConverter : IRequestConverter<TransactionRequest>
    {
        private readonly IMapper _mapper;

        /// <summary>
        ///     Constructs the converter for <see cref="TransactionRequest" />
        /// </summary>
        /// <param name="mapper">For translating fields between the two objects that represent the same message.</param>
        public TransactionRequestConverter(IMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <inheritdoc />
        public byte[] Convert(TransactionRequest message)
        {
            return MessageUtility.ConvertMessageToByteArray(_mapper.Map<MessageTransaction>(message));
        }
    }
}