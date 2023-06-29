namespace Aristocrat.Monaco.Hardware.EdgeLight.SequenceLib
{
    using Contracts;
    using Hardware.Contracts.EdgeLighting;
    using RendererFactoryType = System.Collections.Generic.Dictionary<
        System.Type, System.Func<Hardware.Contracts.EdgeLighting.PatternParameters, Contracts.IEdgeLightRenderer>>;

    internal class RendererFactory : IEdgeLightRendererFactory
    {
        private static readonly RendererFactoryType RendererFactories = new()
        {
            {
                typeof(SolidColorPatternParameters), y =>
                    new SolidColorRenderer { Parameters = y as SolidColorPatternParameters }
            },
            {
                typeof(ChaserPatternParameters), y =>
                    new ChaserPattern { Parameters = y as ChaserPatternParameters }
            },
            {
                typeof(RainbowPatternParameters), y =>
                    new RainbowPattern { Parameters = y as RainbowPatternParameters }
            },
            {
                typeof(BlinkPatternParameters), y =>
                    new BlinkColorRenderer { Parameters = y as BlinkPatternParameters }
            },
            {
                typeof(IndividualLedPatternParameters), y =>
                    new IndividualLedRenderer { Parameters = y as IndividualLedPatternParameters }
            },
            {
                typeof(IndividualLedBlinkPatternParameters), y =>
                    new IndividualLedBlinkColorRenderer { Parameters = y as IndividualLedBlinkPatternParameters }
            }
        };

        public IEdgeLightRenderer CreateRenderer<TParametersType>(TParametersType parameters)
        {
            return !RendererFactories.TryGetValue(parameters.GetType(), out var factory)
                ? null
                : factory(parameters as PatternParameters);
        }
    }
}