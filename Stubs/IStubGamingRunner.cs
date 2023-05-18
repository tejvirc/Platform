namespace Stubs;

using Aristocrat.Monaco.Kernel;

public interface IStubGamingRunner : IService
{
    public void Run();
    public void Stop();
}
