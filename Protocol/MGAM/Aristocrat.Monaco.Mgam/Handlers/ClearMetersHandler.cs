namespace Aristocrat.Monaco.Mgam.Handlers
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client.Messaging;
    using Services.DropMode;
    using Services.Meters;

    /// <summary>
    ///     Handles the <see cref="ClearMeters"/> message.
    /// </summary>
    public class ClearMetersHandler : MessageHandler<ClearMeters>
    {
        private readonly IDropMode _dropModeService;
        private readonly IMeterMonitor _meterMonitor;

        /// <summary>
        ///     Construct <see cref="ClearMetersHandler"/>.
        /// </summary>
        /// <param name="dropModeService">Instance of <see cref="IDropMode"/>.</param>
        /// <param name="meterMonitor"><see cref="IMeterMonitor"/></param>
        public ClearMetersHandler(
            IDropMode dropModeService,
            IMeterMonitor meterMonitor)
        {
            _dropModeService = dropModeService ?? throw new ArgumentNullException(nameof(dropModeService));
            _meterMonitor = meterMonitor ?? throw new ArgumentNullException(nameof(meterMonitor));
        }

        ///<inheritdoc />
        public override Task<IResponse> Handle(ClearMeters message)
        {
            _dropModeService.ClearMeters();

            _meterMonitor.SendAllAttributes();

            return Task.FromResult(Ok<ClearMetersResponse>());
        }
    }
}
