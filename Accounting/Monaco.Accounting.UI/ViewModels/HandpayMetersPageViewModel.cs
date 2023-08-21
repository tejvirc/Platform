namespace Aristocrat.Monaco.Accounting.UI.ViewModels
{
    using System;
    using Application.Contracts.MeterPage;
    using Application.UI.OperatorMenu;

    [CLSCompliant(false)]
    public class HandpayMetersPageViewModel : MetersPageViewModelBase
    {
        public HandpayMetersPageViewModel()
            : base(MeterNodePage.Handpay)
        {
        }
    }
}