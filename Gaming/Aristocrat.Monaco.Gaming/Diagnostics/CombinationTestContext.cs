namespace Aristocrat.Monaco.Gaming.Diagnostics
{
    using System.Collections.Generic;
    using Contracts;
    using Contracts.Diagnostics;

    public class CombinationTestContext : IDiagnosticContext<object>, ICombinationTestContext
    {
        public IReadOnlyDictionary<string, string> GetParameters()
        {
            return new Dictionary<string, string> { { "/Runtime/CombinationTest", "spin" } };
        }

        public object Arguments => null;
    }
}