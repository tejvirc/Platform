namespace Aristocrat.Monaco.Application.EdgeLight
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Handlers;
    using Kernel;

    public class EdgeLightHandlerRegistry : IEdgeLightHandlerRegistry, IService
    {
        private readonly Dictionary<string, IEdgeLightHandler> _handlerSourceDictionary =
            new Dictionary<string, IEdgeLightHandler>();

        public EdgeLightHandlerRegistry()
            : this(
                new IEdgeLightHandler[]
                {
                    new EdgeLightAuditMenuHandler(), new EdgeLightBrightnessHandler(),
                    new EdgeLightBottomStripHandler(), new EdgeLightAsTowerLightHandler()
                }
            )
        {
        }

        public EdgeLightHandlerRegistry(IEnumerable<IEdgeLightHandler> edgeLightHandlerSources)
        {
            foreach (var edgeLightHandlerSource in edgeLightHandlerSources.Where(x => x.Enabled))
            {
                RegisterHandler(edgeLightHandlerSource);
            }
        }

        public IEdgeLightHandler GetHandler(string name)
        {
            return !_handlerSourceDictionary.TryGetValue(name, out var handler) ? null : handler;
        }

        public void RegisterHandler(IEdgeLightHandler edgeLightHandlerSource)
        {
            _handlerSourceDictionary.Add(edgeLightHandlerSource.Name, edgeLightHandlerSource);
        }

        public string Name => nameof(EdgeLightHandlerRegistry);

        public ICollection<Type> ServiceTypes => new[] { typeof(EdgeLightHandlerRegistry) };

        public void Initialize()
        {
        }
    }
}