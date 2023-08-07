namespace Aristocrat.Monaco.Kernel.MarketConfig
{
    using Kernel;

    /// <summary>
    ///     An interface through which a market configuration file can be accessed using the chosen jurisdiction
    ///     identifier and the segment's configuration object
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The Market Config Manager is the point of integration between the Monaco system and the market
    ///         configuration files generated through the Monaco Configuration Tool.
    ///     </para>
    ///     <para>
    ///         The Monaco Config Tool allows architects to identify configuration segments and their configuration
    ///         fields. It then allows users to set the values for each market jurisdiction using an inheritance model.
    ///         The system will then export the segment definitions as POCO model classes that this class can populate.
    ///     </para>
    ///     <para>
    ///         The segment classes are populated using the values within the configuration tool, which are also
    ///         exported. The data exports consist of a top level manifest file (manifest.json) which identifies
    ///         the available market jurisdictions and the files that contain the configuration data for each segment.
    ///     </para>
    ///     <para>
    ///         The segments are mapped to the model classes via the <see cref="MarketConfigSegmentAttribute"/>
    ///         class attribute The attribute is applied to the model class and identifies the segment name that is
    ///         listed in the manifest.json file.
    ///     </para>
    ///     <para>
    ///         When this service is initialized via the <see cref="InitializeFromDirectory"/> method, the manifest
    ///         file is read and stored within the manager. The <see cref="GetMarketConfiguration{T}"/> method
    ///         is then used to retrieve the configuration data for a given jurisdiction and segment.
    ///     </para>
    ///     <para>
    ///         The export files can be provided via an ISO within the packages deployment folder starting with
    ///         ATI_MarketConfigs. Additionally they can be provided during development by placing them in the
    ///         MarketConfig project within the DeveloperConfigs folder.
    ///     </para>
    /// </remarks>
    public interface IMarketConfigManager : IService
    {
        /// <summary>
        ///     Initialize the manager from the given directory path. This will first parse the manifest.json file and
        ///     retain it locally. It will then scan the assembly for the <see cref="MarketConfigSegmentAttribute"/>
        ///     class attribute and build a map of segment names to model classes.
        /// </summary>
        /// <param name="configurationLinkPath">
        ///     The path that contains the manifest.json file from the configuration tool to parse. The market
        ///     jurisdiction files that are referenced in the manifest.json file must also be present relative to this
        ///     path.
        /// </param>
        public void InitializeFromDirectory(string configurationLinkPath);

        /// <summary>
        ///     Retrieve the configuration data for the given jurisdiction and segment. The segment is identified by the
        ///     templated class type. The template class must have the <see cref="MarketConfigSegmentAttribute"/> class attribute
        ///     applied to it and the segment name must match the segment name in the manifest.json file.
        /// </summary>
        /// <param name="jurisdictionInstallationId">
        ///     The jurisdiction identifier of the configuration to deserialize.
        /// </param>
        /// <typeparam name="T">
        ///     The model object class to return for the segment data to be retrieved.
        /// </typeparam>
        /// <returns>
        ///     The parsed configuration data for the segment model and the identified market jurisdiction.
        /// </returns>
        /// <exception cref="MarketConfigException">
        ///     This exception will be thrown if there is any issue with parsing the configuration.
        /// </exception>
        public T GetMarketConfiguration<T>(string jurisdictionInstallationId);

        /// <summary>
        ///     Retrieve the configuration data for the currently selected jurisdiction and a segment. The segment is identified by the
        ///     templated class type. The template class must have the <see cref="MarketConfigSegmentAttribute"/> class attribute
        ///     applied to it and the segment name must match the segment name in the manifest.json file.
        /// </summary>
        /// <exception cref="MarketConfigException">
        ///     This exception will be thrown if there is any issue with parsing the configuration.
        /// </exception>
        /// <typeparam name="T">
        ///     The model object class to return for the segment data to be retrieved.
        /// </typeparam>
        /// <returns></returns>
        public T GetMarketConfigForSelectedJurisdiction<T>();
    }
}