namespace Aristocrat.Monaco.Hardware.EdgeLight.Manager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Hardware.Contracts.EdgeLighting;
    using StripBrightnessData =
        System.Collections.Generic.SortedList<Hardware.Contracts.EdgeLighting.StripPriority, int>;
    using StripBrightnessDataMap =
        System.Collections.Generic.Dictionary<int,
            System.Collections.Generic.SortedList<Hardware.Contracts.EdgeLighting.StripPriority, int>>;

    internal sealed class EdgeLightBrightnessData : IDisposable
    {
        private const int StripIdAll = -1;

        private readonly Dictionary<StripPriority, EdgeLightingBrightnessLimits> _brightnessLimits =
            new Dictionary<StripPriority, EdgeLightingBrightnessLimits>();

        private readonly EdgeLightingBrightnessLimits _defaultLimits =
            new EdgeLightingBrightnessLimits
            {
                MaximumAllowed = EdgeLightingBrightnessLimits.MaximumBrightness,
                MinimumAllowed = EdgeLightingBrightnessLimits.MinimumBrightness
            };

        private readonly PriorityComparer _priorityComparer;

        private StripBrightnessDataMap _stripBrightnessDataMap;

        private bool _disposed;

        public EdgeLightBrightnessData(PriorityComparer priorityComparer)
        {
            _priorityComparer = priorityComparer ?? throw new ArgumentNullException(nameof(priorityComparer));
            _stripBrightnessDataMap = new StripBrightnessDataMap
            {
                { StripIdAll, new StripBrightnessData(_priorityComparer) }
            };
            _priorityComparer.ComparerChanged += PriorityComparerChanged;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (_priorityComparer != null)
            {
                _priorityComparer.ComparerChanged -= PriorityComparerChanged;
            }

            _disposed = true;
        }

        public void SetBrightnessLimits(EdgeLightingBrightnessLimits limits, StripPriority forPriority)
        {
            _brightnessLimits[forPriority] = limits;
        }

        public EdgeLightingBrightnessLimits GetBrightnessLimits(StripPriority forPriority)
        {
            return _brightnessLimits.TryGetValue(forPriority, out var value) ? value : _defaultLimits;
        }

        public void SetBrightnessForPriority(int brightness, StripPriority priority)
        {
            // Remove individual strip settings for priority.
            RemovePriority(priority);
            _stripBrightnessDataMap[StripIdAll][priority] =
                EdgeLightingBrightnessLimits.TruncateBrightnessValue(brightness);
        }

        public void ClearBrightnessForPriority(StripPriority priority)
        {
            RemovePriority(priority);
        }

        public void SetStripBrightnessForPriority(int stripId, int brightness, StripPriority priority)
        {
            if (!_stripBrightnessDataMap.TryGetValue(stripId, out var brightnessData))
            {
                brightnessData = new StripBrightnessData(_priorityComparer);
                _stripBrightnessDataMap.Add(stripId, brightnessData);
            }

            brightnessData[priority] = EdgeLightingBrightnessLimits.TruncateBrightnessValue(brightness);
        }

        public void ClearStripBrightnessForPriority(int stripId, StripPriority priority)
        {
            if (!_stripBrightnessDataMap.TryGetValue(stripId, out var brightnessData))
            {
                return;
            }

            brightnessData.Remove(priority);
            if (brightnessData.Count == 0)
            {
                _stripBrightnessDataMap.Remove(stripId);
            }
        }

        public int GetSystemBrightness(int defaultValue)
        {
            return GetFromBrightnessData(_stripBrightnessDataMap[StripIdAll], defaultValue);
        }

        public int GetBrightness(int stripId, int defaultValue)
        {
            var systemBrightness = _stripBrightnessDataMap[StripIdAll];
            if (!_stripBrightnessDataMap.TryGetValue(stripId, out var brightnessData) || !brightnessData.Any())
            {
                return GetFromBrightnessData(systemBrightness, defaultValue);
            }

            var stripBrightness = brightnessData.Last();
            return systemBrightness.Any() && systemBrightness.Last().Key > stripBrightness.Key
                ? GetFromBrightnessData(systemBrightness, defaultValue)
                : stripBrightness.Value;
        }

        private int GetFromBrightnessData(StripBrightnessData data, int defaultValue)
        {
            return data.Any()
                ? LimitBrightness(data.Last().Value, data.Last().Key)
                : defaultValue;
        }

        private void RemovePriority(StripPriority priority)
        {
            var temporaryMap = new StripBrightnessDataMap(_stripBrightnessDataMap);
            foreach (var brightnessData in temporaryMap)
            {
                brightnessData.Value.Remove(priority);
                if (!brightnessData.Value.Any() && brightnessData.Key != StripIdAll)
                {
                    _stripBrightnessDataMap.Remove(brightnessData.Key);
                }
            }
        }

        private int LimitBrightness(int brightness, StripPriority priority)
        {
            brightness = EdgeLightingBrightnessLimits.TruncateBrightnessValue(brightness);
            if (!_brightnessLimits.TryGetValue(priority, out var value))
            {
                value = _defaultLimits;
            }

            var brightnessFactor = Math.Abs(value.MaximumAllowed - value.MinimumAllowed) / (double)
                EdgeLightConstants.MaxChannelBrightness;
            return value.MinimumAllowed + (int)(brightnessFactor * brightness);
        }

        private void PriorityComparerChanged(object sender, EventArgs e)
        {
            _stripBrightnessDataMap = _stripBrightnessDataMap.ToDictionary(
                stripRendererData => stripRendererData.Key,
                stripRendererData => new StripBrightnessData(stripRendererData.Value, _priorityComparer));
        }
    }
}