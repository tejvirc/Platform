namespace Aristocrat.Monaco.Gaming.Contracts.Meters
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Kernel;
    using Progressives;
    using Progressives.Linked;
    using Progressives.SharedSap;

    /// <summary>
    ///     Definition of the IProgressiveMeterManager interface.
    /// </summary>
    public interface IProgressiveMeterManager : IService
    {
        /// <summary>
        ///     Gets the number of total progressives.
        /// </summary>
        int ProgressiveCount { get; }

        /// <summary>
        ///     Gets the number of total number levels across all progressives.
        /// </summary>
        int LevelCount { get; }

        /// <summary>
        ///     Gets the number of total number of linked levels.
        /// </summary>
        int LinkedLevelCount { get; }

        /// <summary>
        ///     Gets the number of total number of shared levels.
        /// </summary>
        int SharedLevelCount { get; }

        /// <summary>
        ///     Registers a <see cref="ProgressiveAdded" /> event handler.
        /// </summary>
        event EventHandler<ProgressiveAddedEventArgs> ProgressiveAdded;

        /// <summary>
        ///     Registers a <see cref="LinkedProgressiveAdded" /> event handler.
        /// </summary>
        event EventHandler<LinkedProgressiveAddedEventArgs> LinkedProgressiveAdded;

        /// <summary>
        ///     Registers a <see cref="LPCompositeMetersCanUpdate" /> event handler.
        /// </summary>
        event EventHandler<LPCompositeMetersCanUpdateEventArgs> LPCompositeMetersCanUpdate;

        /// <summary>
        ///     update Linked Progressive Win Occurrence Composite meters
        /// </summary>
        void UpdateLPCompositeMeters();

        /// <summary>
        ///     Adds a new progressive meter to the progressive meter manager.
        /// </summary>
        /// <param name="progressives">A collection of progressives to add.</param>
        void AddProgressives(IEnumerable<IViewableProgressiveLevel> progressives);

        /// <summary>
        ///     Adds a new progressive meter to the progressive meter manager.
        /// </summary>
        /// <param name="progressives">A collection of progressives to add.</param>
        void AddProgressives(IEnumerable<IViewableSharedSapLevel> progressives);

        /// <summary>
        ///     Adds a new linked progressive meter to the progressive meter manager.
        /// </summary>
        /// <param name="progressives">A collection of progressives to add.</param>
        void AddLinkedProgressives(IEnumerable<IViewableLinkedProgressiveLevel> progressives);

        /// <summary>
        ///     Gets block the progressive devices and the associated block index
        /// </summary>
        /// <returns>A collection of device Ids with the associated block index</returns>
        IEnumerable<(int deviceId, int blockIndex)> GetProgressiveBlocks();

        /// <summary>
        ///     Gets progressive levels and their associated block index
        /// </summary>
        /// <returns>A collection of device and level Ids with the associated block index</returns>
        IEnumerable<(int deviceId, int levelId, int blockIndex)> GetProgressiveLevelBlocks();

        /// <summary>
        ///     Gets progressive levels and their associated block index
        /// </summary>
        /// <returns>A collection of level Ids with the associated block index</returns>
        IEnumerable<(string linkedLevelName, int blockIndex)> GetLinkedLevelBlocks();

        /// <summary>
        ///     Gets progressive levels and their associated block index
        /// </summary>
        /// <returns>A collection of level Ids with the associated block index</returns>
        IEnumerable<(Guid sharedLevelId, int blockIndex)> GetSharedLevelBlocks();

        /// <summary>
        ///     Gets the meter for the given progressive.
        /// </summary>
        /// <param name="deviceId">The progressive device identifier.</param>
        /// <param name="meterName">The name of the meter. </param>
        /// <returns>An IMeter object representing that meter. </returns>
        IMeter GetMeter(int deviceId, string meterName);

        /// <summary>
        ///     Gets the meter for the given progressive.
        /// </summary>
        /// <param name="deviceId">The progressive device identifier. </param>
        /// <param name="levelId">The level identifier. </param>
        /// <param name="meterName">The name of the meter. </param>
        /// <returns>An IMeter object representing that meter. </returns>
        IMeter GetMeter(int deviceId, int levelId, string meterName);

        /// <summary>
        ///     Gets the meter for the share level.
        /// </summary>
        /// <param name="linkedLevelName">The linked level key.</param>
        /// <param name="meterName">The name of the meter. </param>
        /// <returns>An IMeter object representing that meter. </returns>
        IMeter GetMeter(string linkedLevelName, string meterName);

        /// <summary>
        ///     Gets the meter for the share level.
        /// </summary>
        /// <param name="sharedLevelId">The shared level Id.</param>
        /// <param name="meterName">The name of the meter. </param>
        /// <returns>An IMeter object representing that meter. </returns>
        IMeter GetMeter(Guid sharedLevelId, string meterName);

        /// <summary>
        ///     Gets the meter.
        /// </summary>
        /// <param name="meterName">The name of the meter. </param>
        /// <returns>An IMeter object representing that meter. </returns>
        IMeter GetMeter(string meterName);

        /// <summary>
        ///     Gets the named meter for the given progressive.
        /// </summary>
        /// <param name="deviceId">The progressive device identifier.</param>
        /// <param name="meterName">The name of the meter. </param>
        /// <returns>An IMeter object representing that meter. </returns>
        string GetMeterName(int deviceId, string meterName);

        /// <summary>
        ///     Gets the formatted meter name.
        /// </summary>
        /// <param name="deviceId">The progressive device identifier. </param>
        /// <param name="levelId">The level identifier. </param>
        /// <param name="meterName">The name of the meter. </param>
        /// <returns>A formatted meter name. </returns>
        string GetMeterName(int deviceId, int levelId, string meterName);

        /// <summary>
        ///     Gets the named meter for the share level.
        /// </summary>
        /// <param name="linkedLevelName">The linked level key.</param>
        /// <param name="meterName">The name of the meter. </param>
        /// <returns>An IMeter object representing that meter. </returns>
        string GetMeterName(string linkedLevelName, string meterName);

        /// <summary>
        ///     Gets the named meter for the share level.
        /// </summary>
        /// <param name="sharedLevelId">The shared level Id.</param>
        /// <param name="meterName">The name of the meter. </param>
        /// <returns>An IMeter object representing that meter. </returns>
        string GetMeterName(Guid sharedLevelId, string meterName);

        /// <summary>
        ///     Returns whether or not a meter with the given name is provided.
        /// </summary>
        /// <param name="deviceId">The progressive device identifier.</param>
        /// <param name="meterName">The name of the meter. </param>
        /// <returns>An IMeter object representing that meter. </returns>
        bool IsMeterProvided(int deviceId, string meterName);

        /// <summary>
        ///     Returns whether or not a meter with the given name is provided.
        /// </summary>
        /// <param name="meterName">The name of the meter. </param>
        /// <returns>An IMeter object representing that meter. </returns>
        bool IsMeterProvided(string meterName);

        /// <summary>
        ///     Returns whether or not a meter with the given name is provided.
        /// </summary>
        /// <param name="deviceId">The progressive device identifier. </param>
        /// <param name="levelId">The level identifier. </param>
        /// <param name="meterName">The name of the meter. </param>
        /// <returns>An IMeter object representing that meter. </returns>
        bool IsMeterProvided(int deviceId, int levelId, string meterName);

        /// <summary>
        ///     Returns whether or not a meter with the given name is provided.
        /// </summary>
        /// <param name="linkedLevelName">The linked level key. </param>
        /// <param name="meterName">The name of the meter. </param>
        /// <returns>An IMeter object representing that meter. </returns>
        bool IsMeterProvided(string linkedLevelName, string meterName);

        /// <summary>
        ///     Returns whether or not a meter with the given name is provided.
        /// </summary>
        /// <param name="sharedLevelId">The shared level Id.</param>
        /// <param name="meterName">The name of the meter. </param>
        /// <returns>An IMeter object representing that meter. </returns>
        bool IsMeterProvided(Guid sharedLevelId, string meterName);
    }
}