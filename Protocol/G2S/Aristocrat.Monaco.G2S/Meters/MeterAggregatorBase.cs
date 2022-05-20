namespace Aristocrat.Monaco.G2S.Meters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Base class for a meter aggregator.
    /// </summary>
    /// <typeparam name="TDevice">The device type</typeparam>
    public abstract class MeterAggregatorBase<TDevice> : IMeterAggregator<TDevice>
        where TDevice : IDevice
    {
        private readonly IMeterManager _meterManager;
        private readonly IDictionary<string, string> _meterMap;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MeterAggregatorBase{TDevice}" /> class.
        /// </summary>
        /// <param name="meterManager">An instance of an IMeterManager.</param>
        /// <param name="meterMap">The G2S to platform meter map.</param>
        protected MeterAggregatorBase(IMeterManager meterManager, IDictionary<string, string> meterMap)
        {
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
            _meterMap = meterMap ?? throw new ArgumentNullException(nameof(meterMap));
        }

        /// <inheritdoc />
        public IEnumerable<meterInfo> GetMeters(TDevice device, params string[] includeMeters)
        {
            if (device == null)
            {
                return new List<meterInfo>();
            }

            return new List<meterInfo>
            {
                new meterInfo
                {
                    meterDateTime = DateTime.UtcNow,
                    meterInfoType = MeterInfoType.Event, // TODO: Pass this in?
                    deviceMeters =
                        new[]
                        {
                            new deviceMeters
                            {
                                deviceClass = device.PrefixedDeviceClass(),
                                deviceId = device.Id,
                                simpleMeter = GetSimpleMeters(includeMeters).ToArray()
                            }
                        }
                }
            };
        }

        /// <summary>
        ///     Provides a mechanism to override the meter value if the meter is not provided
        /// </summary>
        /// <param name="meter">The meter</param>
        /// <returns>The meter value</returns>
        protected virtual long GetValue(string meter)
        {
            return 0;
        }

        /// <summary>
        ///     Gets the meters if provided from the meter manager based on the provided meter map.
        /// </summary>
        /// <param name="includeMeters">Meters to include</param>
        /// <returns>A collection of simple meters</returns>
        protected IEnumerable<simpleMeter> GetSimpleMeters(params string[] includeMeters)
        {
            return _meterMap.Where(m => { return includeMeters != null && includeMeters.Any(i => i == m.Key); })
                .Select(
                    meter => new simpleMeter
                    {
                        meterName = meter.Key.StartsWith("G2S_", StringComparison.InvariantCultureIgnoreCase)
                            ? meter.Key
                            : $"G2S_{meter.Key}",
                        meterValue = _meterManager.IsMeterProvided(meter.Value)
                            ? _meterManager.GetMeter(meter.Value).Lifetime
                            : GetValue(meter.Value)
                    }).ToList();
        }
    }
}
