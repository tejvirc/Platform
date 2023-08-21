namespace Aristocrat.Monaco.UI.Common
{
    using System.ComponentModel;

    /// <summary>
    ///     An extension class of PropertyChangedEventArgs that includes the old and new values
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ExtendedPropertyChangedEventArgs<T> : PropertyChangedEventArgs
    {
        /// <summary>
        ///     Old value of property that changed
        /// </summary>
        public virtual T OldValue { get; }

        /// <summary>
        ///     New value of property that changed
        /// </summary>
        public virtual T NewValue { get; }

        /// <summary>
        ///     Constructor for ExtendedPropertyChangedEventArgs
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        public ExtendedPropertyChangedEventArgs(string propertyName, T oldValue, T newValue)
            : base(propertyName)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
