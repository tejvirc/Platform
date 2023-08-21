////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="DynamicPrivateObject.cs" company="ARISTOCRAT TECHNOLOGIES AUSTRALIA PTY LTD">
// COPYRIGHT © 2017 ARISTOCRAT TECHNOLOGIES AUSTRALIA PTY LTD
// Absolutely no use, dissemination or copying in any matter whatsoever
// Of this material or any portion of it is to be made without the prior
// written authorisation of Aristocrat Technologies Australia Pty Ltd.
// All rights in and to this work are fully reserved
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////

namespace Aristocrat.Monaco.Test.Common
{
    #region Using

    using System.Dynamic;
    using System.Reflection;
    using Test.Common.UnitTesting;

    #endregion

    /// <summary>
    ///     Definition of the DynamicPrivateObject class
    ///     This class allows unit tests to access protected/private class members and methods
    /// </summary>
    public class DynamicPrivateObject : DynamicObject
    {
        private readonly PrivateObject _privateObject;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DynamicPrivateObject" /> class.
        /// </summary>
        /// <param name="testClass">An instance of the class to be tested.</param>
        public DynamicPrivateObject(object testClass)
        {
            _privateObject = new PrivateObject(testClass);
        }

        /// <summary>
        ///     Gets the target type.
        /// </summary>
        public object Target => _privateObject.Target;

        /// <summary>
        ///     Provides the implementation for operations that set member values. Classes derived from the
        ///     <see cref="T:System.Dynamic.DynamicObject" /> class can override this method to specify dynamic behavior for
        ///     operations such as setting a value for a property.
        /// </summary>
        /// <param name="binder">
        ///     Provides information about the object that called the dynamic operation. The binder.Name property
        ///     provides the name of the member to which the value is being assigned. For example, for the statement
        ///     sampleObject.SampleProperty = "Test", where sampleObject is an instance of the class derived from the
        ///     <see cref="T:System.Dynamic.DynamicObject" /> class, binder.Name returns "SampleProperty". The binder.IgnoreCase
        ///     property specifies whether the member name is case-sensitive.
        /// </param>
        /// <param name="value">
        ///     The value to set to the member. For example, for sampleObject.SampleProperty = "Test", where
        ///     sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject" /> class, the
        ///     <paramref name="value" /> is "Test".
        /// </param>
        /// <returns>
        ///     true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the
        ///     language determines the behavior. (In most cases, a language-specific run-time exception is thrown.)
        /// </returns>
        /// <remarks>You don't call this method directly, the .Net framework will call this for you.</remarks>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _privateObject.SetFieldOrProperty(
                binder.Name,
                BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                value);
            return true;
        }

        /// <summary>
        ///     Provides the implementation for operations that get member values. Classes derived from the
        ///     <see cref="T:System.Dynamic.DynamicObject" /> class can override this method to specify dynamic behavior for
        ///     operations such as getting a value for a property.
        /// </summary>
        /// <param name="binder">
        ///     Provides information about the object that called the dynamic operation. The binder.Name property
        ///     provides the name of the member on which the dynamic operation is performed. For example, for the
        ///     Console.WriteLine(sampleObject.SampleProperty) statement, where sampleObject is an instance of the class derived
        ///     from
        ///     the <see cref="T:System.Dynamic.DynamicObject" /> class, binder.Name returns "SampleProperty". The
        ///     binder.IgnoreCase
        ///     property specifies whether the member name is case-sensitive.
        /// </param>
        /// <param name="result">
        ///     The result of the get operation. For example, if the method is called for a property, you can
        ///     assign the property value to <paramref name="result" />.
        /// </param>
        /// <returns>
        ///     true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the
        ///     language determines the behavior. (In most cases, a run-time exception is thrown.)
        /// </returns>
        /// <remarks>You don't call this method directly, the .Net framework will call this for you.</remarks>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = _privateObject.GetFieldOrProperty(
                binder.Name,
                BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            return true;
        }

        /// <summary>
        ///     Provides the implementation for operations that invoke a member. Classes derived from the
        ///     <see cref="T:System.Dynamic.DynamicObject" /> class can override this method to specify dynamic behavior for
        ///     operations such as calling a method.
        /// </summary>
        /// <param name="binder">
        ///     Provides information about the dynamic operation. The binder.Name property provides the name of
        ///     the member on which the dynamic operation is performed. For example, for the statement
        ///     sampleObject.SampleMethod(100), where sampleObject is an instance of the class derived from the
        ///     <see cref="T:System.Dynamic.DynamicObject" /> class, binder.Name returns "SampleMethod". The binder.IgnoreCase
        ///     property specifies whether the member name is case-sensitive.
        /// </param>
        /// <param name="args">
        ///     The arguments that are passed to the object member during the invoke operation. For example, for the
        ///     statement sampleObject.SampleMethod(100), where sampleObject is derived from the
        ///     <see cref="T:System.Dynamic.DynamicObject" /> class, <paramref name="args[0]" /> is equal to 100.
        /// </param>
        /// <param name="result">The result of the member invocation.</param>
        /// <returns>
        ///     true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the
        ///     language determines the behavior. (In most cases, a language-specific run-time exception is thrown.)
        /// </returns>
        /// <remarks>You don't call this method directly, the .Net framework will call this for you.</remarks>
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = _privateObject.Invoke(
                binder.Name,
                BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                args);
            return true;
        }
    }
}