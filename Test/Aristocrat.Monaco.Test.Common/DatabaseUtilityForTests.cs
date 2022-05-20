namespace Aristocrat.Monaco.Test.Common
{
    using System.IO;
    using Kernel;
    using Mono.Addins;

    public static class DatabaseUtilityForTests
    {
        private const string PathMapperExtensionPath = "/Kernel/PathMapper";

        private const string EventBusExtensionPath = "/Kernel/EventBus";

        private const string PropertiesManagerExtensionPath = "/Kernel/PropertiesManager";

        private const string DataPath = @"/Data";

        private const string DatabaseFileName = @"protocol.sqlite";

        private const int VerboseMonoLogLevel = 2;

        public static void PrepareDatabase()
        {
            ConfigureAddinManager();

            ConfigureFoldersAndFiles();
        }

        private static void ConfigureFoldersAndFiles()
        {
            var pathMapper = ServiceManager.GetInstance().GetService<IPathMapper>();
            var directoryInfo = pathMapper.GetDirectory(DataPath);
            if ((string.IsNullOrEmpty(directoryInfo.FullName) == false)
                && (Directory.Exists(directoryInfo.FullName) == false))
            {
                Directory.CreateDirectory(directoryInfo.FullName);
            }

            var filePath = Path.Combine(directoryInfo.FullName, DatabaseFileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        private static void ConfigureAddinManager()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update(new MonoLogger(VerboseMonoLogLevel));

            // Register PathMapper
            var typeExtensionNode = MonoAddinsHelper.GetSingleTypeExtensionNode(PathMapperExtensionPath);
            var service = (IService)typeExtensionNode.CreateInstance();
            ServiceManager.GetInstance().AddServiceAndInitialize(service);

            // Register EventBus
            typeExtensionNode = MonoAddinsHelper.GetSingleTypeExtensionNode(EventBusExtensionPath);
            service = (IService)typeExtensionNode.CreateInstance();
            ServiceManager.GetInstance().AddServiceAndInitialize(service);

            // Register PropertiesManager
            typeExtensionNode = MonoAddinsHelper.GetSingleTypeExtensionNode(PropertiesManagerExtensionPath);
            service = (IService)typeExtensionNode.CreateInstance();
            ServiceManager.GetInstance().AddServiceAndInitialize(service);
        }
    }
}