namespace Aristocrat.Monaco.Application.Meters
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Reflection;
    using Contracts;
    using log4net;
    using Mono.Addins;

    /// <summary>
    ///     Contains only composite meters.
    /// </summary>
    public class CompositeMetersProvider : IMeterProvider
    {
        private const string CompositeMetersExtensionPoint = "/Application/Metering/CompositeMeters";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Dictionary<string, CompositeMeter> _meters = new Dictionary<string, CompositeMeter>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="CompositeMetersProvider" /> class.
        /// </summary>
        public CompositeMetersProvider()
        {
            // Get the list of CompositeMeterNodes which contain a parsed
            // evaluation expression and list of dependent meters.
            var meterNodes = GetCompositeMeterNodes();

            // Check for duplicates and circular references
            if (meterNodes.Count > 1)
            {
                CheckForDuplicates(meterNodes);

                foreach (var node in meterNodes)
                {
                    CheckForCircularExpressions(node.Name, node.ExpressionMeters, meterNodes);
                }
            }

            CreateCompositeMeters(meterNodes);
        }

        /// <inheritdoc />
        public string Name => typeof(CompositeMetersProvider).ToString();

        /// <inheritdoc />
        public ICollection<string> MeterNames => _meters.Keys;

        /// <inheritdoc />
        public IMeter GetMeter(string meterName)
        {
            if (!_meters.ContainsKey(meterName))
            {
                var message = "Meter not found: " + meterName;
                Logger.Fatal(message);
                throw new MeterNotFoundException(message);
            }

            return _meters[meterName];
        }

        /// <inheritdoc />
        public void ClearPeriodMeters()
        {
        }

        /// <inheritdoc />
        public void RegisterMeterClearDelegate(ClearPeriodMeter del)
        {
        }

        private static IReadOnlyCollection<CompositeMeterNode> GetCompositeMeterNodes()
        {
            var compositeMeterNodes = new List<CompositeMeterNode>();
            foreach (CompositeMeterNode node in AddinManager.GetExtensionNodes<CompositeMeterNode>(CompositeMetersExtensionPoint))
            {
                node.Initialize();
                compositeMeterNodes.Add(node);
            }

            return compositeMeterNodes;
        }

        private static CompositeMeterNode GetCompositeMeterNode(string name, IReadOnlyCollection<CompositeMeterNode> meterNodes)
        {
            CompositeMeterNode returnNode = null;
            foreach (var node in meterNodes)
            {
                if (name == node.Name)
                {
                    returnNode = node;
                }
            }

            return returnNode;
        }

        private static void CheckForDuplicates(IEnumerable<CompositeMeterNode> meterNodes)
        {
            var metersListNames = new List<string>();
            foreach (var node in meterNodes)
            {
                // Composite meter names must be unique
                if (metersListNames.Contains(node.Name))
                {
                    var message = "Duplicate definition for composite meter: " + node.Name;
                    Logger.Fatal(message);
                    throw new ConfigurationErrorsException(message);
                }

                metersListNames.Add(node.Name);
            }
        }

        private static void CheckForCircularExpressions(
            string rootMeter,
            IEnumerable<string> expressionMeters,
            IReadOnlyCollection<CompositeMeterNode> meterNodes)
        {
            // Iterate through all meters in the expression
            foreach (var expressionMeter in expressionMeters)
            {
                // If this expression meter is the root meter, a circular reference exists
                if (expressionMeter == rootMeter)
                {
                    var message = "Circular reference for composite meter " + rootMeter;
                    Logger.Fatal(message);
                    throw new ConfigurationErrorsException(message);
                }

                // If this expression meter is a composite meter, check its expression meters
                var node = GetCompositeMeterNode(expressionMeter, meterNodes);
                if (node != null)
                {
                    CheckForCircularExpressions(rootMeter, node.ExpressionMeters, meterNodes);
                }
            }
        }

        private void CreateCompositeMeters(IEnumerable<CompositeMeterNode> meterNodes)
        {
            foreach (var node in meterNodes)
            {
                var meter = new CompositeMeter(
                    node.Name,
                    node.Expression,
                    node.ExpressionMeters,
                    node.Classification);

                _meters.Add(meter.Name, meter);

                Logger.Debug(
                    $"Created composite meter {meter.Name} of classification {meter.Classification.Name} with expression ({meter.Formula})");
            }
        }

        /// <inheritdoc />
        public virtual DateTime LastPeriodClear => DateTime.MinValue;
    }
}