namespace Aristocrat.Monaco.Mgam.Handlers
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client.Messaging;
    using Services.Meters;

    /// <summary>
    ///     Handle the <see cref="UpdateMeters"/> message.
    /// </summary>
    public class UpdateMetersHandler : MessageHandler<UpdateMeters>
    {
        private readonly IMeterMonitor _meterMonitor;

        /// <summary>
        ///     Construct <see cref="UpdateMetersHandler"/>.
        /// </summary>
        /// <param name="meterMonitor"><see cref="IMeterMonitor"/></param>
        public UpdateMetersHandler(
            IMeterMonitor meterMonitor)
        {
            _meterMonitor = meterMonitor ?? throw new ArgumentNullException(nameof(meterMonitor));
        }

        ///<inheritdoc />
        public override async Task<IResponse> Handle(UpdateMeters message)
        {
            _meterMonitor.SendAllAttributes();

            return await Task.FromResult(Ok<UpdateMetersResponse>());
        }
    }
}
