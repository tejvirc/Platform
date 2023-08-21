namespace Aristocrat.Monaco.Hhr.Storage.Helpers
{
    using System.Collections.Generic;
    using Models;

    public interface IProgressiveUpdateEntityHelper
    {
        IEnumerable<ProgressiveUpdateEntity> ProgressiveUpdates { get; set; }
        
    }
}