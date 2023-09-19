namespace Aristocrat.G2S.Client.Devices.v21
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Controls.Primitives;
    using Protocol.v21;

    /// <summary>
    ///     create class AnalyticsDevice
    /// </summary>
    public class AnalyticsDevice : ClientDeviceBase<analytics>, IAnalyticsDevice
    {

        /// <summary>
        ///     A dictionary for the track command intervals
        ///     with the key in the format "action_category"
        /// </summary>
        private Dictionary<string, long> _trackCommandIntervals;

        /// <summary>
        ///     A dictionary for the last time a track command was sent
        ///     with the key in the format "action_category"
        /// </summary>
        private Dictionary<string, DateTime> _trackCommandLastSentTimes;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AnalyticsDevice" /> class.
        /// </summary>
        /// <param name="deviceId">The device ID</param>
        /// <param name="deviceObserver">An IDeviceObserver instance.</param>
        public AnalyticsDevice(int deviceId, IDeviceObserver deviceObserver)
            : base(deviceId, deviceObserver)
        {
            _trackCommandIntervals = new Dictionary<string, long>();
            _trackCommandLastSentTimes = new Dictionary<string, DateTime>();
        }

        /// <inheritdoc />
        public trackAck SendTrack(track command)
        {
            throw new NotImplementedException();
        }

        private string GetActionCategoryKey(string action, string category)
        {
            return action + "_" + category;
        }

        /// <inheritdoc />
        public override void Open(IStartupContext context)
        {
        }

        /// <inheritdoc />
        public override void Close()
        {
        }

        /// <inheritdoc />
        public bool RestartStatus { get; protected set; }
    }
}
