namespace Aristocrat.Monaco.PackageManifest.Models
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.PackageManifest.Extension.v100;

    /// <summary>
    ///     <see cref="LineOption"/>.
    /// </summary>
    public class LineOptionList : IEnumerable<LineOption>
    {
        private readonly IEnumerable<LineOption> _lineOptions;

        /// <summary>
        ///     Creates a LineOptionList from a corresponding manifest object
        /// </summary>
        public LineOptionList(IEnumerable<c_lineOption> options)
        {
            if (options == null)
            {
                _lineOptions = Enumerable.Empty<LineOption>();
                return;
            }

            _lineOptions =
                from o in options
                select new LineOption
                {
                    Name = o.name,
                    Description = o.description,
                    Lines =
                        from line in o.line
                        select new Line { Button = line.button, ButtonName = line.buttonName, Cost = line.cost, Multiplier = line.costMultiplier }
                };
        }

        /// <summary>
        ///     Creates a LineOptionList from an IEnumerable LineOptions.
        /// </summary>
        /// <param name="options"></param>
        public LineOptionList(IEnumerable<LineOption> options)
        {
            _lineOptions = options.Select(i => i);
        }
        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        public IEnumerator<LineOption> GetEnumerator() => _lineOptions.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}