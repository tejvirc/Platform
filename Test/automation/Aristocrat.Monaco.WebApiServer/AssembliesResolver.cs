namespace Aristocrat.Monaco.WebApiServer
{
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Web.Http.Dispatcher;
     
    public class AssembliesResolver : IAssembliesResolver
    {
        private readonly string[] _paths = {
                                    $"{AppDomain.CurrentDomain.BaseDirectory}\\LoadControllers\\",
                                    $"{AppDomain.CurrentDomain.BaseDirectory}\\LoadDevControllers\\"
                                  };

        private readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static IReadOnlyCollection<string> AssemblyList => _assemblyList;

        private static readonly List<string> _assemblyList = new();

        public ICollection<Assembly> GetAssemblies()
        {
            List<Assembly> baseAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();

            var initialAssemblyCount = baseAssemblies.Count;

            foreach (var path in _paths)
            {
                _logger.Info($"Loading Controller DLLs from '{path}'");

                try
                {
                    foreach (string dll in Directory.GetFiles(path, "*.dll", SearchOption.TopDirectoryOnly))
                    {
                        Assembly loadedAssembly = Assembly.LoadFile(dll);
                        _assemblyList.Add(loadedAssembly.FullName.Split(',')[0]);
                        _logger.Info($"DLL loaded : '{loadedAssembly.FullName}'");

                        Type type = loadedAssembly.GetType($"{loadedAssembly.FullName.Split(',')[0]}.Setup");
                        if (type != null)
                        {
                            _logger.Info($"Setup class found for : '{loadedAssembly.FullName}'");
                            MethodInfo methodInfo = type.GetMethod("InitialSetup");
                            if (methodInfo != null)
                            {
                                _logger.Info($"InitialSetup function invoked for : '{loadedAssembly.FullName}'");
                                methodInfo.Invoke(null, null);
                            }
                        }

                        baseAssemblies.Add(loadedAssembly);
                    }
                }
                catch (Exception e)
                {
                    _logger.Error($"Error occurred while loading API DLLs : '{e.Message}'");
                }
            }

            _logger.Info($" Number of Web APIs loaded : {baseAssemblies.Count - initialAssemblyCount }");

            return baseAssemblies;
        }
    }
}
