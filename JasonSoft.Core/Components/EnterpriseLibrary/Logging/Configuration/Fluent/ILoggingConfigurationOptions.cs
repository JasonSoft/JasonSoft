﻿//===============================================================================
// Microsoft patterns & practices Enterprise Library
// Logging Application Block
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JasonSoft.Components.EnterpriseLibrary.Logging.Configuration;
using System.Diagnostics;
using System.Messaging;
using JasonSoft.Components.EnterpriseLibrary.Logging.TraceListeners;
using System.Collections.Specialized;
using JasonSoft.Components.EnterpriseLibrary.Logging.Formatters;

namespace JasonSoft.Components.EnterpriseLibrary.Common.Configuration.Fluent
{

    /// <summary>
    /// Fluent interface that allows global logging settings to be configured.
    /// </summary>
    public interface ILoggingConfigurationOptions : ILoggingConfigurationContd, IFluentInterface
    {
        /// <summary>
        /// Disables tracing in the configuration schema. <br/>
        /// Tracing is enabled by deault.
        /// </summary>
        ILoggingConfigurationOptions DisableTracing();

        /// <summary>
        /// Disables temporarily reverting impersonation when logging. <br/>
        /// Impersonation is temporarily reverted by default.
        /// </summary>
        ILoggingConfigurationOptions DoNotRevertImpersonation();

        /// <summary>
        /// Specifies that no warnings should be logged for non-existant categories. <br/>
        /// Warnings are logged for non-existant categories by default.
        /// </summary>
        ILoggingConfigurationOptions DoNotLogWarningsWhenNoCategoryExists();
    }
}