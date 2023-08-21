namespace Aristocrat.Monaco.G2S.Common.PackageManager.CommandHandlers
{
    using System;
    using Data.Model;

    /// <summary>
    ///     Delete package arguments
    /// </summary>
    public class DeletePackageArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DeletePackageArgs" /> class.
        /// </summary>
        /// <param name="packageId">The packageId.</param>
        /// <param name="deletePackageCallback">Delete package callback</param>
        public DeletePackageArgs(string packageId, Action<DeletePackageArgs> deletePackageCallback)
        {
            if (string.IsNullOrEmpty(packageId))
            {
                throw new ArgumentNullException(nameof(packageId));
            }

            PackageId = packageId;
            DeletePackageCallback =
                deletePackageCallback ?? throw new ArgumentNullException(nameof(deletePackageCallback));
        }

        /// <summary>
        ///     Gets the package entity.
        /// </summary>
        /// <value>
        ///     The package entity.
        /// </value>
        public string PackageId { get; }

        /// <summary>
        ///     Gets delete package callback
        /// </summary>
        public Action<DeletePackageArgs> DeletePackageCallback { get; }

        /// <summary>
        ///     Gets or sets the package result.
        /// </summary>
        public PackageLog PackageResult { get; set; }
    }
}