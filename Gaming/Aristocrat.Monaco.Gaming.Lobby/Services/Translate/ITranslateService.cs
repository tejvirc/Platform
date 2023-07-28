namespace Aristocrat.Monaco.Gaming.Lobby.Services.Translate;

using System.Threading.Tasks;

public interface ITranslateService
{
    string GetSelectedLocaleCode();

    Task SetSelectedLocaleCodeAsync();

    void SetDefaultSelectedLocaleCode();
}
