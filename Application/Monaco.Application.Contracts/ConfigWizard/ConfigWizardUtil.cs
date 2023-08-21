namespace Aristocrat.Monaco.Application.Contracts.ConfigWizard
{
    using Kernel;

    /// <summary>
    /// Config wizard configuration-related utilities.
    /// </summary>
    public static class ConfigWizardUtil
    {
        /// <summary>
        /// Is the given identity property configured by the current jurisdiction to be visible in the Audit Menu?
        /// </summary>
        /// <param name="properties">The manager to search for the override.</param>
        /// <param name="configPropertyName">The name of the field's <see cref="IdentityFieldOverride"/></param>
        /// <returns>The field's visibility if an override exists; else true.</returns>
        public static bool VisibleByConfig(IPropertiesManager properties, string configPropertyName)
        {
            var configOverride = properties.GetValue<IdentityFieldOverride>(configPropertyName, null);
            if (configOverride == null)
            {
                return true;
            }
            return configOverride.Visible == Presence.Always || configOverride.Visible == Presence.MenuOnly;
        }
    }
}