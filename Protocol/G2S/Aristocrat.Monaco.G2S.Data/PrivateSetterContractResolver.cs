namespace Aristocrat.Monaco.G2S.Data
{
    using System.Reflection;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    ///     Custom JSON.Net contract resolver for setting protected/private properties
    /// </summary>
    public class PrivateSetterContractResolver : DefaultContractResolver
    {
        /// <inheritdoc />
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if (property.Writable)
            {
                return property;
            }

            property.Writable = member.IsPropertyWithSetter();

            return property;
        }
    }

#pragma warning disable SA1402 // File may only contain a single class
    /// <summary>
    ///     MemberInfo extension methods
    /// </summary>
    internal static class MemberInfoExtensions
#pragma warning restore SA1402 // File may only contain a single class
    {
        /// <summary>
        ///     Determines if the MemberInfo is a property and has a setter.
        /// </summary>
        /// <param name="member">The MemberInfo instance</param>
        /// <returns>True if it's a property and has a setter, otherwise false.</returns>
        internal static bool IsPropertyWithSetter(this MemberInfo member)
        {
            var property = member as PropertyInfo;

            return property?.GetSetMethod(true) != null;
        }
    }
}