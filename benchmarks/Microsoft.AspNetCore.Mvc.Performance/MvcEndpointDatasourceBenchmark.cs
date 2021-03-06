﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.AspNetCore.Mvc.Performance
{
    public class MvcEndpointDataSourceBenchmark
    {
        private const string DefaultRoute = "{Controller=Home}/{Action=Index}/{id?}";

        private MockActionDescriptorCollectionProvider _conventionalActionProvider;
        private MockActionDescriptorCollectionProvider _attributeActionProvider;
        private List<MvcEndpointInfo> _conventionalEndpointInfos;

        [Params(1, 100, 1000)]
        public int ActionCount;

        [GlobalSetup]
        public void Setup()
        {
            _conventionalActionProvider = new MockActionDescriptorCollectionProvider(
                Enumerable.Range(0, ActionCount).Select(i => CreateActionDescriptor(i, false)).ToList()
                );

            _attributeActionProvider = new MockActionDescriptorCollectionProvider(
                Enumerable.Range(0, ActionCount).Select(i => CreateActionDescriptor(i, true)).ToList()
                );

            _conventionalEndpointInfos = new List<MvcEndpointInfo>
            {
                new MvcEndpointInfo(
                    "Default",
                    DefaultRoute,
                    new RouteValueDictionary(),
                    new Dictionary<string, object>(),
                    new RouteValueDictionary(),
                    new MockInlineConstraintResolver())
            };
        }

        [Benchmark]
        public void AttributeRouteEndpoints()
        {
            var endpointDataSource = CreateMvcEndpointDataSource(_attributeActionProvider);
            var endpoints = endpointDataSource.Endpoints;
        }

        [Benchmark]
        public void ConventionalEndpoints()
        {
            var endpointDataSource = CreateMvcEndpointDataSource(_conventionalActionProvider);
            endpointDataSource.ConventionalEndpointInfos.AddRange(_conventionalEndpointInfos);
            var endpoints = endpointDataSource.Endpoints;
        }

        private ActionDescriptor CreateActionDescriptor(int id, bool attributeRoute)
        {
            var actionDescriptor = new ActionDescriptor
            {
                RouteValues = new Dictionary<string, string>
                {
                    ["Controller"] = "Controller" + id,
                    ["Action"] = "Index"
                },
                DisplayName = "Action " + id
            };

            if (attributeRoute)
            {
                actionDescriptor.AttributeRouteInfo = new AttributeRouteInfo
                {
                    Template = DefaultRoute
                };
            }

            return actionDescriptor;
        }

        private MvcEndpointDataSource CreateMvcEndpointDataSource(
            IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
        {
            var dataSource = new MvcEndpointDataSource(
                actionDescriptorCollectionProvider,
                new MvcEndpointInvokerFactory(new ActionInvokerFactory(Array.Empty<IActionInvokerProvider>())),
                Array.Empty<IActionDescriptorChangeProvider>(),
                new MockServiceProvider());

            return dataSource;
        }

        private class MockActionDescriptorCollectionProvider : IActionDescriptorCollectionProvider
        {
            public MockActionDescriptorCollectionProvider(List<ActionDescriptor> actionDescriptors)
            {
                ActionDescriptors = new ActionDescriptorCollection(actionDescriptors, 0);
            }

            public ActionDescriptorCollection ActionDescriptors { get; }
        }

        private class MockServiceProvider : IServiceProvider
        {
            public object GetService(Type serviceType)
            {
                throw new NotImplementedException();
            }
        }

        private class MockInlineConstraintResolver : IInlineConstraintResolver
        {
            public IRouteConstraint ResolveConstraint(string inlineConstraint)
            {
                throw new NotImplementedException();
            }
        }
    }
}
