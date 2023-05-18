namespace Stubs
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Monaco.Kernel;

    #region Stub Service 1

    public interface IService1 { }

    public class PlatformDisplay : IService, IService1
    {
        public string Name => "Stub PlatformDisplay";

        public ICollection<Type> ServiceTypes => new[] { typeof(IService1) };

        public void Initialize()
        {
        }
    }

    #endregion

    #region Stub Service 2

    public interface IService2 { }

    public class WindowLauncher : IService, IService2
    {
        public string Name => "Stub WindowLauncher";

        public ICollection<Type> ServiceTypes => new[] { typeof(IService2) };

        public void Initialize()
        {
        }
    }

    #endregion
}
