namespace Aristocrat.Monaco.G2S.Services.Progressive
{
    using System.Collections.Generic;

    public interface IProgressiveDeviceManager
    {
        /// <summary>
        ///     A reference to the G2S engine
        /// </summary>
        IEngine Engine { set; }

        /// <summary>
        ///     This dictionary stores the Vertex device Ids that are configured on the Vertex and Monaco UIs
        ///     The key is the monaco device Id
        ///     The value is the vertex device Id
        /// </summary>
        Dictionary<int, int> VertexDeviceIds { get; set; }

        /// <summary>
        ///     This list stores the configured Vertex progIDs and is used to create the progressive devices according to the configured values
        /// </summary>
        List<int> VertexProgressiveIds { get; set; }

        /// <summary>
        ///     The Progressive IDs for each respective device.
        ///     The key is the device ID.
        /// </summary>
        Dictionary<int, int> DeviceProgIdMap { get; set; }

        /// <summary>
        ///     Updates the progressive service based on configured settings.
        /// </summary>
        /// <param name="fromConfig">
        ///     True to load configuration from the user-set properties
        /// </param>
        /// <param name="fromBase">
        ///     True to load progressive devices from 
        /// </param>
        void OnConfiguredProgressives(bool fromConfig = false, bool fromBase = false);
    }
}
