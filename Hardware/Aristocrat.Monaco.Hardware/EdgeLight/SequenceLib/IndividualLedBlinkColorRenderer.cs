namespace Aristocrat.Monaco.Hardware.EdgeLight.SequenceLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Linq;
    using Contracts;
    using Hardware.Contracts.EdgeLighting;
    using Strips;

    internal class IndividualLedBlinkColorRenderer : BaseRenderer<IndividualLedBlinkPatternParameters>, IEdgeLightRenderer
    {
        private int _currentStep;
        private List<Step> _steps = new();

        public int Id => GetHashCode();

        public void Setup(IEdgeLightManager edgeLightManager)
        {
            EdgeLightManager = edgeLightManager;
            var strips = Strips.ToList();

            _steps = new List<Step>
            {
                new ColorStep(Parameters.StripOnUpdateFunction, strips, Parameters.Priority, edgeLightManager),
                new WaitStep(Parameters.OnTime, edgeLightManager),
                new ColorStep(Parameters.StripOffUpdateFunction, strips, Parameters.Priority, edgeLightManager),
                new WaitStep(Parameters.OffTime, edgeLightManager)
            };

            _currentStep = 0;
        }

        public void Update()
        {
            if (EdgeLightManager == null)
            {
                return;
            }

            var step = _steps[_currentStep];
            if (step.DoStep())
            {
                _currentStep = (_currentStep + 1) % _steps.Count;
            }
        }

        public void Clear()
        {
            if (EdgeLightManager == null)
            {
                return;
            }

            Strips.ToList().ForEach(x => EdgeLightManager.ClearStripForPriority(x.StripId, Parameters.Priority));
            _steps.Clear();
        }

        private abstract class Step
        {
            protected Step(IEdgeLightManager edgeLightManager)
            {
                EdgeLightManager = edgeLightManager;
            }

            protected IEdgeLightManager EdgeLightManager { get; }

            public abstract bool DoStep();
        }

        private class WaitStep : Step
        {
            private readonly Stopwatch _stopwatch = new();
            private readonly int _time;
            private bool _reset = true;

            public WaitStep(int time, IEdgeLightManager edgeLightManager)
                : base(edgeLightManager)
            {
                _time = time;
            }

            public override bool DoStep()
            {
                if (_reset)
                {
                    _stopwatch.Restart();
                }

                return _reset = _stopwatch.ElapsedMilliseconds >= _time;
            }
        }

        private class ColorStep : Step
        {
            private readonly Func<int, int, Color[]> _colors;
            private readonly StripPriority _priority;
            private readonly List<StripData> _strips;

            public ColorStep(
                Func<int, int, Color[]> colors,
                List<StripData> strips,
                StripPriority priority,
                IEdgeLightManager edgeLightManager)
                : base(edgeLightManager)
            {
                _colors = colors;
                _strips = strips;
                _priority = priority;
            }

            public override bool DoStep()
            {
                _strips.ForEach(
                    x =>
                    {
                        var colors = _colors?.Invoke(x.StripId, x.LedCount);
                        if (colors == null)
                        {
                            EdgeLightManager.ClearStripForPriority(x.StripId, _priority);
                        }
                        else
                        {
                            EdgeLightManager.SetStripColors(x.StripId, new LedColorBuffer(colors), 0, _priority);
                        }
                    }
                );

                return true;
            }
        }
    }
}