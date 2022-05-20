namespace Aristocrat.Monaco.Common.Storage
{
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Text.RegularExpressions;

    /// <summary>
    ///     A set of database extensions
    /// </summary>
    public static class DatabaseExtensions
    {
        /// <summary>
        ///     Gets the name of the table.
        /// </summary>
        /// <typeparam name="T">the entity type</typeparam>
        /// <param name="context">The context.</param>
        /// <returns>the table name</returns>
        public static string GetTableName<T>(this IObjectContextAdapter context)
            where T : class
        {
            var objectContext = context.ObjectContext;

            return objectContext.GetTableName<T>();
        }

        /// <summary>
        ///     Gets the name of the table.
        /// </summary>
        /// <typeparam name="T">the entity type</typeparam>
        /// <param name="context">The context.</param>
        /// <returns>the table name</returns>
        public static string GetTableName<T>(this ObjectContext context)
            where T : class
        {
            var sql = context.CreateObjectSet<T>().ToTraceString();
            var regex = new Regex(@"FROM\s+(?<table>.+)\s+AS");
            var match = regex.Match(sql);

            var table = match.Groups["table"].Value;
            return table;
        }
    }
}