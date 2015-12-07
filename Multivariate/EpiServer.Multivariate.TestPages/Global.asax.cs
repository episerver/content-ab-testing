﻿using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace EpiServer.Multivariate.TestPages
{
    public class EPiServerApplication : EPiServer.Global
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            
            //Tip: Want to call the EPiServer API on startup? Add an initialization module instead (Add -> New Item.. -> EPiServer -> Initialization Module)
        }
        protected override void RegisterRoutes(RouteCollection routes)
        {

            base.RegisterRoutes(routes);



            routes.MapRoute(name: "AB API Testing",
               url: "{controller}/{action}/{state}",
               defaults: new { controller = "MultivariateApiTesting", action = "Index", state = UrlParameter.Optional });

            
        }
    }
}