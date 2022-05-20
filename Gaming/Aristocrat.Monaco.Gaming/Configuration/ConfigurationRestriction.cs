namespace Aristocrat.Monaco.Gaming.Configuration
{
    using System;
    using Contracts.Configuration;

    internal class ConfigurationRestriction : IConfigurationRestriction, IEquatable<ConfigurationRestriction>
    {
        public ConfigurationRestriction(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public IGameConfiguration Game { get; set; }

        /// <inheritdoc />
        public bool Equals(ConfigurationRestriction other) => Name == other?.Name;

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is ConfigurationRestriction c && Equals(c);

        /// <inheritdoc />
        public override int GetHashCode() => Name?.GetHashCode() ?? 0;

        public static bool operator ==(ConfigurationRestriction left, ConfigurationRestriction right) => Equals(left, right);

        public static bool operator !=(ConfigurationRestriction left, ConfigurationRestriction right) => !Equals(left, right);
    }
}