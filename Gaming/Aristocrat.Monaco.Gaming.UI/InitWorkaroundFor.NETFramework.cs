//
//  Workaround to enable init; language feature in .NETFramework.
//  Please delete this file after migration to .NET 6, where this
//  feature will be natively supported. See link below for more info.
// 
//  https://stackoverflow.com/questions/64749385/predefined-type-system-runtime-compilerservices-isexternalinit-is-not-defined
//

// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit {}
}