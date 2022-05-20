namespace Aristocrat.G2S.Emdi.Handlers
{
    using System;

    /// <summary>
    /// Attribute that can be set on command handler class to indicate if a valid session is required to execute the handler
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RequiresValidSessionAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="required"></param>
        public RequiresValidSessionAttribute(bool required = true)
        {
            Required = required;
        }

        /// <summary>
        /// Gets a value indicating whether a valid session is required
        /// </summary>
        public bool Required { get; }
    }
}