namespace Aristocrat.Monaco.Gaming.Presentation.Services.InfoBar;
using System;
using System.Threading;
using System.Threading.Tasks;
using Aristocrat.Monaco.Gaming.Contracts.InfoBar;

public interface IInfoBarService
{
    Task ClearMessage(Guid ownerId, InfoBarRegion region);
    Task DisplayTransientMessage(Guid ownerId, string message, InfoBarRegion region, InfoBarColor textColor, InfoBarColor backgroundColor, TimeSpan duration, CancellationToken token);
}