// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable IDE0161 // Convert to file-scoped namespace - not supported in netstandard2.0/net472

#if !NET
using System.ComponentModel;

namespace System.Runtime.CompilerServices
{
    /// <summary>Specifies that a type has required members or that a member is required.</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal sealed class RequiredMemberAttribute : Attribute;
}
#endif