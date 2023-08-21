namespace Aristocrat.Monaco.Hardware.EdgeLight.SequenceLib
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Linq;
    using Contracts;
    using Hardware.Contracts.EdgeLighting;

    internal class BlinkColorRenderer : BaseRenderer<BlinkPatternParameters>, IEdgeLightRenderer
    {
        private int _currentStep;
        private List<Step> _steps = new List<Step>();

        public int Id => GetHashCode();

        public void Setup(IEdgeLightManager edgeLightManager)
        {
            EdgeLightManager = edgeLightManager;
            var strips = Strips.ToList();
            _steps = new List<Step>
            {
                new ColorStep(Parameters.OnColor, strips, Parameters.Priority, edgeLightManager),
                new WaitStep(Parameters.OnTime, edgeLightManager),
                new ColorStep(Parameters.OffColor, strips, Parameters.Priority, edgeLightManager),
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
            private readonly Stopwatch _stopwatch = new Stopwatch();
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
            private readonly Color _color;
            private readonly StripPriority _priority;
            private readonly List<StripData> _strips;

            public ColorStep(
                Color color,
                List<StripData> strips,
                StripPriority priority,
                IEdgeLightManager edgeLightManager)
                : base(edgeLightManager)
            {
                _color = color;
                _strips = strips;
                _priority = priority;
            }

            public override bool DoStep()
            {
                _strips.ForEach(x => EdgeLightManager.SetStripColor(x.StripId, _color, _priority));
                return true;
            }
        }
    }
}