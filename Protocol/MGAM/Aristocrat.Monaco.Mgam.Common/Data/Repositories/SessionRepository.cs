namespace Aristocrat.Monaco.Mgam.Common.Data.Repositories
{
    using System.Linq;
    using Models;
    using Protocol.Common.Storage.Repositories;

    /// <summary>
    ///     Session Repository
    /// </summary>
    public static class SessionRepository
    {
        /// <summary>
        ///     GetSessionId
        /// </summary>
        /// <param name="repository">Session repository.</param>
        /// <returns>SessionId</returns>
        public static int? GetSessionId(this IRepository<Session> repository)
        {
            return repository
                .Queryable().SingleOrDefault()?.SessionId;
        }
    }
}
