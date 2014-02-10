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
using System.IO;
using System.Text;

namespace JasonSoft.Components.Castle.Core.Resource
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class AbstractStreamResource : AbstractResource
    {
        public delegate Stream StreamFactory();

        /// <summary>
        /// This returns a new stream instance each time it is called.
        /// It is the responsability of the caller to dispose of this stream
        /// </summary>
        protected StreamFactory CreateStream;

        ~AbstractStreamResource()
        {
            Dispose(false);
        }

        public override TextReader GetStreamReader()
        {
            return new StreamReader(CreateStream());
        }

        public override TextReader GetStreamReader(Encoding encoding)
        {
            return new StreamReader(CreateStream(), encoding);
        }

        public override void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
        }
    }
}