﻿//===============================================================================
// Microsoft patterns & practices
// Unity Application Block
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================

using System;
using JasonSoft.Components.EnterpriseLibrary.Unity.ObjectBuilder;
using JasonSoft.Components.EnterpriseLibrary.ObjectBuilder2;

namespace JasonSoft.Components.EnterpriseLibrary.Unity
{
    /// <summary>
    /// A <see cref="InjectionParameterValue"/> that lets you specify that
    /// an instance of a generic type parameter should be resolved.
    /// </summary>
    public class GenericParameter : GenericParameterBase
    {
        /// <summary>
        /// Create a new <see cref="GenericParameter"/> instance that specifies
        /// that the given named generic parameter should be resolved.
        /// </summary>
        /// <param name="genericParameterName">The generic parameter name to resolve.</param>
        public GenericParameter(string genericParameterName)
            : base(genericParameterName)
        { }

        /// <summary>
        /// Create a new <see cref="GenericParameter"/> instance that specifies
        /// that the given named generic parameter should be resolved.
        /// </summary>
        /// <param name="genericParameterName">The generic parameter name to resolve.</param>
        /// <param name="resolutionKey">name to use when looking up in the container.</param>
        public GenericParameter(string genericParameterName, string resolutionKey)
            : base(genericParameterName, resolutionKey)
        { }

        /// <summary>
        /// Return a <see cref="IDependencyResolverPolicy"/> instance that will
        /// return this types value for the parameter.
        /// </summary>
        /// <param name="typeToResolve">The actual type to resolve.</param>
        /// <param name="resolutionKey">The resolution key.</param>
        /// <returns>The <see cref="IDependencyResolverPolicy"/>.</returns>
        protected override IDependencyResolverPolicy DoGetResolverPolicy(Type typeToResolve, string resolutionKey)
        {
            return new NamedTypeDependencyResolverPolicy(typeToResolve, resolutionKey);
        }
    }
}
