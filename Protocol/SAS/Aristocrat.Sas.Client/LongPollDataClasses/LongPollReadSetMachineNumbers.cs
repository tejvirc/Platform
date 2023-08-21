namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    using System;

    /// <summary>
    ///     The control flags for the Set Machine Numbers Command
    /// </summary>
    [Flags]
    public enum MachineNumbersControlFlags
    {
        /// <summary>
        ///     The control flag for none fo the options are configurable
        /// </summary>
        None = 0,

        /// <summary>
        ///     The control flag for the asset number being configurable
        /// </summary>
        AssetNumberConfigurable = 0x01,

        /// <summary>
        ///     The control flag for the floor location being configurable
        /// </summary>
        FloorLocationConfigurable = 0x02
    }

    public class LongPollSetMachineNumbersData : LongPollData
    {
        /// <summary>
        ///     Creates an instance of the LongPollSetMachineNumbersData class
        /// </summary>
        /// <param name="assetNumber">Asset Number from server. if 0 only query for asset number</param>
        /// <param name="floorLocation">Floor location from server</param>
        public LongPollSetMachineNumbersData(uint assetNumber, string floorLocation)
        {
            AssetNumber = assetNumber;
            FloorLocation = floorLocation;
        }

        /// <summary>
        ///     Creates an instance of the LongPollSetMachineNumbersData class
        /// </summary>
        public LongPollSetMachineNumbersData()
        {
            FloorLocation = string.Empty;
            AssetNumber = 0;
        }

        /// <summary>
        ///     Asset number from SAS to set asset number or, if 0, queries the asset number
        /// </summary>
        public uint AssetNumber { get; set; }

        /// <summary>
        ///     Floor location from SAS to set floor location, can be string.empty
        /// </summary>
        public string FloorLocation { get; set; }
    }

    /// <summary>
    ///     Data class to hold the response of Set Machine Number result
    /// </summary>
    public class LongPollSetMachineNumbersResponse : LongPollResponse
    {
        /// <summary>
        ///     Creates an instance of the LongPollSetMachineNumbersResponse class
        /// </summary>
        /// <param name="controlFlags">The control flags for what the EGM allows the host to control</param>
        /// <param name="assetNumber">asset number from system</param>
        /// <param name="floorLocation">floor location from system</param>
        public LongPollSetMachineNumbersResponse(
            MachineNumbersControlFlags controlFlags,
            uint assetNumber,
            string floorLocation)
        {
            ControlFlags = controlFlags;
            AssetNumber = assetNumber;
            FloorLocation = floorLocation;
        }

        public MachineNumbersControlFlags ControlFlags { get; }

        /// <summary>
        ///     Asset Number returned from the handler 
        /// </summary>
        public uint AssetNumber { get; }

        /// <summary>
        ///     Floor Location returned from the handler 
        /// </summary>
        public string FloorLocation { get; }
    }
}