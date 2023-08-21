namespace Aristocrat.Monaco.Hhr.Client.Messages.Converters
{
    using System;
    using AutoMapper;
    using Data;

    /// <summary>
    ///     Converter for <see cref="ProgressiveInfoResponse" />
    /// </summary>
    public class ProgressiveInfoResponseConverter : IResponseConverter<ProgressiveInfoResponse>
    {
        private readonly IMapper _mapper;

        /// <summary>
        ///     Constructs the converter for <see cref="ProgressiveInfoRequest" />
        /// </summary>
        /// <param name="mapper">For translating fields between the two objects that represent the same message.</param>
        public ProgressiveInfoResponseConverter(IMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <inheritdoc />
        public ProgressiveInfoResponse Convert(byte[] data)
        {
            return _mapper.Map<ProgressiveInfoResponse>(MessageUtility.ConvertByteArrayToMessage<SMessageProgressiveInfo>(data));
        }
    }
}
