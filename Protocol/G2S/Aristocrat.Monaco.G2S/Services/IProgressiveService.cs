namespace Aristocrat.Monaco.G2S.Services
{
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives.Linked;
    using System;
    using System.Collections.Generic;

    public interface IProgressiveService
    {
        /// <summary>
        ///     A reference to the G2S engine
        /// </summary>
        public IEngine engine { set; }

        /// <summary>
        ///     Last update SetProgressive Value Received Time
        /// </summary>
        public DateTime LastProgressiveUpdateTime { get; set; }

        /// <summary>
        ///     Used to convert level ids between internal Monaco ids and external Vertex ids
        /// </summary>
        public ProgressiveLevelIdManager LevelIds { get; }

        /// <summary>
        ///     This dictionary stores the Vertex device Ids that are configured on the Vertex and Monaco UIs
        ///     The key is the monaco device Id
        ///     The value is the vertex device Id
        /// </summary>
        public Dictionary<int, int> VertexDeviceIds { get; set; }

        /// <summary>
        ///     This list stores the configured Vertex progIDs and is used to create the progressive devices according to the configured values
        /// </summary>
        /// <inheritdoc />
        public List<int> VertexProgressiveIds { get; set; }

        /// <summary>
        ///     The Progressive IDs for each respective device.
        ///     The key is the device ID.
        /// </summary>
        public Dictionary<int, int> DevicesProgIds { get; set; }

        /// <summary>
        /// The array of the last ProgressiveValues assigned in the SetProgressiveValue command.
        /// The key is stored as "ProgressiveID|LevelID"
        /// </summary>
        public Dictionary<string, ProgressiveValue> ProgressiveValues { get; set; }

        /// <summary>
        ///     Gets the specified simple meters for the specified progressive device
        /// </summary>
        /// <param name="deviceId">
        ///     The identifier of the progressive device
        /// </param>
        /// <param name="includedMeters">
        ///     The array of meter names
        /// </param>
        public IEnumerable<simpleMeter> GetProgressiveLevelMeters(int deviceId, params string[] includedMeters);

        /// <summary>
        ///     This method is called whenever the ProgressiveHostOfflineTimer should be reset.
        ///     Currently this will happen any time that SetProgressiveValue is called, though it may be moved if a more suitable location is found.
        ///     If there is no progressive host with the offline check enabled then this returns out.
        /// </summary>
        public void ReceiveProgressiveValueUpdate();

        /// <summary>
        ///     Set the state of a progressive device
        /// </summary>
        /// <param name="state">
        ///     The online or offline state
        /// </param>
        /// <param name="device">
        ///     The progressive device
        /// </param>
        /// <param name="hostReason">
        ///     The string reason sent by the host
        /// </param>
        public void SetProgressiveDeviceState(bool state, IProgressiveDevice device, string hostReason = null);

        /// <summary>
        /// Updates the specified LinkedProgressiveLevel to use the new valueInCents
        /// </summary>
        /// <param name="progId">The Id for the progressive that will be updated.</param>
        /// <param name="levelId">The Id for the level that will be updated.</param>
        /// <param name="valueInCents">The new value in cents for the progressive level.</param>
        /// <returns></returns>
        public LinkedProgressiveLevel UpdateLinkedProgressiveLevels(
        int progId,
        int levelId,
        long valueInCents,
        bool initialize = false);

        /// <summary>
        ///     Updates the progressive service based on configured settings.
        /// </summary>
        /// <param name="fromConfig">
        ///     True to load configuration from the user-set properties
        /// </param>
        /// <param name="fromBase">
        ///     True to load progressive devices from 
        /// </param>
        public void UpdateVertexProgressives(bool fromConfig = false, bool fromBase = false);
    }
}
