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

using JasonSoft.Components.Log4net.Appender;
using JasonSoft.Components.Log4net.Config;

namespace JasonSoft.Components.Log4net.Repository
{
    /// <summary>
    /// Basic Configurator interface for repositories
    /// </summary>
    /// <remarks>
    /// <para>
    /// Interface used by basic configurator to configure a <see cref="ILoggerRepository"/>
    /// with a default <see cref="IAppender"/>.
    /// </para>
    /// <para>
    /// A <see cref="ILoggerRepository"/> should implement this interface to support
    /// configuration by the <see cref="BasicConfigurator"/>.
    /// </para>
    /// </remarks>
    /// <author>Nicko Cadell</author>
    /// <author>Gert Driesen</author>
    public interface IBasicRepositoryConfigurator
    {
        /// <summary>
        /// Initialize the repository using the specified appender
        /// </summary>
        /// <param name="appender">the appender to use to log all logging events</param>
        /// <remarks>
        /// <para>
        /// Configure the repository to route all logging events to the
        /// specified appender.
        /// </para>
        /// </remarks>
        void Configure(IAppender appender);
    }
}