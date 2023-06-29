namespace Aristocrat.Monaco.Hardware.EdgeLight.Strips
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Serialization;
    using Contracts;
    using log4net;

    public class LogicalStripFactory : ILogicalStripFactory
    {
        public static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private EdgeLightMappings _mappings;

        public LogicalStripFactory(ILogicalStripInformation logicalStripInfo)
        {
            if (logicalStripInfo == null)
            {
                throw new ArgumentNullException(nameof(logicalStripInfo));
            }

            LoadXmlDefinition(logicalStripInfo.LogicalStripCreationRuleXmlPath);
        }

        public IList<IStrip> GetLogicalStrips(IReadOnlyCollection<IStrip> physicalStrips)
        {
            var physicalIds = physicalStrips.Select(x => (x.StripId, x.LedCount));
            var mapping = _mappings.Mappings.Where(
                x => !x.HardwareStrips.Select(y => (y.Id, y.LedCount)).Except(physicalIds).Any()).ToList();
            if (!mapping.Any())
            {
                return CreateDefaultLogicalStrips(physicalStrips.Where(x => x.LedCount > 0)).Cast<IStrip>().ToList();
            }

            var unmatchedPhysicalStrips = physicalStrips.Where(
                y => mapping.SelectMany(item => item.HardwareStrips)
                    .All(x => x.Id != y.StripId || x.LedCount != y.LedCount));
            var logicalStrips = mapping.SelectMany(item => item.LogicalStrips).Concat(
                    CreateDefaultLogicalStrips(unmatchedPhysicalStrips.Where(x => x.LedCount > 0)))
                .ToList();
            foreach (var ledSegment in logicalStrips.SelectMany(
                         mappingLogicalStrip => mappingLogicalStrip.LedSegments))
            {
                ledSegment.PhysicalStrip =
                    physicalStrips.FirstOrDefault(x => x.LedCount > 0 && x.StripId == ledSegment.HardwareStripId);
            }

            return logicalStrips.Cast<IStrip>().ToList();
        }

        private static IEnumerable<LogicalStrip> CreateDefaultLogicalStrips(IEnumerable<IStrip> physicalStrips)
        {
            return physicalStrips.Select(
                x => new LogicalStrip
                {
                    HexStringId = (x.StripId & 0xff).ToString("x"),
                    LedSegments = new List<LedSegment>
                    {
                        new()
                        {
                            From = 0,
                            To = x.LedCount - 1,
                            HardwareStripId = x.StripId,
                            PhysicalStrip = x,
                            Id = 0
                        }
                    }
                });
        }

        private void LoadXmlDefinition(Stream stream)
        {
            var xmlSerializer = new XmlSerializer(typeof(EdgeLightMappings));

            _mappings = (EdgeLightMappings)xmlSerializer.Deserialize(stream);
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