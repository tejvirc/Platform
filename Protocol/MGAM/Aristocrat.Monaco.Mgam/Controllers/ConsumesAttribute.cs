namespace Aristocrat.Monaco.Mgam.Controllers
{
    using System;

    /// <summary>
    ///     Decorates a message handler method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ConsumesAttribute : Attribute
    {
    }
}
