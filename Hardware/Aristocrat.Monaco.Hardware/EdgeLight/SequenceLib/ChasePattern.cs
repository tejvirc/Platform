namespace Aristocrat.Monaco.Hardware.EdgeLight.SequenceLib
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Contracts;
    using Hardware.Contracts.EdgeLighting;
    using Strips;

    internal sealed class ChaserPattern : BaseRenderer<ChaserPatternParameters>, IEdgeLightRenderer
    {
        private CurrentStripData _currentStripData;

        private List<StripData> _orderedStripIds;

        public int Id => GetHashCode();

        public void Setup(IEdgeLightManager edgeLightManager)
        {
            EdgeLightManager = edgeLightManager;
            _orderedStripIds = Strips.ToList();
            _currentStripData = null;
        }

        public void Update()
        {
            if (EdgeLightManager == null || !_orderedStripIds.Any())
            {
                return;
            }

            if (_currentStripData == null)
            {
                _orderedStripIds.ForEach(
                    x =>
                        EdgeLightManager.SetStripColor(
                            x.StripId,
                            Parameters.BackgroundColor,
                            Parameters.Priority));
                _currentStripData = new CurrentStripData(_orderedStripIds.GetEnumerator(), Parameters);
                _currentStripData.Next();
            }
            else if (_currentStripData.Stopwatch.ElapsedMilliseconds < Parameters.Delay)
            {
                return;
            }
            else
            {
                EdgeLightManager.SetStripColor(
                    _currentStripData.CurrentStrip.StripId,
                    Parameters.BackgroundColor,
                    Parameters.Priority);
                _currentStripData.Next();
            }

            EdgeLightManager.SetStripColors(
                _currentStripData.CurrentStrip.StripId,
                _currentStripData.LedColorBuffer,
                0,
                Parameters.Priority);

            _currentStripData.Stopwatch.Restart();
        }

        public void Clear()
        {
            if (EdgeLightManager == null)
            {
                return;
            }

            _orderedStripIds.ForEach(x => EdgeLightManager.ClearStripForPriority(x.StripId, Parameters.Priority));
            EdgeLightManager = null;
            _orderedStripIds.Clear();
        }

        private class CurrentStripData
        {
            private readonly IEnumerator<StripData> _currentStripEnumerator;
            private readonly ChaserPatternParameters _parameters;

            public CurrentStripData(IEnumerator<StripData> stripEnumerator, ChaserPatternParameters parameters)
            {
                _currentStripEnumerator = stripEnumerator;
                _parameters = parameters;
                LedIndex = 0;

                Stopwatch.Restart();
            }

            private int LedIndex { get; set; }
            public Stopwatch Stopwatch { get; } = new Stopwatch();
            public StripData CurrentStrip { get; private set; }
            public LedColorBuffer LedColorBuffer { get; private set; }

            public void Next()
            {
                if (CurrentStrip != null && LedIndex + 1 < CurrentStrip.LedCount)
                {
                    LedColorBuffer[LedIndex] = _parameters.BackgroundColor;
                    LedColorBuffer[++LedIndex] = _parameters.ForegroundColor;
                    return;
                }

                if (!_currentStripEnumerator.MoveNext())
                {
                    _currentStripEnumerator.Reset();
                    if (!_currentStripEnumerator.MoveNext())
                    {
                        return;
                    }
                }

                if (_currentStripEnumerator.Current == null)
                {
                    return;
                }

                CurrentStrip = _currentStripEnumerator.Current;
                LedIndex = 0;
                LedColorBuffer =
                    new LedColorBuffer(Enumerable.Repeat(_parameters.BackgroundColor, CurrentStrip.LedCount))
                    {
                        [LedIndex] = _parameters.ForegroundColor
                    };
            }
        }
    }
}