using EPiServer.Marketing.Testing.Web.ClientKPI;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Testing.Web
{
    /// <summary>
    /// Middleware called to insert ABT Script into HTML
    /// </summary>
    public class AppendClientKpiScriptMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IClientKpiInjector _clientKpiInjector;

        public AppendClientKpiScriptMiddleware(RequestDelegate next,
            IClientKpiInjector clientKpiInjector)
        {
            _next = next;
            _clientKpiInjector = clientKpiInjector;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var originalBody = context.Response.Body;
            var updatedStream = GenerateStreamFromString(_clientKpiInjector.AppendClientKpiScript());
            
            //Injects the specified script into the response stream.
            await updatedStream.CopyToAsync(originalBody);

            context.Response.Body = originalBody;
            await _next.Invoke(context);
        }
        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
