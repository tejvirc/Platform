namespace Aristocrat.Monaco.Hardware.EdgeLight.Strips
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Serialization;
    using Contracts;

    internal class StripCloningHandler
    {
        private const string LogicalStripsCreationRuleXml = @"StripCloneMappings.xml";

        private readonly IEdgeLightManager _edgeLightManager;

        private List<(Mapping From, int FromCount, Mapping To, int ToCount)> _mappingsTuples;

        public StripCloningHandler(IEdgeLightManager manager, string pathToXml = LogicalStripsCreationRuleXml)
        {
            _edgeLightManager = manager ?? throw new ArgumentNullException();
            LoadXmlDefinition(pathToXml);
        }

        public IEnumerable<(int Id, LedColorBuffer Data)> Clone(IEnumerable<(int Id, LedColorBuffer Data)> stripData)
        {
            var gameStripData = stripData.ToList();
            var data = new Dictionary<int, (int Id, LedColorBuffer Data)>();
            foreach (var (from,_,  to, toCount) in _mappingsTuples)
            {
                if (gameStripData.All(x => x.Id != (int)from.Id) ||
                    gameStripData.Any(x => x.Id == (int)to.Id)) continue;
                if (!data.TryGetValue((int)to.Id, out var toData))
                {
                    toData = data[(int)to.Id] = ((int)to.Id, new LedColorBuffer(toCount));
                }

                var (_, ledColorBuffer) = gameStripData.First(x => x.Id == (int)from.Id);
                toData.Data.DrawScaled(
                    ledColorBuffer,
                    from.LedStart,
                    from.Count == Mapping.AllStrips ? ledColorBuffer.Count - from.LedStart : from.Count,
                    to.LedStart,
                    to.Count == Mapping.AllStrips ? toData.Data.Count - to.LedStart : to.Count);
            }

            return gameStripData.Concat(data.Values);
        }

        private void LoadXmlDefinition(Stream stream)
        {
            var theXmlRootAttribute = Attribute.GetCustomAttributes(typeof(StripCloneMappings))
    .FirstOrDefault(x => x is XmlRootAttribute) as XmlRootAttribute;
            var xmlSerializer = new XmlSerializer(typeof(StripCloneMappings), theXmlRootAttribute ?? new XmlRootAttribute(nameof(StripCloneMappings)));

            var mappings = (StripCloneMappings)xmlSerializer.Deserialize(stream);
            var strips = _edgeLightManager.LogicalStrips;
            var validFrom = mappings.Mappings.Where(m => strips.Any(s => s.StripId == (int)m.From.Id));
                var validTo = validFrom
                .SelectMany(
                    m => m.To.Where(t => strips.Any(s => s.StripId == (int)t.Id))
                        .Select(
                            t => (From: m.From, FromCount: strips.First(x => x.StripId == (int)m.From.Id).LedCount,
                                    To: t, ToCount: strips.First(x => x.StripId == (int)t.Id).LedCount
                                )));
                _mappingsTuples = validTo.ToList();
        }

        private void LoadXmlDefinition(string fileOrResource)
        {
            Stream stream;
            if (File.Exists(fileOrResource))
            {
                stream = new FileStream(fileOrResource, FileMode.Open, FileAccess.Read);
            }
            else
            {
                var localAssembly = Assembly.GetExecutingAssembly();
                stream = localAssembly.GetManifestResourceStream(fileOrResource);
            }

            if (null == stream)
            {
                throw new Exception($" The Passed {fileOrResource} cannot be found");
            }

            using (stream)
            {
                LoadXmlDefinition(stream);
            }
        }
    }
}