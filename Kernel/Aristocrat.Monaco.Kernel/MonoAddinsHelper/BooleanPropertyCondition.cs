namespace Aristocrat.Monaco.Kernel
{
    using log4net;
    using Mono.Addins;
    using System;
    using System.Globalization;
    using System.Reflection;

    /// <summary>
    ///     An implementation of mono-addins abstract ConditionType class used to conditionally include an add-in during
    ///     the discovery process depending on whether or not a given property is the specified value.
    /// </summary>
    /// <remarks>
    ///     Mono.Addins instantiates one object of this type when there is a ConditionType declared with this class name
    ///     specified for the type
    ///     in the addin.xml for a host. Multiple declarations can use this class name, but they must be distinguished with
    ///     different id attributes,
    ///     which are the names of properties checked by objects of the class. There will be one object per declaration.
    ///     Evaluate is called, and a Condition node is passed in, for each extension that uses the condition. The evaluation
    ///     of
    ///     conditions occurs the first time a host
    ///     obtains its extensions. In order for a reevaluation of a condition to occur, NotifyChanged must be called on the
    ///     base
    ///     class when something
    ///     occurs that may result in a change in the evaluation of the condition.
    ///     <example>
    ///         To apply a condition in the .addin.xml for an extension, conditional elements in an extension node are enclosed
    ///         in
    ///         a condition as shown below.
    ///         The id attribute is mandatory and must match the id used in a ConditionType declaration. The defaultValue and
    ///         compareTo attributes are optional.
    ///         The defaultValue attribute is the assumed value of a property if it is not found in the Properties Manager. It
    ///         defaults to false if omitted.
    ///         The compareTo attribute is the value to compare the property to. This gets set to false when you want the
    ///         Evaluate
    ///         method to return the opposite
    ///         of the property value. Its default is true, so when omitted, the value of the property is what gets returned by
    ///         Evaluate.
    ///         <code>
    ///     {Extension path = "/Application/OperatorMenu/DisplayMeters"}
    ///     {Condition id="Cabinet.isParticipatingInProgressive" defaultValue="false" compareTo="true"}
    ///     ...
    ///     {/Condition}
    /// {/Extension}
    /// </code>
    ///     </example>
    /// </remarks>
    [CLSCompliant(false)]
    public class BooleanPropertyCondition : ConditionType
    {
        /// <summary>
        ///     Name of attribute with a value that matches ConditionType declaration and identifies the property to be checked.
        /// </summary>
        private const string NameOfIdAttribute = "id";

        /// <summary>
        ///     Name of attribute with a value that the property is being compared with.
        /// </summary>
        private const string NameOfCompareToAttribute = "compareTo";

        /// <summary>
        ///     Name of attribute with a value identifying the default value of the property being checked
        /// </summary>
        private const string NameOfDefaultValueAttribute = "defaultValue";

        /// <summary>
        ///     Create a logger for use in this class.
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        /// <summary>
        ///     Property name sent to Evaluate. Should not change after being sent the first time.
        /// </summary>
        private string _propertyName = string.Empty;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BooleanPropertyCondition" /> class.
        /// </summary>
        public BooleanPropertyCondition()
        {
            Logger.Debug("Creating a BooleanPropertyCondition object");
        }

        /// <summary>
        ///     Gets a property name and default value from a node from a Mono.Addins configuration file and looks up the
        ///     value in the Properties Manager.
        /// </summary>
        /// <param name="conditionNode">A condition node from a Mono.Addins configuration file.</param>
        /// <returns>A boolean value indicating whether or not a value obtained from the Properties Manager is the specified value.</returns>
        /// <exception cref="System.InvalidCastException">Thrown when the object returned by the Properties Manager is not a bool.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the node passed in does not have an id attribute.</exception>
        public override bool Evaluate(NodeElement conditionNode)
        {
            var propertyName = conditionNode.GetAttribute(NameOfIdAttribute);

            if (string.IsNullOrEmpty(propertyName))
            {
                Logger.Fatal("A NodeElement object without an id attribute was passed into Evaluate");
                throw new ArgumentException("A NodeElement object without an id attribute was passed into Evaluate");
            }

            if (_propertyName.Length == 0)
            {
                _propertyName = propertyName;

                // This is the first time the property is evaluated, subscribe to PropertyChangedEvent so that
                // property changes will trigger a reevaluation.
                var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
                eventBus.Subscribe<PropertyChangedEvent>(this, HandlePropertyChanged, evt => evt.PropertyName == _propertyName);
            }
            else if (_propertyName != propertyName)
            {
                Logger.FatalFormat(
                    CultureInfo.InvariantCulture,
                    "A NodeElement object {0} was passed with an unexpected id.  Expected id to be {1}.",
                    propertyName,
                    _propertyName);
                throw new ArgumentException(
                    "A NodeElement object with an unexpected id attribute value was passed into Evaluate");
            }

            Logger.Debug("Entering Evaluate, which returns the value of " + propertyName + " or its opposite");

            var valueToCompareWith = true;
            var hasCompareTo = conditionNode.GetAttribute(NameOfCompareToAttribute) != null &&
                               bool.TryParse(
                                   conditionNode.GetAttribute(NameOfCompareToAttribute),
                                   out valueToCompareWith);

            valueToCompareWith = !hasCompareTo || valueToCompareWith;

            var defaultValue = false;
            var hasDefault = conditionNode.GetAttribute(NameOfDefaultValueAttribute) != null &&
                             bool.TryParse(conditionNode.GetAttribute(NameOfDefaultValueAttribute), out defaultValue);

            defaultValue = hasDefault && defaultValue;

            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();

            return (bool?)propertiesManager?.GetProperty(propertyName, defaultValue) == valueToCompareWith;
        }

        private void HandlePropertyChanged(PropertyChangedEvent data)
        {
            NotifyChanged();
        }
    }
}