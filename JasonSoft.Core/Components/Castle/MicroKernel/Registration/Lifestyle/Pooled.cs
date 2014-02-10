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

using JasonSoft.Components.Castle.Core;
using JasonSoft.Components.Castle.Core.Configuration;

namespace JasonSoft.Components.Castle.MicroKernel.Registration.Lifestyle
{
    public class Pooled<S, T> : LifestyleDescriptor<S, T>
    {
        private readonly int? initialSize;
        private readonly int? maxSize;

        public Pooled()
            : base(LifestyleType.Pooled)
        {
        }

        public Pooled(int initialSize, int maxSize)
            : this()
        {
            this.initialSize = initialSize;
            this.maxSize = maxSize;
        }

        protected override void ApplyLifestyleDetails(IConfiguration configuration)
        {
            if (initialSize.HasValue)
            {
                configuration.Attributes["initialPoolSize"] = initialSize.ToString();
            }

            if (maxSize.HasValue)
            {
                configuration.Attributes["maxPoolSize"] = maxSize.ToString();
            }
        }
    }
}