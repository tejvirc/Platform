namespace Aristocrat.Monaco.Application.Meters
{
    using System;
    using System.Collections.Generic;
    using Mono.Addins;

    // Disable the warning that a field is never assigned to and will
    // always have its default value.  Below, values for 'name',
    // 'classification' and 'expression' are assigned via reflection
    // in Mono.Addins.
#pragma warning disable 0649

    /// <summary>
    ///     CompositeMeterNode is a Mono.Addins ExtensionNode used to create
    ///     composite meter extensions.
    /// </summary>
    [CLSCompliant(false)]
    public class CompositeMeterNode : ExtensionNode
    {
        /// <summary>
        ///     The math operations and grouping symbols that may exist in a composite
        ///     meter expression.
        /// </summary>
        private static readonly char[] ExpressionSeparators =
            { '+', '-', '*', '/', '%', '^', '(', ')' };

        /// <summary>
        ///     The name of this meter's classification
        /// </summary>
        [NodeAttribute("classification")] private string _classification;

        /// <summary>
        ///     The expression for calculating the meter value
        /// </summary>
        [NodeAttribute("expression")] private string _expression;

        /// <summary>
        ///     The list of meter names in the expression, in the order in
        ///     which they appear in the expression, minus duplicates.
        /// </summary>
        private List<string> _expressionMeters;

        /// <summary>
        ///     The name of the meter
        /// </summary>
        [NodeAttribute("name")] private string _name;

        /// <summary>
        ///     Gets the name of the meter
        /// </summary>
        public string Name => _name;

        /// <summary>
        ///     Gets the classification name for the meter
        /// </summary>
        public string Classification => _classification;

        /// <summary>
        ///     Gets the expression for calculating the meter
        /// </summary>
        public string Expression => _expression;

        /// <summary>
        ///     Gets the list of names of the meters used in the expression,
        ///     in the order in which they appear in the expression.
        /// </summary>
        public IList<string> ExpressionMeters => _expressionMeters;

        /// <summary>
        ///     Parses the expression to build the list of meter names
        ///     contained in the expression.
        /// </summary>
        public void Initialize()
        {
            _expressionMeters = new List<string>();

            // Split the expression at each operator or parenthesis.
            var meterNames = _expression.Split(ExpressionSeparators);

            foreach (var meterName in meterNames)
            {
                // Make sure the string is not null, empty, a number, or
                // already in the list.
                var trimmedName = meterName.Trim();
                if (!string.IsNullOrEmpty(trimmedName) && !long.TryParse(trimmedName, out _) && !_expressionMeters.Contains(trimmedName))
                {
                    _expressionMeters.Add(meterName.Trim());
                }
            }
        }
    }

#pragma warning restore 0649
}