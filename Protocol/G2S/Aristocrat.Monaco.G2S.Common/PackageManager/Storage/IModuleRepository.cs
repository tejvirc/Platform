namespace Aristocrat.Monaco.G2S.Common.PackageManager.Storage
{
    using Microsoft.EntityFrameworkCore;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Base interface for module entity repository.
    /// </summary>
    public interface IModuleRepository : IRepository<Module>
    {
        /// <summary>
        ///     Gets module instance by specified module Id.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="moduleId">Module Id.</param>
        /// <returns>Returns module instance or null.</returns>
        Module GetModuleByModuleId(DbContext context, string moduleId);
    }
}