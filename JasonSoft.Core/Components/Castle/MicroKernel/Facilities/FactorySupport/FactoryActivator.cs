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
using System.Reflection;
using JasonSoft.Components.Castle.Core;
using JasonSoft.Components.Castle.MicroKernel;
using JasonSoft.Components.Castle.MicroKernel.ComponentActivator;
using JasonSoft.Components.Castle.MicroKernel.Facilities;
using JasonSoft.Components.Castle.MicroKernel.Proxy;
using JasonSoft.Components.Castle.MicroKernel.SubSystems.Conversion;

namespace JasonSoft.Components.Castle.Facilities.FactorySupport
{
    /// <summary>
    /// 
    /// </summary>
    public class FactoryActivator : DefaultComponentActivator
    {
        public FactoryActivator(ComponentModel model, IKernel kernel, ComponentInstanceDelegate onCreation,
                                ComponentInstanceDelegate onDestruction)
            : base(model, kernel, onCreation, onDestruction)
        {
        }

        protected override object Instantiate(CreationContext context)
        {
            String factoryId = (String) Model.ExtendedProperties["factoryId"];
            String factoryCreate = (String) Model.ExtendedProperties["factoryCreate"];

            if (!Kernel.HasComponent(factoryId))
            {
                String message = String.Format("You have specified a factory ('{2}') " +
                                               "for the component '{0}' {1} but the kernel does not have this " +
                                               "factory registered",
                                               Model.Name, Model.Implementation.FullName, factoryId);
                throw new FacilityException(message);
            }

            IHandler factoryHandler = Kernel.GetHandler(factoryId);

            // Let's find out whether the create method is a static or instance method

            Type factoryType = factoryHandler.ComponentModel.Implementation;

            MethodInfo staticCreateMethod =
                factoryType.GetMethod(factoryCreate,
                                      BindingFlags.Public | BindingFlags.Static);

            if (staticCreateMethod != null)
            {
                return Create(null, factoryId, staticCreateMethod, factoryCreate, context);
            }
            else
            {
                object factoryInstance = Kernel[factoryId];

                MethodInfo instanceCreateMethod =
                    factoryInstance.GetType().GetMethod(factoryCreate,
                                                        BindingFlags.Public | BindingFlags.Instance);

                if (instanceCreateMethod == null)
                {
                    factoryInstance = ProxyUtil.GetUnproxiedInstance(factoryInstance);

                    instanceCreateMethod =
                        factoryInstance.GetType().GetMethod(factoryCreate,
                                                            BindingFlags.Public | BindingFlags.Instance);
                }

                if (instanceCreateMethod != null)
                {
                    return Create(factoryInstance, factoryId, instanceCreateMethod, factoryCreate, context);
                }
                else
                {
                    String message = String.Format("You have specified a factory " +
                                                   "('{2}' - method to be called: {3}) " +
                                                   "for the component '{0}' {1} but we couldn't find the creation method" +
                                                   "(neither instance or static method with the name '{3}')",
                                                   Model.Name, Model.Implementation.FullName, factoryId, factoryCreate);
                    throw new FacilityException(message);
                }
            }
        }

        private object Create(object factoryInstance, string factoryId, MethodInfo instanceCreateMethod,
                              string factoryCreate, CreationContext context)
        {
            IConversionManager converter = (IConversionManager)
                                           Kernel.GetSubSystem(SubSystemConstants.ConversionManagerKey);

            try
            {
                ParameterInfo[] parameters = instanceCreateMethod.GetParameters();

                ArrayList methodArgs = new ArrayList();

                foreach (ParameterInfo parameter in parameters)
                {
                    Type paramType = parameter.ParameterType;

                    DependencyModel depModel;

                    if (converter.IsSupportedAndPrimitiveType(paramType))
                    {
                        depModel = new DependencyModel(DependencyType.Parameter, parameter.Name, paramType, false);
                    }
                    else
                    {
                        depModel = new DependencyModel(DependencyType.Service, parameter.Name, paramType, false);
                    }

                    if (!Kernel.Resolver.CanResolve(context, null, Model, depModel))
                    {
                        String message = String.Format(
                            "Factory Method {0}.{1} requires an argument '{2}' that could not be resolved",
                            instanceCreateMethod.DeclaringType.FullName, instanceCreateMethod.Name, parameter.Name);
                        throw new FacilityException(message);
                    }

                    object arg = Kernel.Resolver.Resolve(context, null, Model, depModel);

                    methodArgs.Add(arg);
                }

                return instanceCreateMethod.Invoke(factoryInstance, methodArgs.ToArray());
            }
            catch (Exception ex)
            {
                String message = String.Format("You have specified a factory " +
                                               "('{2}' - method to be called: {3}) " +
                                               "for the component '{0}' {1} that failed during invoke.",
                                               Model.Name, Model.Implementation.FullName, factoryId, factoryCreate);

                throw new FacilityException(message, ex);
            }
        }
    }
}