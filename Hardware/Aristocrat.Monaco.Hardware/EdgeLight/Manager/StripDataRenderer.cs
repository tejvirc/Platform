namespace Aristocrat.Monaco.Hardware.EdgeLight.Manager
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using Contracts;
    using Hardware.Contracts.EdgeLighting;
    using Strips;

    /// <summary>
    ///     Utility class to assist in rendering and transformation of strip data
    /// </summary>
    internal sealed class StripDataRenderer : IDisposable
    {
        private readonly PriorityComparer _priorityComparer;

        private readonly Dictionary<int, StripRendererData> _stripData =
            new Dictionary<int, StripRendererData>();

        private bool _disposed;

        public StripDataRenderer(PriorityComparer priorityComparer)
        {
            _priorityComparer = priorityComparer ?? throw new ArgumentNullException(nameof(priorityComparer));
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

        public void SetColor(
            int stripId,
            Color color,
            StripPriority priority)
        {
            GetStripRenderData(stripId, true).SetColor(color, priority);
        }

        public void SetColorBuffer(
            int stripId,
            LedColorBuffer data,
            int sourceColorIndex,
            int destinationLedIndex,
            int ledCount,
            StripPriority priority)
        {
            GetStripRenderData(stripId, true).SetArgbData(
                data,
                sourceColorIndex,
                destinationLedIndex,
                ledCount,
                priority);
        }

        public bool RemovePriority(int stripId, StripPriority priority)
        {
            return GetStripRenderData(stripId).RemovePriority(priority);
        }

        public LedColorBuffer RenderedData(int stripId)
        {
            return GetStripRenderData(stripId).RenderedData;
        }

        public void Clear()
        {
            _stripData.Clear();
        }

        private void PriorityComparerChanged(object sender, EventArgs e)
        {
            foreach (var stripRendererData in _stripData)
            {
                stripRendererData.Value.ReOrderPriorities(_priorityComparer);
            }
        }

        private StripRendererData GetStripRenderData(int stripId, bool addIfNotExist = false)
        {
            if (_stripData.TryGetValue(stripId, out var data))
            {
                return data;
            }

            data = new StripRendererData(_priorityComparer);
            if (addIfNotExist)
            {
                _stripData.Add(stripId, data);
            }

            return data;
        }

        private class StripRendererData
        {
            private SortedList<StripPriority, LedColorBuffer> _colorBuffers;

            public StripRendererData(
                IComparer<StripPriority> comparer,
                int ledCount = EdgeLightConstants.MaxLedPerStrip)
            {
                LedCount = ledCount;
                _colorBuffers =
                    new SortedList<StripPriority, LedColorBuffer>(comparer);
            }

            public LedColorBuffer RenderedData => GetRenderedData();

            private int LedCount { get; }

            public void SetColor(Color color, StripPriority priority)
            {
                var colorBuffer = GetColorBuffer(priority);
                colorBuffer.SetColor(0, color, LedCount);
            }

            public void SetArgbData(
                LedColorBuffer sourceBuffer,
                int sourceColorIndex,
                int destinationLedIndex,
                int ledCount,
                StripPriority priority)
            {
                var colorBuffer = GetColorBuffer(priority);
                colorBuffer.SetColors(sourceBuffer, sourceColorIndex, ledCount, destinationLedIndex);
            }

            public bool RemovePriority(StripPriority priority)
            {
                return _colorBuffers.Remove(priority);
            }

            public void ReOrderPriorities(IComparer<StripPriority> priorityComparer)
            {
                _colorBuffers = new SortedList<StripPriority, LedColorBuffer>(_colorBuffers, priorityComparer);
            }

            private LedColorBuffer GetRenderedData()
            {
                var colorBuffer = new LedColorBuffer(LedCount);

                foreach (var ledColorBuffer in _colorBuffers)
                {
                    colorBuffer.Draw(ledColorBuffer.Value);
                }

                return colorBuffer;
            }

            private LedColorBuffer GetColorBuffer(StripPriority priority)
            {
                if (_colorBuffers.TryGetValue(priority, out var buffer))
                {
                    return buffer;
                }

                buffer = new LedColorBuffer(LedCount);
                _colorBuffers.Add(priority, buffer);

                return buffer;
            }
        }
    }
}