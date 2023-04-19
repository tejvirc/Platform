namespace Stubs
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Monaco.Kernel;

    #region Stub Service 1

    public interface IService1 { }

    public class StubService1 : IService, IService1
    {
        public string Name => "Stub Service 1";

        public ICollection<Type> ServiceTypes => new[] { typeof(IService1) };

        public void Initialize()
        {
        }
    }

    #endregion

    #region Stub Service 2

    public interface IService2 { }

    public class StubService2 : IService, IService2
    {
        public string Name => "Stub Service 2";

        public ICollection<Type> ServiceTypes => new[] { typeof(IService2) };

        public void Initialize()
        {
        }
    }

    #endregion

    #region Stub Service 3

    public interface IService3 { }

    public class StubService3 : IService, IService3
    {
        public string Name => "Stub Service 3";

        public ICollection<Type> ServiceTypes => new[] { typeof(IService3) };

        public void Initialize()
        {
        }
    }

    #endregion
}
