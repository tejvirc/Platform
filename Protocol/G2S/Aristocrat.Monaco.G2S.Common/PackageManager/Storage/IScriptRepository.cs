namespace Aristocrat.Monaco.G2S.Common.PackageManager.Storage
{
    using Microsoft.EntityFrameworkCore;
    using Monaco.Common.Storage;

    /// <summary>
    ///     Provides a mechanism interact with the script repository.
    /// </summary>
    public interface IScriptRepository : IRepository<Script>
    {
        /// <summary>
        ///     Gets the maximum last sequence.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <returns>Max last sequence.</returns>
        long GetMaxLastSequence(DbContext context);

        /// <summary>
        ///     Gets script instance by specified package Id.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="scriptId">Script Id.</param>
        /// <returns>Returns script instance or null.</returns>
        Script GetScriptByScriptId(DbContext context, int scriptId);
    }
}