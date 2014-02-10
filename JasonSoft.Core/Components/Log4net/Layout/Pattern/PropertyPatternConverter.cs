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
using JasonSoft.Components.Log4net.Util;

namespace JasonSoft.Components.Log4net.Layout.Pattern
{
    /// <summary>
    /// Property pattern converter
    /// </summary>
    /// <remarks>
    /// <para>
    /// Writes out the value of a named property. The property name
    /// should be set in the <see cref="PatternConverter.Option"/>
    /// property.
    /// </para>
    /// <para>
    /// If the <see cref="PatternConverter.Option"/> is set to <c>null</c>
    /// then all the properties are written as key value pairs.
    /// </para>
    /// </remarks>
    /// <author>Nicko Cadell</author>
    internal sealed class PropertyPatternConverter : PatternLayoutConverter
    {
        /// <summary>
        /// Write the property value to the output
        /// </summary>
        /// <param name="writer"><see cref="TextWriter" /> that will receive the formatted result.</param>
        /// <param name="loggingEvent">the event being logged</param>
        /// <remarks>
        /// <para>
        /// Writes out the value of a named property. The property name
        /// should be set in the <see cref="PatternConverter.Option"/>
        /// property.
        /// </para>
        /// <para>
        /// If the <see cref="PatternConverter.Option"/> is set to <c>null</c>
        /// then all the properties are written as key value pairs.
        /// </para>
        /// </remarks>
        protected override void Convert(TextWriter writer, LoggingEvent loggingEvent)
        {
            if (Option != null)
            {
                // Write the value for the specified key
                WriteObject(writer, loggingEvent.Repository, loggingEvent.LookupProperty(Option));
            }
            else
            {
                // Write all the key value pairs
                WriteDictionary(writer, loggingEvent.Repository, loggingEvent.GetProperties());
            }
        }
    }
}