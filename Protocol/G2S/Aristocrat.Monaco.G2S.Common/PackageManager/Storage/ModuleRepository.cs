namespace Aristocrat.Monaco.G2S.Common.PackageManager.Storage
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Repository implementation for module entity.
    /// </summary>
    public class ModuleRepository : BaseRepository<Module>, IModuleRepository
    {
        /// <inheritdoc />
        public Module GetModuleByModuleId(DbContext context, string moduleId)
        {
            return context.Set<Module>()
                .FirstOrDefault(
                    x => string.Compare(x.ModuleId, moduleId, StringComparison.InvariantCultureIgnoreCase) == 0);
        }
    }
}