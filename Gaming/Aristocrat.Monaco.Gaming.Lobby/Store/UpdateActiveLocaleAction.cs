namespace Aristocrat.Monaco.Gaming.Lobby.Store;

public record UpdateActiveLocaleAction
{
    public string? ActiveLocaleCode { get; init; }
}
