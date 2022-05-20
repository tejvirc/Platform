namespace Aristocrat.Monaco.UI.Common
{
    using System;
    using System.Windows;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media;

    /// <summary>
    ///     Helper class for traversing the visual tree.
    /// </summary>
    public static class TreeHelper
    {
        /// <summary>
        ///     The the parent element.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static DependencyObject GetParent(DependencyObject element)
        {
            return GetParent(element, true);
        }

        /// <summary>
        ///     Find the first parent element of a specific type.
        /// </summary>
        /// <typeparam name="T">The target type for the element returned. Descendent of the <see cref="DependencyObject"/> class.</typeparam>
        /// <param name="startingObject">The object from which to start traversing up the tree.</param>
        /// <returns>First parent element matching the specified criteria.</returns>
        public static T FindParent<T>(DependencyObject startingObject)
            where T : DependencyObject
        {
            return FindParent<T>(startingObject, false, null);
        }

        /// <summary>
        ///      Find the first parent element of a specific type.
        /// </summary>
        /// <typeparam name="T">The target type for the element returned. Descendent of the <see cref="DependencyObject"/> class.</typeparam>
        /// <param name="startingObject">The object from which to start traversing up the tree.</param>
        /// <param name="checkStartingObject">Determines if the starting object is included in the search.</param>
        /// <returns>First parent element matching the specified criteria.</returns>
        public static T FindParent<T>(DependencyObject startingObject, bool checkStartingObject)
            where T : DependencyObject
        {
            return FindParent<T>(startingObject, checkStartingObject, null);
        }

        /// <summary>
        ///     Find the first parent element of a specific type.
        /// </summary>
        /// <typeparam name="T">The target type for the element returned. Descendent of the <see cref="DependencyObject"/> class.</typeparam>
        /// <param name="startingObject">The object from which to start traversing up the tree.</param>
        /// <param name="checkStartingObject">Determines if the starting object is included in the search.</param>
        /// <param name="additionalCheck">A function used as part of the criteria check.</param>
        /// <returns>First parent element matching the specified criteria.</returns>
        public static T FindParent<T>(
            DependencyObject startingObject,
            bool checkStartingObject,
            Func<T, bool> additionalCheck)
            where T : DependencyObject
        {
            var parent = (checkStartingObject ? startingObject : GetParent(startingObject, true));

            while (parent != null)
            {
                if (parent is T foundElement)
                {
                    if (additionalCheck == null)
                    {
                        return foundElement;
                    }

                    if (additionalCheck(foundElement))
                    {
                        return foundElement;
                    }
                }

                parent = GetParent(parent, true);
            }

            return null;
        }

        /// <summary>
        ///     Find the first child element of a specific type.
        /// </summary>
        /// <typeparam name="T">The target type for the element returned. Descendent of the <see cref="DependencyObject"/> class.</typeparam>
        /// <param name="startingObject">The object from which to start traversing up the tree.</param>
        /// <returns>First child element matching the specified criteria.</returns>
        public static T FindChild<T>(DependencyObject startingObject)
            where T : DependencyObject
        {
            return FindChild<T>(startingObject, null);
        }

        /// <summary>
        ///     Find the first child element of a specific type.
        /// </summary>
        /// <typeparam name="T">The target type for the element returned. Descendent of the <see cref="DependencyObject"/> class.</typeparam>
        /// <param name="startingObject">The object from which to start traversing up the tree.</param>
        /// <param name="additionalCheck">A function used as part of the criteria check.</param>
        /// <returns>First child element matching the specified criteria.</returns>
        public static T FindChild<T>(DependencyObject startingObject, Func<T, bool> additionalCheck)
            where T : DependencyObject
        {
            var childrenCount = VisualTreeHelper.GetChildrenCount(startingObject);
            T child;

            for (var index = 0; index < childrenCount; index++)
            {
                child = VisualTreeHelper.GetChild(startingObject, index) as T;

                if (child == null)
                {
                    continue;
                }

                if (additionalCheck == null)
                {
                    return child;
                }

                if (additionalCheck(child))
                {
                    return child;
                }
            }

            for (var index = 0; index < childrenCount; index++)
            {
                child = FindChild(VisualTreeHelper.GetChild(startingObject, index), additionalCheck);

                if (child != null)
                {
                    return child;
                }
            }

            return null;
        }

        /// <summary>
        ///     Determines if and element is a child of another element.
        /// </summary>
        /// <param name="element">Child element.</param>
        /// <param name="parent">Parent element.</param>
        /// <returns>Returns true if child is a descendant of parent. Otherwise, false.</returns>
        public static bool IsDescendantOf(DependencyObject element, DependencyObject parent)
        {
            return IsDescendantOf(element, parent, true);
        }

        /// <summary>
        ///     Determines if and element is a child of another element.
        /// </summary>
        /// <param name="element">Child element.</param>
        /// <param name="parent">Parent element.</param>
        /// <param name="recurseIntoPopup">Include popups.</param>
        /// <returns>Returns true if child is a descendant of parent. Otherwise, false.</returns>
        public static bool IsDescendantOf(DependencyObject element, DependencyObject parent, bool recurseIntoPopup)
        {
            while (element != null)
            {
                if (Equals(element, parent))
                {
                    return true;
                }

                element = GetParent(element, recurseIntoPopup);
            }

            return false;
        }

        private static DependencyObject GetParent(DependencyObject element, bool recurseIntoPopup)
        {
            if (recurseIntoPopup)
            {
                var popup = element as Popup;

                if (popup?.PlacementTarget != null)
                {
                    return popup.PlacementTarget;
                }
            }

            var parent = !(element is Visual visual) ? null : VisualTreeHelper.GetParent(visual);

            if (parent != null)
            {
                return parent;
            }


            if (element is FrameworkElement fe)
            {
                parent = fe.Parent ?? fe.TemplatedParent;
            }
            else
            {

                if (!(element is FrameworkContentElement fce))
                {
                    return null;
                }

                parent = fce.Parent ?? fce.TemplatedParent;
            }

            return parent;
        }
    }
}
