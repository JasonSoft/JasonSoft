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
using JasonSoft.Components.EnterpriseLibrary.Common.Configuration.Fluent;
using JasonSoft.Components.EnterpriseLibrary.Common.Properties;

namespace JasonSoft.Components.EnterpriseLibrary.Common.Configuration
{
    /// <summary/>
    public static class PriorityFilterBuilderExtensions
    {
        /// <summary/>
        public static ILoggingConfigurationFilterOnPriority FilterOnPriority(this ILoggingConfigurationOptions context, string logFilterName)
        {

            if (string.IsNullOrEmpty(logFilterName))
                throw new ArgumentException(Resources.ExceptionStringNullOrEmpty, "logFilterName");

            return new FilterOnPriorityBuilder(context, logFilterName);
        }


        private class FilterOnPriorityBuilder : LoggingConfigurationExtension, ILoggingConfigurationFilterOnPriority 
        {
            PriorityFilterData priorityFilterData;

            public FilterOnPriorityBuilder(ILoggingConfigurationOptions context, string logFilterName)
                :base(context)
            {
                priorityFilterData = new PriorityFilterData()
                {
                    Name = logFilterName
                };

                LoggingSettings.LogFilters.Add(priorityFilterData);
            }

            public ILoggingConfigurationFilterOnPriority StartingWithPriority(int minimumPriority)
            {
                priorityFilterData.MinimumPriority = minimumPriority;
                return this;
            }

            public ILoggingConfigurationFilterOnPriority UpToPriority(int maximumPriority)
            {
                priorityFilterData.MaximumPriority = maximumPriority;
                return this;
            }

        }
    }
}
