namespace Aristocrat.Monaco.Hhr.Client.Messages.Converters
{
    using System;
    using AutoMapper;
    using Data;

    /// <summary>
    ///     Converter for <see cref="CommandResponse" />
    /// </summary>
    public class CommandResponseConverter : IResponseConverter<CommandResponse>
    {
        private readonly IMapper _mapper;

        /// <summary>
        ///     Constructs the converter for <see cref="CommandResponse" />
        /// </summary>
        /// <param name="mapper">For translating fields between the two objects that represent the same message.</param>
        public CommandResponseConverter(IMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <inheritdoc />
        public CommandResponse Convert(byte[] data)
        {
            return _mapper.Map<SMessageCommand, CommandResponse>(
                MessageUtility.ConvertByteArrayToMessage<SMessageCommand>(data));
        }
    }
}
