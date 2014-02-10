// Copyright 2004-2008 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections;
using JasonSoft.Components.Castle.Core;
using JasonSoft.Components.Castle.MicroKernel.Util;

namespace JasonSoft.Components.Castle.MicroKernel.Registration
{
    public class ServiceOverrideDescriptor<S, T> : AbstractPropertyDescriptor<S, T>
    {
        public ServiceOverrideDescriptor(params ServiceOverride[] overrides)
            : base(overrides)
        {
        }

        public ServiceOverrideDescriptor(IDictionary dictionary)
            : base(dictionary)
        {
        }

        public ServiceOverrideDescriptor(object overridesAsAnonymousType)
            : base(new ReflectionBasedDictionaryAdapter(overridesAsAnonymousType))
        {
        }

        protected override void ApplyProperty(ComponentModel model,
                                              String key, Object value)
        {
            String reference = FormattedReferenceExpression(value.ToString());
            Registration.AddParameter(model, key, reference);
        }

        private static String FormattedReferenceExpression(String value)
        {
            if (!ReferenceExpressionUtil.IsReference(value))
            {
                value = String.Format("${{{0}}}", value);
            }
            return value;
        }
    }
}