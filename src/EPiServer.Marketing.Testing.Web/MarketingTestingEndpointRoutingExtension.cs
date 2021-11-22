using EPiServer.Web.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Testing.Web
{
    public class MarketingTestingEndpointRoutingExtension : IEndpointRoutingExtension
    {
        public void MapEndpoints(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute(
                name: "EPiServerContentOptimization",
                pattern: "api/episerver/Testing/{action}",
                defaults: new { controller = "Testing", action = "GetAllTests" });
        }
    }
}
