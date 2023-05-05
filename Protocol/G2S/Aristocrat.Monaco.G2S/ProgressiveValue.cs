namespace Aristocrat.Monaco.G2S
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class ProgressiveValue
    {
        public int LevelId { get; set; } = 0;

        public int ProgressiveId { get; set; } = 0;

        public long ProgressiveValueAmount { get; set; } = 0L;
        public string ProgressiveValueText { get; set; } = "";
        public long ProgressiveValueSequence { get; set; } = 0L;

    }
}
