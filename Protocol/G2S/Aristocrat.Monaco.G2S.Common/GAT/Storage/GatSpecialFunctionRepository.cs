namespace Aristocrat.Monaco.G2S.Common.GAT.Storage
{
    using System.Data.Entity;
    using System.Linq;
    using Monaco.Common.Storage;

    /// <summary>
    ///     GAT special function repository
    /// </summary>
    public class GatSpecialFunctionRepository : BaseRepository<GatSpecialFunction>, IGatSpecialFunctionRepository
    {
        /// <summary>
        ///     Gets all entities of repository from database.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <returns>
        ///     Returns all entities of repository from database.
        /// </returns>
        public override IQueryable<GatSpecialFunction> GetAll(DbContext context)
        {
            return base.GetAll(context).Include(x => x.Parameters);
        }
    }
}