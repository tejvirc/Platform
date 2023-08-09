namespace Aristocrat.Monaco.Kernel
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Definition of the DummyPlatformDisplay class.
    /// </summary>
    /// <remarks>All members of this class purposefully throw a NotImplementedException.</remarks>
    public class DummyPlatformDisplay : IPlatformDisplay, IService
    {
        public bool IsVisible => true;

        public void CreateAndShow()
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Hide()
        {
            throw new NotImplementedException();
        }

        public void Shutdown(bool closeApplication)
        {
            throw new NotImplementedException();
        }

        public void Show()
        {
            throw new NotImplementedException();
        }

        public string Name => "PlatformDisplay Test Dummy";

        public ICollection<Type> ServiceTypes => new[] { typeof(IPlatformDisplay) };

        public void Initialize()
        {
            throw new NotImplementedException();
        }
    }
}