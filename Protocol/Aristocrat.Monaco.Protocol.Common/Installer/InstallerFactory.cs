namespace Aristocrat.Monaco.Protocol.Common.Installer
{ 
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Kernel.Contracts;

    /// <summary>
    ///     Allows for the registration of package installers by type
    /// </summary>
    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Justification = "Serializable isn't required. This is only used during appliction composition.")]
    public sealed class InstallerFactory : Dictionary<string, IInstaller>, IInstallerFactory
    {
        /// <inheritdoc />
        IInstaller IInstallerFactory.CreateNew(string name)
        {
            return this[name];
        }
    }
}
