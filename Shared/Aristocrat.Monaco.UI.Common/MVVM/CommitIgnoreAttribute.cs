namespace Aristocrat.Monaco.UI.Common.MVVM
{
    using System;

    /// <summary>
    /// For use with <see cref="CustomObservableValidator"/> to indicate that a property should be ignored for commits.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    internal class CommitIgnoreAttribute : Attribute
    {
    }
}
