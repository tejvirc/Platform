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
        public int TimeToLive { get; protected set; }

        /// <inheritdoc />
        public bool RestartStatus { get; protected set; }

        /// <inheritdoc />
        public int NoResponseTimer { get; protected set; }

        /// <inheritdoc />
        public int NoMessageTimer { get; protected set; }


        /// <inheritdoc />
        public string NoHostText { get; protected set; }

        /// <inheritdoc />
        public override void ApplyOptions(DeviceOptionConfigValues optionConfigValues)
        {
            base.ApplyOptions(optionConfigValues);

            SetDeviceValue(
                G2SParametersNames.RestartStatusParameterName,
                optionConfigValues,
                parameterId => { RestartStatus = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.UseDefaultConfigParameterName,
                optionConfigValues,
                parameterId => { UseDefaultConfig = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.RequiredForPlayParameterName,
                optionConfigValues,
                parameterId => { RequiredForPlay = optionConfigValues.BooleanValue(parameterId); });

            SetDeviceValue(
                G2SParametersNames.TimeToLiveParameterName,
                optionConfigValues,
                parameterId => { TimeToLive = optionConfigValues.Int32Value(parameterId); });
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

        /// <inheritdoc />
        public override void RegisterEvents()
        {
            var deviceClass = this.PrefixedDeviceClass();

            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.ATI_ANE001);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.ATI_ANE002);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.ATI_ANE003);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.ATI_ANE004);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.ATI_ANE005);
            EventHandlerDevice.RegisterEvent(deviceClass, Id, EventCode.ATI_ANE006);
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
        public trackAck SendTrack(track command)
        {
            throw new NotImplementedException();
        }

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
