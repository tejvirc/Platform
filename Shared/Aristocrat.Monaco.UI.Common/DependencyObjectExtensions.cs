namespace Aristocrat.Monaco.UI.Common
{
    using System;
    using System.Windows;
    using System.Windows.Media;

    /// <summary>
    ///     A set of <see cref="DependencyObject" /> extension methods
    /// </summary>
    public static class DependencyObjectExtensions
    {
        /// <summary>
        ///     Finds the parent of a given type for the dependency object
        /// </summary>
        /// <typeparam name="T">The DependencyObject to find</typeparam>
        /// <param name="this">The child</param>
        /// <returns>the parent DependencyObject or null</returns>
        public static T FindParent<T>(this DependencyObject @this)
            where T : DependencyObject
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            var child = @this;

            while (true)
            {
                var parentObject = VisualTreeHelper.GetParent(child);
                if (parentObject == null)
                {
                    return null;
                }

                if (parentObject is T parent)
                {
                    return parent;
                }

                child = parentObject;
            }
        }

        /// <summary>
        ///     Finds the child of a given type for the dependency object
        /// </summary>
        public static T FindChild<T>(this DependencyObject @this) where T : DependencyObject
        {
            if (@this == null)
            {
                return null;
            }

            var childrenCount = VisualTreeHelper.GetChildrenCount(@this);
            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(@this, i);
                if (child is T dependencyObject)
                {
                    return dependencyObject;
                }

                var result = FindChild<T>(child);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }
    }
}