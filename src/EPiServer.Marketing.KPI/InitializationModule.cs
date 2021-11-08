using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Marketing.KPI.Dal;
using EPiServer.ServiceLocation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Marketing.KPI
{
    [InitializableModule]
    [ModuleDependency(typeof(Shell.UI.InitializationModule))]
    [ModuleDependency(typeof(Web.InitializationModule))]
    public partial class InitializationModule : IConfigurableModule //netcore , IInitializableHttpModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            context.Services.AddDbContext<DatabaseContext>(
                options => options.UseSqlServer(ServiceLocator.Current.GetInstance<IConfiguration>().GetConnectionString("EPiServerDB")));
        }

        public void Initialize(InitializationEngine context)
        {
            throw new NotImplementedException();
        }

        public void Uninitialize(InitializationEngine context)
        {
            throw new NotImplementedException();
        }
    }
}
