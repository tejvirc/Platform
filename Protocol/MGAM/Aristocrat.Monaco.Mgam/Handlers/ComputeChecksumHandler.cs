namespace Aristocrat.Monaco.Mgam.Handlers
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client.Messaging;
    using Services.Security;

    /// <summary>
    ///     Handles the <see cref="ComputeChecksum"/> message.
    /// </summary>
    public class ComputeChecksumHandler : MessageHandler<ComputeChecksum>
    {
        private readonly IChecksumCalculator _checksumCalculator;

        /// <summary>
        ///     Creates instance of <see cref="ComputeChecksumHandler"/>.
        /// </summary>
        /// <param name="checksumCalculator">Checksum Calculator</param>
        public ComputeChecksumHandler(
            IChecksumCalculator checksumCalculator)
        {
            _checksumCalculator = checksumCalculator ?? throw new ArgumentNullException(nameof(checksumCalculator));
        }

        ///<inheritdoc />
        public override async  Task<IResponse> Handle(ComputeChecksum message)
        {
            await _checksumCalculator.Calculate(message.Seed);

            return Ok<ComputeChecksumResponse>();
        }
    }
}
