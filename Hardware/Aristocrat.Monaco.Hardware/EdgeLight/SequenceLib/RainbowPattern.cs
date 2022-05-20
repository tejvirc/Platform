namespace Aristocrat.Monaco.Hardware.EdgeLight.SequenceLib
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Linq;
    using System.Numerics;
    using Contracts;
    using Hardware.Contracts.EdgeLighting;
    using Strips;

    internal sealed class RainbowPattern : BaseRenderer<RainbowPatternParameters>, IEdgeLightRenderer
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private Color[] _rainbowTexture;
        private List<StripColors> _stripColors = new List<StripColors>();
        private double _timeElapsed;

        public int Id => GetHashCode();

        public void Clear()
        {
            _stripColors.ForEach(x => EdgeLightManager.ClearStripForPriority(x.Strip.StripId, Parameters.Priority));
            _stripColors.Clear();
            EdgeLightManager = null;
        }

        public void Setup(IEdgeLightManager edgeLightManager)
        {
            EdgeLightManager = edgeLightManager;
            _stripColors = Strips.OrderBy(x => x.StripId)
                .Where(x => x.LedCount > 1)
                .Select(x => new StripColors { Strip = x, Colors = new LedColorBuffer(x.LedCount) }).ToList();
            _rainbowTexture = BuildRainbowColors(360);
            _timeElapsed = 0;
            _stopwatch.Start();
        }

        public void Update()
        {
            if (EdgeLightManager == null || _stripColors.Count == 0 ||
                _stopwatch.ElapsedMilliseconds < Parameters.Delay)
            {
                return;
            }

            UpdateRainbowColors(_timeElapsed);
            _timeElapsed += _stopwatch.ElapsedMilliseconds / 1000.0;
            _stopwatch.Restart();
        }

        private void UpdateRainbowColors(double timeElapsed)
        {
            const double speed = 2.0f;
            var totalLength = _stripColors.Sum(x => x.Strip.LedCount);
            var currentIndex = 0;
            _stripColors.ForEach(AssignColors);

            void AssignColors(StripColors stripColors)
            {
                for (var i = 0; i < stripColors.Colors.Count; ++i)
                {
                    // Normalize to [0, 1].
                    var point = currentIndex++ / (double)(totalLength - 1);

                    stripColors.Colors[i] = _rainbowTexture[GetSample(point)];
                }

                EdgeLightManager.SetStripColors(
                    stripColors.Strip.StripId,
                    stripColors.Colors,
                    0,
                    Parameters.Priority);
            }

            // Point sample texture
            int GetSample(double point)
            {
                // Scale
                point *= 4.0;

                // Animate texture coordinate.
                point += speed * timeElapsed;

                // Wrap mode.
                if (point >= 1.0f)
                {
                    point -= Math.Truncate(point);
                }

                return (int)(point * (_rainbowTexture.Length - 1) + 0.5f);
            }
        }

        private static Color[] BuildRainbowColors(int sampleCount)
        {
            // See https://en.wikipedia.org/wiki/HSL_and_HSV#Converting_to_RGB, but we have
            // the special case that: V = S = 1.0
            const float value = 1.0f;
            const float saturation = 1.0f;
            const float chroma = value * saturation;

            // In HSV color model, hue is in [0, 360] degrees.  Note that we do not sample
            // right end point since H = 0 equals H = 360.
            var dHue = 360.0f / sampleCount;

            var curve = new Color[sampleCount];
            for (var i = 0; i < sampleCount; ++i)
            {
                var hue = i * dHue;

                var hp = hue / 60.0f;

                var x = 1.0f - Math.Abs(hp % 2.0f - 1.0f);

                if (hp >= 0.0f && hp <= 1.0f)
                {
                    curve[i] = Vec4ToColor(new Vector4(chroma, x, 0.0f, 1.0f));
                }
                else if (hp >= 1.0f && hp <= 2.0f)
                {
                    curve[i] = Vec4ToColor(new Vector4(x, chroma, 0.0f, 1.0f));
                }
                else if (hp >= 2.0f && hp <= 3.0f)
                {
                    curve[i] = Vec4ToColor(new Vector4(0.0f, chroma, x, 1.0f));
                }
                else if (hp >= 3.0f && hp <= 4.0f)
                {
                    curve[i] = Vec4ToColor(new Vector4(0.0f, x, chroma, 1.0f));
                }
                else if (hp >= 4.0f && hp <= 5.0f)
                {
                    curve[i] = Vec4ToColor(new Vector4(x, 0.0f, chroma, 1.0f));
                }
                else if (hp >= 5.0f && hp < 6.0f)
                {
                    curve[i] = Vec4ToColor(new Vector4(chroma, 0.0f, x, 1.0f));
                }
            }

            return curve;
        }

        private static Color Vec4ToColor(Vector4 v)
        {
            return Color.FromArgb(
                (int)(v.W * 255.0f),
                (int)(v.X * 255.0f),
                (int)(v.Y * 255.0f),
                (int)(v.Z * 255.0f));
        }

        private class StripColors
        {
            public StripData Strip { get; set; }
            public LedColorBuffer Colors { get; set; }
        }
    }
}