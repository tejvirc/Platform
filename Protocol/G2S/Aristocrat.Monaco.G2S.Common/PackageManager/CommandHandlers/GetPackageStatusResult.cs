namespace Aristocrat.Monaco.G2S.Common.PackageManager.CommandHandlers
{
    using G2S.Data.Model;
    using Storage;

    /// <summary>
    ///     Get package status result
    /// </summary>
    public class GetPackageStatusResult
    {
        /// <summary>
        ///     Gets or sets gets package state.
        /// </summary>
        public PackageState PackageState { get; set; }

        /// <summary>
        ///     Gets or sets gets current transfer entity in case it is in 'In Progress' state.
        /// </summary>
        public TransferEntity Transfer { get; set; }

        /// <summary>
        ///     Gets or sets gets package error entity if there is one.
        /// </summary>
        public PackageError PackageError { get; set; }
    }
}