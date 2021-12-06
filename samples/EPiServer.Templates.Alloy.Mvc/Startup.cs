using AlloyMvcTemplates.Extensions;
using AlloyMvcTemplates.Infrastructure;
using EPiServer.Cms.UI.AspNetIdentity;
using EPiServer.Data;
using EPiServer.DependencyInjection;
using EPiServer.Framework.Web.Resources;
using EPiServer.Web.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using EPiServer.Scheduler;
using EPiServer.Cms.TinyMce;
using EPiServer.Web.Mvc.Html;
using EPiServer.Cms.Shell;
using EPiServer.Cms.UI.Admin;
using EPiServer.Cms.UI.VisitorGroups;
using System;
using EPiServer.Framework.Hosting;
using EPiServer.Web.Hosting;

namespace EPiServer.Templates.Alloy.Mvc
{
    public class Startup
    {
        private readonly IWebHostEnvironment _webHostingEnvironment;
        private readonly IConfiguration _configuration;

        public Startup(IWebHostEnvironment webHostingEnvironment, IConfiguration configuration)
        {
            _webHostingEnvironment = webHostingEnvironment;
            _configuration = configuration;

            string path = Path.Combine(Directory.GetCurrentDirectory(), "App_Data");
            AppDomain.CurrentDomain.SetData("DataDirectory", path);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var dbPath = Path.Combine(_webHostingEnvironment.ContentRootPath, "App_Data\\Alloy.mdf");
            var commDbPath = Path.Combine(_webHostingEnvironment.ContentRootPath, "App_Data\\AlloyCommerce.mdf");
            var connectionstring = _configuration.GetConnectionString("EPiServerDB") ?? $"Data Source=(LocalDb)\\MSSQLLocalDB;AttachDbFilename={dbPath};Initial Catalog=mt_alloy_mvc_netcore;Integrated Security=True;Connect Timeout=30;MultipleActiveResultSets=True";
            var commconnectionstring = _configuration.GetConnectionString("EcfSqlConnection") ?? $"Data Source=(LocalDb)\\MSSQLLocalDB;AttachDbFilename={commDbPath};Initial Catalog=mt_alloy_commerce_netcore;Connection Timeout=60;Integrated Security=True;MultipleActiveResultSets=True";

            services.Configure<SchedulerOptions>(o =>
            {
                o.Enabled = false;
            });

            services.Configure<DataAccessOptions>(o =>
            {
                o.ConnectionStrings.Add(new ConnectionStringOptions
                {
                    ConnectionString = connectionstring,
                    Name = "EPiServerDB"
                });
                o.ConnectionStrings.Add(new ConnectionStringOptions
                {
                    ConnectionString = commconnectionstring,
                    Name = "EcfSqlConnection"
                });
            });

            services.AddCmsAspNetIdentity<ApplicationUser>();


            if (_webHostingEnvironment.IsDevelopment())
            {
                services.AddUIMappedFileProviders(_webHostingEnvironment.ContentRootPath, @"..\..\");
                services.Configure<ClientResourceOptions>(uiOptions =>
                {
                    uiOptions.Debug = true;
                });
            }

            services.AddMvc();
            services.AddAlloy();
            services.AddCms()
                .AddCommerce();

            services.AddEmbeddedLocalization<Startup>();
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMiddleware<AdministratorRegistrationPageMiddleware>();
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapContent();
                endpoints.MapControllerRoute("Register", "/Register", new { controller = "Register", action = "Index" });
                endpoints.MapRazorPages();
            });
        }
    }

    internal static class IntenalServiceCollectionExtensions
    {
        public static IServiceCollection AddUIMappedFileProviders(this IServiceCollection services, string applicationRootPath, string uiSolutionRelativePath)
        {
            services.Configure<ClientResourceOptions>(o => o.Debug = true);

            var uiSolutionFolder = Path.Combine(applicationRootPath, uiSolutionRelativePath);
            EnsureDictionary(new DirectoryInfo(Path.Combine(applicationRootPath, "modules/_protected")));
            services.Configure<CompositeFileProviderOptions>(c =>
            {
                c.BasePathFileProviders.Add(new MappingPhysicalFileProvider("/EPiServer/EPiServer.Marketing.KPI.Commerce", string.Empty, Path.Combine(uiSolutionFolder, @"src\EPiServer.Marketing.KPI.Commerce")));
                c.BasePathFileProviders.Add(new MappingPhysicalFileProvider("/EPiServer/EPiServer.Marketing.KPI", string.Empty, Path.Combine(uiSolutionFolder, @"src\EPiServer.Marketing.KPI")));
            });
            return services;
        }

        private static void EnsureDictionary(DirectoryInfo directoryInfo)
        {
            if (!directoryInfo.Parent.Exists)
            {
                EnsureDictionary(directoryInfo.Parent);
            }

            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }
        }
    }
}
