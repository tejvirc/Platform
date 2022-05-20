namespace Aristocrat.Monaco.Asp.Client.Contracts
{
    using System;

    /// <summary>
    ///     Indicates the status of the Meter Snapshot. Reference Class 2 Type 3 Parameter 2 in ASP5000 Protocol Document.
    /// </summary>
    [Flags]
    public enum MeterSnapshotStatus
    {
        Enabled = 0x00,
        Disabled = 0x01
    }

    /// <summary>
    ///     Provides a mechanism to interact with the MeterSnapshotProvider
    ///     A meter provider that implements this interface indicates
    ///     the ability to create a snapshot of meter values (under its management)
    ///     on request and also allows access to values from the snapshot, rather
    ///     than the meters themselves. A meter Provider also maintains and persists the snapshot.
    /// </summary>
    public interface IMeterSnapshotProvider
    {
        /// <summary>
        ///     Gets and Sets the status of the Meter Snapshot
        ///     This can be enabled or disabled by the system.
        ///     refer Class 2 Type 3 Parameter 2 ASP5000 protocol document.
        /// </summary>
        MeterSnapshotStatus SnapshotStatus { get; set; }

        /// <summary>
        ///     Creates and Persists the Snapshot
        /// </summary>
        ///  <param name="notifyOnCompletion">A flag to determine if an event is raised after the snapshot is completed</param>
        void CreatePersistentSnapshot(bool notifyOnCompletion);

        /// <summary>
        ///     Gets the SnapShot meter value
        /// </summary>
        /// <param name="name">The name of the meter for which to get the Snapshot value</param>
        long GetSnapshotMeter(string name);
    }
}