namespace Aristocrat.Monaco.Hhr.Client.Messages.Converters
{
    using System;
    using AutoMapper;
    using Data;

    /// <summary>
    ///     Converter for <see cref="CloseTranErrorResponse" />
    /// </summary>
    public class CloseTranErrorResponseConverter : IResponseConverter<CloseTranErrorResponse>
    {
        private readonly IMapper _mapper;

        /// <summary>
        ///     Constructs the converter for <see cref="PlayerIdResponse" />
        /// </summary>
        /// <param name="mapper">For translating fields between the two objects that represent the same message.</param>
        public CloseTranErrorResponseConverter(IMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <inheritdoc />
        public CloseTranErrorResponse Convert(byte[] data)
        {
            return _mapper.Map<MessageCloseTranError, CloseTranErrorResponse>(MessageUtility
                .ConvertByteArrayToMessage<MessageCloseTranError>(data));
        }
    }
}
