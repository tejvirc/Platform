namespace Aristocrat.Monaco.Gaming.Presentation.Services;

using System.Threading.Tasks;

public interface IPresentationService
{
    public Task StartAsync();

    public Task StopAsync();
}
