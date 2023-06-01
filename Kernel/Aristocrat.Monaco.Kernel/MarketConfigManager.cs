namespace Aristocrat.Monaco.Kernel;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using MarketConfig;
using System.Text.Json;

/// <inheritdoc />
public sealed class MarketConfigManager: IMarketConfigManager
{
    private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

    private bool _serviceInitialized = false;

    private MarketConfigManifest _marketConfigManifest;

    private string _configurationLinkPath;

    private readonly Dictionary<string, string> _segmentIdToModelClassMap = new();

    /// <inheritdoc />
    public string Name => "ConfigurationManager";

    /// <inheritdoc />
    public ICollection<Type> ServiceTypes => new[] { typeof(IMarketConfigManager) };

    /// <summary>
    ///     Initializes a new instance of the <see cref="MarketConfigManager" /> class.
    /// </summary>
    public MarketConfigManager()
    {
        Logger.Debug("Adding the configuration manager");
    }

    /// <inheritdoc />
    public void Initialize()
    {

    }

    /// <inheritdoc />
    public void InitializeFromDirectory(string configurationLinkPath)
    {
        Logger.Debug("Loading market config manifest from: " + configurationLinkPath);

        _configurationLinkPath = configurationLinkPath;

        // Parse the manifest.json file
        var manifestJson = File.ReadAllText(Path.Combine(configurationLinkPath, "manifest.json"));
        _marketConfigManifest = JsonSerializer.Deserialize<MarketConfigManifest>(manifestJson);

        _serviceInitialized = true;

        Logger.Debug($"Loaded {_marketConfigManifest.Jurisdictions.Count} jurisdictions from manifest.json");

        // Scan the assembly for classes decorated with MarketConfigSegmentAttribute and build a map of segment ID to
        // class name
        Assembly.GetExecutingAssembly().GetExportedTypes()
            .Where(t => t.GetCustomAttribute(typeof(MarketConfigSegmentAttribute), false) != null)
            .ToList()
            .ForEach(t =>
            {
                var attribute = t.GetCustomAttribute(
                    typeof(MarketConfigSegmentAttribute),
                    false) as MarketConfigSegmentAttribute;
                _segmentIdToModelClassMap[t.FullName!] = attribute!.SegmentId;
            });
    }

    private MarketConfigManifestJurisdiction GetMarketJurisdictionByInstallationId(string jurisdictionInstallationId)
    {
        if (!_serviceInitialized) throw new MarketConfigException("Service not initialized");

        var jurisdiction = _marketConfigManifest.Jurisdictions.Find(j =>
            j.JurisdictionInstallationId == jurisdictionInstallationId);

        if (jurisdiction == null)
        {
            throw new MarketConfigException(
                $"Jurisdiction {jurisdictionInstallationId} not found in manifest.json");
        }

        return jurisdiction;
    }

    private string GetSegmentIdFromClassName(string className)
    {
        if (_segmentIdToModelClassMap.TryGetValue(className, out var segmentId))
        {
            return segmentId;
        }
        throw new MarketConfigException(
            $"Type {className} not found in segment map, it must be decorated with MarketConfigSegmentAttribute");
    }

    /// <inheritdoc />
    public T GetMarketConfiguration<T>(string jurisdictionInstallationId)
    {
        if (!_serviceInitialized) throw new MarketConfigException("Service not initialized");

        var jurisdiction = GetMarketJurisdictionByInstallationId(jurisdictionInstallationId);
        var segmentId = GetSegmentIdFromClassName(typeof(T).FullName);

        // Find the relative path to the json file for this segment and jurisdiction
        if (!jurisdiction.Segments.TryGetValue(segmentId, out var filename))
        {
            throw new MarketConfigException(
                $"Segment {segmentId} not found in jurisdiction {jurisdiction.MachineId}");
        }

        // Parse the json file and create the configuration model object
        var json = File.ReadAllText(Path.Combine(_configurationLinkPath, filename));
        var config = JsonSerializer.Deserialize<T>(json);

        return config;
    }
}