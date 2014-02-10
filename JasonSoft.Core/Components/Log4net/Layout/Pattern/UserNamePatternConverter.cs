#region Copyright & License

//
// Copyright 2001-2005 The Apache Software Foundation
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

#endregion

using System.IO;
using JasonSoft.Components.Log4net.Core;

namespace JasonSoft.Components.Log4net.Layout.Pattern
{
    /// <summary>
    /// Converter to include event user name
    /// </summary>
    /// <author>Douglas de la Torre</author>
    /// <author>Nicko Cadell</author>
    internal sealed class UserNamePatternConverter : PatternLayoutConverter
    {
        /// <summary>
        /// Convert the pattern to the rendered message
        /// </summary>
        /// <param name="writer"><see cref="TextWriter" /> that will receive the formatted result.</param>
        /// <param name="loggingEvent">the event being logged</param>
        protected override void Convert(TextWriter writer, LoggingEvent loggingEvent)
        {
            writer.Write(loggingEvent.UserName);
        }
    }
}