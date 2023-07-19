namespace Aristocrat.Monaco.UI.Common.MVVM
{
    using System;

    /// <summary>
    /// For use with <see cref="TrackableObservableValidator"/> to indicate that a property should be ignored for commits.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class CommitIgnoreAttribute : Attribute
    {
    }
}
