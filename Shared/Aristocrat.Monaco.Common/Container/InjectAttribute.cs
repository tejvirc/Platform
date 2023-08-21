namespace Aristocrat.Monaco.Common.Container
{
    using System;

    /// <summary>
    ///     Used to inject instances from the container into properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class InjectAttribute : Attribute
    {
    }
}
