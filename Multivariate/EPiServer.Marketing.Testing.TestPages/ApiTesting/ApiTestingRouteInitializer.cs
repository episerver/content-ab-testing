﻿using System.Web.Mvc;
using System.Web.Routing;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;

namespace EPiServer.Marketing.Testing.TestPages.ApiTesting
{
    [InitializableModule]
    public class ApiTestingRouteInitializer : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {

            MapMultivariateTestRoute(RouteTable.Routes);

        }

        private static void MapMultivariateTestRoute(RouteCollection routes)
        {
            routes.MapRoute(name: "AB API Testing",
               url: "ApiTesting/{action}/{state}",
               defaults: new { controller = "ApiTesting", action = "Index", state = UrlParameter.Optional });

        }

        public void Uninitialize(InitializationEngine context)
        {
            throw new System.NotImplementedException();
        }
    }
}