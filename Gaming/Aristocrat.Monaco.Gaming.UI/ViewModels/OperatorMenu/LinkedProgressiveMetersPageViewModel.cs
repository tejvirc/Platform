namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Application.Contracts.MeterPage;
    using Application.UI.OperatorMenu;
    using Aristocrat.Extensions.CommunityToolkit;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Aristocrat.Monaco.Application.UI.Events;
    using Aristocrat.Monaco.Application.UI.MeterPage;
    using Localization.Properties;

    public class LinkedProgressiveMetersPageViewModel : MetersPageViewModelBase
    {
        public LinkedProgressiveMetersPageViewModel()
            : base(MeterNodePage.LinkedProgressives)
        {

        }
    }
}