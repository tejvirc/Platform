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
        private readonly Dictionary<string, TrackInterval> _trackCommandIntervals;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AnalyticsDevice" /> class.
        /// </summary>
        /// <param name="deviceId">The device ID</param>
        /// <param name="deviceObserver">An IDeviceObserver instance.</param>
        public AnalyticsDevice(int deviceId, IDeviceObserver deviceObserver)
            : base(deviceId, deviceObserver)
        {
            _trackCommandIntervals = new Dictionary<string, TrackInterval>();
        }

        /// <inheritdoc />
        public trackAck SendTrack(track command)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void SetTrackInterval(string action, string category, TimeSpan interval)
        {
            var key = BuildActionCategoryKey(action, category);

            if (interval.TotalMilliseconds == 0)
            {
                //interval 0 means no limit.
                _trackCommandIntervals.Remove(key);
            }
            else if (_trackCommandIntervals.TryGetValue(key, out var trackInterval))
            {
                trackInterval.Interval = interval;
            }
            else
            {
                _trackCommandIntervals[key] = new TrackInterval(interval);
            }
        }

        private static string BuildActionCategoryKey(string action, string category) => $"{action}_{category}";

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

        private class TrackInterval
        {
            private DateTime _lastSent = DateTime.MinValue;

            public TimeSpan Interval { get; set; }

            public TrackInterval(TimeSpan interval)
            {
                Interval = interval;
            }

            public void UpdateLastSent() => _lastSent = DateTime.Now;

            public bool CanSend()
            {
                return (DateTime.Now - _lastSent) > Interval;
            }
        };
    }
}
