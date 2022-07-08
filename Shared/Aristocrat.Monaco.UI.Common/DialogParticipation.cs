namespace Aristocrat.Monaco.UI.Common
{
    using System;
    using System.Collections.Generic;
    using System.Windows;

    /// <summary>
    ///     Registers a view, which provides a mechanism to assign the owner/parent of a dialog.
    /// </summary>
    public static class DialogParticipation
    {
        /// <summary>
        ///     Attached property for registering a dialog
        /// </summary>
        public static readonly DependencyProperty RegisterProperty = DependencyProperty.RegisterAttached(
            "Register",
            typeof(object),
            typeof(DialogParticipation),
            new PropertyMetadata(default(object), RegisterPropertyChangedCallback));

        private static readonly IDictionary<Type, WeakReference<DependencyObject>> ContextRegistrationIndex =
            new Dictionary<Type, WeakReference<DependencyObject>>();

        /// <summary>
        ///     Registers the dependency object
        /// </summary>
        /// <param name="element">The dependency object</param>
        /// <param name="context">The context</param>
        public static void SetRegister(DependencyObject element, object context)
        {
            CollectTheDead();

            element.SetValue(RegisterProperty, context);
        }

        /// <summary>
        ///     Gets the associated object.
        /// </summary>
        /// <param name="element">The dependency object.</param>
        /// <returns>The associated object.</returns>
        public static object GetRegister(DependencyObject element)
        {
            return element.GetValue(RegisterProperty);
        }

        /// <summary>
        ///     Checks if the context is registered.
        /// </summary>
        /// <param name="context">The context to lookup</param>
        /// <returns>true if registered.</returns>
        internal static bool IsRegistered(object context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            CollectTheDead();

            return ContextRegistrationIndex.TryGetValue(context.GetType(), out var registration) &&
                   registration.TryGetTarget(out var _);
        }

        /// <summary>
        ///     Gets the associated dependency object
        /// </summary>
        /// <param name="context">The context</param>
        /// <returns>The dependency object</returns>
        internal static DependencyObject GetAssociation(object context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!IsRegistered(context))
            {
                return null;
            }

            return !ContextRegistrationIndex[context.GetType()].TryGetTarget(out var dependencyObject) ? null : dependencyObject;
        }

        private static void RegisterPropertyChangedCallback(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            CollectTheDead();

            if (dependencyPropertyChangedEventArgs.OldValue != null)
            {
                ContextRegistrationIndex.Remove(dependencyPropertyChangedEventArgs.OldValue.GetType());
            }

            if (dependencyPropertyChangedEventArgs.NewValue != null)
            {
                ContextRegistrationIndex[dependencyPropertyChangedEventArgs.NewValue.GetType()] =
                    new WeakReference<DependencyObject>(dependencyObject);
            }
        }

        private static void CollectTheDead()
        {
            var dead = new List<Type>();
            foreach (var registration in ContextRegistrationIndex)
            {
                if (registration.Value == null || !registration.Value.TryGetTarget(out _))
                {
                    dead.Add(registration.Key);
                }
            }

            foreach (var deadRegistration in dead)
            {
                ContextRegistrationIndex.Remove(deadRegistration);
            }
        }
    }
}