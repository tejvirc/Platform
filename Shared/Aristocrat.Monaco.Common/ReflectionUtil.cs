namespace Aristocrat.Monaco.Common
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Utilities for working with reflection, expressions, etc. in a type-safe way (where possible).
    /// </summary>
    public static class ReflectionUtil
    {
        /// <summary>
        /// Gets the name of a member from the given "parent => parent.member" expression.
        /// </summary>
        /// <typeparam name="TParent">The parent type.</typeparam>
        /// <typeparam name="TMember">The member type.</typeparam>
        /// <param name="memberExpression">The expression, e.g. parent => parent.SomeMember</param>
        /// <returns>The name of the member, e.g. "SomeMember".</returns>
        public static string GetFieldName<TParent, TMember>(Expression<Func<TParent, TMember>> memberExpression)
        {
            return ((MemberExpression)memberExpression.Body).Member.Name;
        }

        /// <summary>
        /// Gets the name of a member from the given parent and "parent => parent.member" expression.
        /// </summary>
        /// <typeparam name="TParent">The parent type.</typeparam>
        /// <typeparam name="TMember">The member type.</typeparam>
        /// <param name="parent">The parent object (used merely for type inference).</param>
        /// <param name="memberExpression">The expression, e.g. parent => parent.SomeMember</param>
        /// <returns>The name of the member, e.g. "SomeMember".</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public static string GetFieldName<TParent, TMember>(TParent parent, Expression<Func<TParent, TMember>> memberExpression)
        {
            return ((MemberExpression)memberExpression.Body).Member.Name;
        }
    }
}
