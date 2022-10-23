namespace Aristocrat.Monaco.G2S.Common.PackageManager.Storage
{
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Base interface for package error entity repository.
    /// </summary>
    public interface IPackageErrorRepository : IRepository<PackageError>
    {
        /// <summary>
        ///     Gets all package errors instances by specified package Id.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="packageId">Package Id.</param>
        /// <returns>Returns package instance or null.</returns>
        IEnumerable<PackageError> GetByPackageId(DbContext context, string packageId);

        /// <summary>
        ///     Deletes all package error entities by package id.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="packageId">The package identifier</param>
        void DeleteByPackageId(DbContext context, string packageId);
    }
}