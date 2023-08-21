////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="IConfigWizardNavigator.cs" company="Video Gaming Technologies, Inc.">
// Copyright © 2010-2012 Video Gaming Technologies, Inc.  All rights reserved.
// Confidential and proprietary information.
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Application.Contracts.ConfigWizard
{
    /// <summary>
    ///     An interface to provide properties for the component displaying
    ///     wizards to control and indicate the page's navigation.
    /// </summary>
    /// <remarks>
    ///     The navigation is all about an ability for a page to move forwards or backwards.
    ///     For example, the component implementing this interface may provide two buttons:
    ///     <c>Next</c>Next to jump into the next new page, and <c>Previous</c> to move back to the right
    ///     previous page. If the <c>CanNavigateForward</c> is false, the <c>Next</c> button will be
    ///     disabled.
    /// </remarks>
    public interface IConfigWizardNavigator
    {
        /// <summary>
        ///     Gets or sets a value indicating whether the current config wizard is ready to move forward
        /// </summary>
        bool CanNavigateForward { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the current config wizard is ready to move backward
        /// </summary>
        bool CanNavigateBackward { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the current config wizard page displays a Back button
        /// </summary>
        bool IsBackButtonVisible { get; set; }

        /// <summary>
        ///     Use this to cause the configuration wizard to navigate to the next page.  This function should only be used
        ///     in special cases where navigation needs to be automated.  Navigation is normally handled by configuration
        ///     wizard parent page.
        /// </summary>
        void NavigateForward();
    }
}