namespace Aristocrat.Monaco.Hhr.Client.Messages.Converters
{
    using System;
    using AutoMapper;
    using Data;

    /// <summary>
    ///     Converter for <see cref="PlayerIdResponse" />
    /// </summary>
    public class PlayerIdResponseConverter : IResponseConverter<PlayerIdResponse>
    {
        private readonly IMapper _mapper;

        /// <summary>
        ///     Constructs the converter for <see cref="PlayerIdResponse" />
        /// </summary>
        /// <param name="mapper">For translating fields between the two objects that represent the same message.</param>
        public PlayerIdResponseConverter(IMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <inheritdoc />
        public PlayerIdResponse Convert(byte[] data)
        {
            return _mapper.Map<PlayerIdResponse>(
                MessageUtility.ConvertByteArrayToMessage<SMessagePlayerRequestResponse>(data));
        }
    }
}