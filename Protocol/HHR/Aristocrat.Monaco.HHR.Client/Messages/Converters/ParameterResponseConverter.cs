namespace Aristocrat.Monaco.Hhr.Client.Messages.Converters
{
    using System;
    using AutoMapper;
    using Data;

    /// <summary>
    ///     Converter for <see cref="ParameterResponse" />
    /// </summary>
    public class ParameterResponseConverter : IResponseConverter<ParameterResponse>
    {
        private readonly IMapper _mapper;

        /// <summary>
        ///     Constructs the converter for <see cref="ParameterResponse" />
        /// </summary>
        /// <param name="mapper">For translating fields between the two objects that represent the same message.</param>
        public ParameterResponseConverter(IMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <inheritdoc />
        public ParameterResponse Convert(byte[] data)
        {
            return _mapper.Map<ParameterResponse>(MessageUtility.ConvertByteArrayToMessage<SMessageGtParameter>(data));
        }
    }
}