namespace Aristocrat.Monaco.Common.Storage
{
    using Microsoft.EntityFrameworkCore;

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
        public static string GetTableName<T>(this DbContext context)
            where T : class
        {
            var m = context.Model.FindEntityType(typeof(T));
            return m.GetSchemaQualifiedTableName();
        }
    }
}