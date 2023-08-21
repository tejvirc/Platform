namespace Aristocrat.Monaco.Gaming.Presentation.Services;

using System.Threading.Tasks;

public interface IPresentation
{
    public Task StartAsync();

    public Task StopAsync();
}
