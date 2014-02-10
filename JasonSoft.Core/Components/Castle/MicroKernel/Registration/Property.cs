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

namespace JasonSoft.Components.Castle.MicroKernel.Registration
{
    public class Property
    {
        private readonly String key;
        private readonly object value;

        internal Property(String key, Object value)
        {
            this.key = key;
            this.value = value;
        }

        public string Key
        {
            get { return key; }
        }

        public object Value
        {
            get { return value; }
        }

        public static PropertyKey WithKey(String key)
        {
            return new PropertyKey(key);
        }
    }

    public class PropertyKey
    {
        private readonly String name;

        internal PropertyKey(String name)
        {
            this.name = name;
        }

        public string Name
        {
            get { return name; }
        }

        public Property Eq(Object value)
        {
            return new Property(name, value);
        }
    }
}