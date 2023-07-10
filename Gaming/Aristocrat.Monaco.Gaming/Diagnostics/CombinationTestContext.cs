namespace Aristocrat.Monaco.Gaming.Diagnostics
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Contracts;
    using Contracts.Diagnostics;

    public class CombinationTestContext : IDiagnosticContext<object>, ICombinationTestContext
    {
        private readonly Dictionary<string, string> _parameters = new()
        {
            { "/Runtime/CombinationTest", "spin" }
        };

        public IReadOnlyDictionary<string, string> GetParameters()
        {
            return new ReadOnlyDictionary<string, string>(_parameters);
        }

        public CombinationTestContext(IDictionary<string, string> parameters = null)
        {
            if (parameters is null)
            {
                return;
            }

            foreach (var kvp in parameters)
            {
                _parameters[kvp.Key] = kvp.Value;
            }
        }

        public object Arguments => null;
    }
}