namespace Aristocrat.Monaco.Gaming.Presentation.Store
{
    using Aristocrat.Monaco.Gaming.UI.Models;
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public record AttractSetVideosAction
    {
        public List<AttractVideoDetails>? AttractList { get; init; }
    }
}
