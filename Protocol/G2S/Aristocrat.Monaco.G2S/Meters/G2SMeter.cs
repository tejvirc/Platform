namespace Aristocrat.Monaco.G2S.Meters
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Aristocrat.G2S.Protocol.v21;
    using Kernel;

    /// <summary>
    ///     Maps meter to G2S meter
    /// </summary>
    public class G2SMeter
    {
        private const string BetLevelNameSuffix = "AtBetLevel";
        private const string WagerCategoryNameSuffix = "AtWagerCategory";
        private const string DenominationMeterNamePrefix = "BillCount";
        private const string DenominationMeterNamePostfix = "s";
        private const string GameTag = "Game";
        private readonly Func<long> _callback;
        private readonly string _g2SMeterId;
        private readonly MeterValueType _meterValueType;
        private readonly string _trimmedG2SId;
        private readonly bool _meterIncreasing = true;
        private long _meterRollover = 999999999999999; //9,999,999,999.99999

        /// <summary>
        ///     Initializes a new instance of the <see cref="G2SMeter" /> class.
        /// </summary>
        /// <param name="game2SystemMeterId">G2S Meter Id</param>
        /// <param name="meterId">Meter Id</param>
        /// <param name="meterValueType">Meter value type</param>
        public G2SMeter(
            string game2SystemMeterId,
            string meterId,
            MeterValueType meterValueType = MeterValueType.Lifetime)
        {
            _g2SMeterId = game2SystemMeterId;
            MeterId = meterId;
            _meterValueType = meterValueType;
            _trimmedG2SId = _g2SMeterId.Split(new[] { "." }, StringSplitOptions.None)[1];
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="G2SMeter" /> class.
        /// </summary>
        /// <param name="game2SystemMeterId">G2S Meter Id</param>
        /// <param name="callback">Callback</param>
        public G2SMeter(string game2SystemMeterId, Func<long> callback)
        {
            _g2SMeterId = game2SystemMeterId;
            _callback = callback;
            _trimmedG2SId = _g2SMeterId.Split(new[] { "." }, StringSplitOptions.None)[1];
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="G2SMeter" /> class.
        /// </summary>
        /// <param name="game2SystemMeterId">G2S Meter Id</param>
        public G2SMeter(string game2SystemMeterId)
        {
            _g2SMeterId = game2SystemMeterId;
            _trimmedG2SId = _g2SMeterId.Split(new[] { "." }, StringSplitOptions.None)[1];
        }

        /// <summary>
        ///     Gets Meter Id
        /// </summary>
        public string MeterId { get; }

        /// <summary>
        ///     Creates G2S simple meter
        /// </summary>
        /// <param name="meters">Meter snapshot</param>
        /// <param name="meterManager">Meter Manager.</param>
        /// <param name="deviceId">Device Id.</param>
        /// <param name="denom">Denomination</param>
        /// <param name="propertiesManager">Properties manager.</param>
        /// <param name="valueOverride">Flag to return empty meter.</param>
        /// <param name="includeDefinition">Flag to include meter definitions.</param>
        /// <param name="wagerCategory">An optional wager category</param>
        /// <returns>G2S simple meter</returns>
        public simpleMeter GetMeter(
            Dictionary<string, MeterSnapshot> meters,
            IMeterManager meterManager,
            int deviceId = 0,
            long denom = 0,
            IPropertiesManager propertiesManager = null,
            bool valueOverride = false,
            bool includeDefinition = false,
            string wagerCategory = "")
        {
            var meterName = _trimmedG2SId.Contains("G2S_") ? _trimmedG2SId : "G2S_" + _g2SMeterId;
            meterName = meterName.TrimEnd(deviceId.ToString().ToCharArray());
            var meterValue = 0L;

            if (valueOverride)
            {
                return SimpleMeter(meterName, meterValue, includeDefinition);
            }

            if (MeterId == null && _callback != null)
            {
                meterValue = _callback.Invoke();
            }
            else if (MeterId != null)
            {
                string meterId;

                if (denom != 0)
                {
                    meterId = $"{MeterId}{BetLevelNameSuffix}{denom}";
                }
                else if (!string.IsNullOrEmpty(wagerCategory))
                {
                    meterId = $"{MeterId}{GameTag}{deviceId}{WagerCategoryNameSuffix}{wagerCategory}";
                }
                else
                {
                    meterId = MeterId;
                }

                if (meters.ContainsKey(meterId))
                {
                    meterValue = GetMeterValues(meters, meterId, meterManager, includeDefinition);
                }
                else
                {
                    if (propertiesManager != null)
                    {
                        if (meterName.Contains("currency"))
                        {
                            var denominationToCurrencyMultiplier =
                                (double)propertiesManager.GetProperty(ApplicationConstants.CurrencyMultiplierKey, ApplicationConstants.DefaultCurrencyMultiplier);
                            var denomination = (long)(denom / denominationToCurrencyMultiplier);
                            var meter = DenominationMeterNamePrefix + denomination + DenominationMeterNamePostfix;
                            if (meters.ContainsKey(meter))
                            {
                                meterValue = GetMeterValues(meters, meter, meterManager, includeDefinition);

                                if (_g2SMeterId.Contains("Amt"))
                                {
                                    meterValue *= denom;

                                    var currencyMeter = new CurrencyMeterClassification();
                                    var rollOver = _meterRollover * denom;
                                    _meterRollover = rollOver < currencyMeter.UpperBounds && rollOver >= 0L
                                        ? rollOver
                                        : currencyMeter.UpperBounds - 1;
                                }
                            }
                        }
                        else
                        {
                            if (meters.ContainsKey(MeterId))
                            {
                                meterValue = GetMeterValues(meters, MeterId, meterManager, includeDefinition);
                            }
                        }
                    }
                }
            }

            return SimpleMeter(meterName, meterValue, includeDefinition);
        }

        private long GetMeterValues(
            Dictionary<string, MeterSnapshot> meters,
            string meterId,
            IMeterManager meterManager,
            bool includeDefinition)
        {
            if (includeDefinition)
            {
                var realMeter = meterManager.GetMeter(meterId);
                _meterRollover = realMeter.Classification.UpperBounds - 1;
            }

            return meters[meterId].Values[_meterValueType];
        }

        private simpleMeter SimpleMeter(string meterName, long meterValue, bool includeDefinition)
        {
            return new simpleMeter
            {
                meterType = _g2SMeterId.Contains("Amt")
                    ? t_meterTypes.G2S_amount
                    : _g2SMeterId.Contains("Pct")
                        ? t_meterTypes.G2S_percent
                        : t_meterTypes.G2S_count,
                meterTypeSpecified = includeDefinition,
                meterIncreasing = _meterIncreasing,
                meterIncreasingSpecified = includeDefinition,
                meterRollover = _meterRollover,
                meterRolloverSpecified = includeDefinition,
                meterName = meterName,
                meterValue = meterValue
            };
        }
    }
}