namespace Aristocrat.G2S.Client
{
    using System;

    /// <summary>
    ///     Defines a command handler that is prohibited when the device is disabled
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ProhibitWhenDisabledAttribute : Attribute
    {
    }
}
