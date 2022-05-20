////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="Accessor.cs" company="ARISTOCRAT TECHNOLOGIES AUSTRALIA PTY LTD">
// COPYRIGHT © 2017 ARISTOCRAT TECHNOLOGIES AUSTRALIA PTY LTD
// Absolutely no use, dissemination or copying in any matter whatsoever
// Of this material or any portion of it is to be made without the prior
// written authorisation of Aristocrat Technologies Australia Pty Ltd.
// All rights in and to this work are fully reserved
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Test.Common
{
    using System;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     This class contains methods to access private variables and methods of a class
    ///     under test
    /// </summary>
    public static class Accessor
    {
        private static PrivateObject _accessor;

        /// <summary>
        ///     Gets or sets the PrivateObject used for reflection
        /// </summary>
        public static PrivateObject Target
        {
            get
            {
                if (_accessor != null)
                {
                    return _accessor;
                }

                throw new NullReferenceException(
                    "You must call Accessor.Target to set the PrivateObject before calling other Accessor methods");
            }

            set { _accessor = value; }
        }

        /// <summary>
        ///     Invokes the specified target class, non-static method on the target object
        /// </summary>
        /// <param name="methodName">The name of the method to invoke</param>
        /// <param name="parameters">The optional method parameters</param>
        /// <returns>The value returned by the invoked method, or null if the method does not return a value</returns>
        public static object InvokeMethod(string methodName, params object[] parameters)
        {
            return Target.Invoke(methodName, BindingFlags.NonPublic | BindingFlags.Instance, parameters);
        }

        /// <summary>
        ///     Invokes the specified target class, static method on the target object
        /// </summary>
        /// <param name="methodName">The name of the method to invoke</param>
        /// <param name="parameters">The optional method parameters</param>
        /// <returns>The value returned by the invoked method, or null if the method does not return a value</returns>
        public static object InvokeStaticMethod(string methodName, params object[] parameters)
        {
            return Target.Invoke(methodName, BindingFlags.NonPublic | BindingFlags.Static, parameters);
        }

        /// <summary>
        ///     Sets the target object's non-public, non-static field to the supplied value
        /// </summary>
        /// <param name="target">The target private object to invoke the method on</param>
        /// <param name="fieldName">The name of the field to set</param>
        /// <param name="value">The new value for the field</param>
        public static void SetField(string fieldName, object value)
        {
            Target.SetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance, value);
        }

        /// <summary>
        ///     Sets the target object's non-public, non-static field to the supplied value
        /// </summary>
        /// <param name="fieldName">The name of the field to set</param>
        /// <param name="value">The new value for the field</param>
        public static void SetStaticField(string fieldName, object value)
        {
            Target.SetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static, value);
        }

        /// <summary>
        ///     Returns the value of a non-public, non-static field in the target object's class.
        /// </summary>
        /// <param name="fieldName">The name of the field to retrieve</param>
        /// <returns>The value of the field</returns>
        public static object GetField(string fieldName)
        {
            return Target.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        }

        /// <summary>
        ///     Returns the value of a non-public, static field in the target object's class.
        /// </summary>
        /// <param name="fieldName">The name of the field to retrieve</param>
        /// <returns>The value of the field</returns>
        public static object GetStaticField(string fieldName)
        {
            return Target.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
        }
    }
}