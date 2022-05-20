namespace Aristocrat.Monaco.G2S.Common.PackageManager.Storage
{
    using System.Data.Entity;
    using System.Linq;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Repository implementation for script entity.
    /// </summary>
    public class ScriptRepository : BaseRepository<Script>, IScriptRepository
    {
        /// <inheritdoc />
        public long GetMaxLastSequence(DbContext context)
        {
            return context.Set<Script>().AsQueryable().DefaultIfEmpty().Max(x => x.Id);
        }

        /// <inheritdoc />
        public Script GetScriptByScriptId(DbContext context, int scriptId)
        {
            return context.Set<Script>()
                .Include(x => x.AuthorizeItems)
                .FirstOrDefault(x => x.ScriptId == scriptId);
        }
    }
}