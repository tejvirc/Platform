namespace Aristocrat.Monaco.Hhr.Client.Messages.Converters
{
    using System;
    using AutoMapper;
    using Data;

    /// <summary>
    ///     Converter for <see cref="ConnectRequest" />
    /// </summary>
    public class ConnectRequestConverter : IRequestConverter<ConnectRequest>
    {
        private readonly IMapper _mapper;

        /// <summary>
        ///     Constructs the converter for <see cref="ConnectRequest" />
        /// </summary>
        /// <param name="mapper">For translating fields between the two objects that represent the same message.</param>
        public ConnectRequestConverter(IMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <inheritdoc />
        public byte[] Convert(ConnectRequest message)
        {
            return MessageUtility.ConvertMessageToByteArray(_mapper.Map<ConnectRequest, MessageConnect>(message));
        }
    }
}
