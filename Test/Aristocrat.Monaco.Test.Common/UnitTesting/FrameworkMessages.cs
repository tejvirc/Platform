namespace Aristocrat.Monaco.Test.Common.UnitTesting
{
    internal class FrameworkMessages
    {
        public const string PrivateAccessorConstructorNotFound = @"
      The constructor with the specified signature could not be found.You might need to regenerate your private accessor,
      or the member may be private and defined on a base class. If the latter is true, you need to pass the type
      that defines the member into PrivateObject's constructor.
    ";

        public const string PrivateAccessorMemberNotFound = @"
      The member specified ({0}) could not be found. You might need to regenerate your private accessor,
      or the member may be private and defined on a base class. If the latter is true, you need to pass the type
      that defines the member into PrivateObject's constructor.
    ";

        public const string AccessStringInvalidSyntax = "Access string has invalid syntax.";
    }
}
