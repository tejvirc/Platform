namespace Aristocrat.Monaco.Hardware.Contracts
{
    using System;
    using Kernel.Contracts.MessageDisplay;

    /// <summary>
    ///     ErrorGuidAttribute
    /// </summary>
    public class ErrorGuidAttribute : Attribute
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="id">The string Guid Id</param>
        public ErrorGuidAttribute(string id)
        {
            Id = new Guid(id);
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="id">The string Guid Id</param>
        /// <param name="classification">The displayable message classification</param>
        public ErrorGuidAttribute(string id, DisplayableMessageClassification classification)
        {
            Id = new Guid(id);
            Classification = classification;
        }

        /// <summary> Id </summary>
        public Guid Id { get; }

        /// <summary> Message Classification </summary>
        public DisplayableMessageClassification Classification { get; }
    }

    /// <summary>
    ///     Enum Helper Class
    /// </summary>
    public static class EnumHelper
    {
        /// <summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="en"></param>
        /// <returns></returns>
        public static T GetAttribute<T>(Enum en) where T : Attribute
        {
            var type = en.GetType();
            var memInfo = type.GetMember(en.ToString());
            if (memInfo.Length > 0)
            {
                var attrs = memInfo[0].GetCustomAttributes(typeof(T), false);
                if (attrs.Length > 0)
                {
                    return (T)attrs[0];
                }
            }

            return null;
        }
    }
}
