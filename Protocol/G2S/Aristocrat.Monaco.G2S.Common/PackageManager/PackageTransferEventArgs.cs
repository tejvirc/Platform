namespace Aristocrat.Monaco.G2S.Common.PackageManager
{
    using G2S.Data.Model;
    using System;

    /// <summary>
    ///     Arguments for any transfer package activity.
    /// </summary>
    public class PackageTransferEventArgs : EventArgs
    {
        /// <summary>
        ///     Gets or sets Package Id
        /// </summary>
        public string PackageId { get; set; }

        /// <summary>
        ///     Gets or sets transfer state.
        /// </summary>
        public PackageState? PackageState { get; set; }

        /// <summary>
        ///     Gets or sets transfer unique identifier.
        /// </summary>
        public int TransferId { get; set; }

        /// <summary>
        ///     Gets or sets transfer state.
        /// </summary>
        public TransferState TransferState { get; set; }

        /// <summary>
        ///     Gets or sets the package file path.
        /// </summary>
        public string PackageFilePath { get; set; }

        /// <summary>
        ///     Gets or sets the package file size.
        /// </summary>
        public long Size { get; set; }
    }
}