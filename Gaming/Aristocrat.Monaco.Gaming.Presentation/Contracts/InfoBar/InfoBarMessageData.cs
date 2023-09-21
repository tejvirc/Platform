namespace Aristocrat.Monaco.Gaming.Presentation.Contracts.InfoBar;

using System;
using System.Threading;
using System.Threading.Tasks;
using Aristocrat.Monaco.Gaming.Contracts.InfoBar;

public class InfoBarMessageData
{
    public Guid OwnerId;
    public InfoBarRegion Region;
    public string Text;
    public InfoBarColor TextColor;
    public InfoBarColor BackgroundColor;
    public CancellationTokenSource Cts;
    public Task TransientMessageTask;
    public double Duration;
}
