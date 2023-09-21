namespace Aristocrat.Monaco.Gaming.Presentation.Services.InfoBar
{
    using System;
    using Aristocrat.Monaco.Gaming.Contracts.InfoBar;

    public interface IInfoBarService
    {
        void ClearMessage(Guid ownerId, InfoBarRegion region);
    }
}
