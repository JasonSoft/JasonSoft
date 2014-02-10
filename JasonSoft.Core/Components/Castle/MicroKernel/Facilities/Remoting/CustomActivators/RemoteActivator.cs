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
using System.Runtime.Remoting;
using JasonSoft.Components.Castle.Core;
using JasonSoft.Components.Castle.MicroKernel;
using JasonSoft.Components.Castle.MicroKernel.ComponentActivator;

namespace JasonSoft.Components.Castle.Facilities.Remoting
{
    /// <summary>
    /// Activates a object connecting to the remote server.
    /// </summary>
    public class RemoteActivator : DefaultComponentActivator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteActivator"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="kernel">The kernel.</param>
        /// <param name="onCreation">The oncreation event handler.</param>
        /// <param name="onDestruction">The ondestruction event handler.</param>
        public RemoteActivator(ComponentModel model, IKernel kernel, ComponentInstanceDelegate onCreation,
                               ComponentInstanceDelegate onDestruction) : base(model, kernel, onCreation, onDestruction)
        {
        }

        protected override object Instantiate(CreationContext context)
        {
            String url = (String) Model.ExtendedProperties["remoting.uri"];

            // return Activator.GetObject(Model.Service, url);

            return RemotingServices.Connect(Model.Service, url);
        }
    }
}