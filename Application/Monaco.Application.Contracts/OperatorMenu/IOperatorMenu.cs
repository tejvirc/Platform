////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="IOperatorMenu.cs" company="Video Gaming Technologies, Inc.">
// Copyright © 1996-2009 Video Gaming Technologies, Inc.  All rights reserved.
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Vgt.Client12.Application.OperatorMenu
{
    /// <summary>
    ///     Definition of the IOperatorMenu interface.
    /// </summary>
    public interface IOperatorMenu
    {
        /// <summary>
        ///     Command to display the operator menu now.
        /// </summary>
        void Show();

        /// <summary>
        ///     Command to hide the operator menu now.
        /// </summary>
        void Hide();

        /// <summary>
        ///     Command to take down the operator menu now.
        /// </summary>
        void Close();

        /// <summary>
        ///     Command to activate the operator menu.
        /// </summary>
        void Activate();
    }
}