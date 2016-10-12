﻿using System;
using System.Linq;
using System.Reflection;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Framework.Localization;
using EPiServer.Framework.Localization.XmlResources;
using EPiServer.Marketing.KPI.Manager.DataClass;

namespace EPiServer.Marketing.KPI.Initializers
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class KpiLanguageProvider : IInitializableModule
    {
        private const string ProviderName = "KpiCustomLanguageProvider";
        public void Initialize(InitializationEngine context)
        {
            ProviderBasedLocalizationService localizationService = context.Locate.Advanced.GetInstance<LocalizationService>() as ProviderBasedLocalizationService;
            if (localizationService != null)
            {
                EmbeddedXmlLocalizationProviderInitializer localizationProviderInitializer =
                    new EmbeddedXmlLocalizationProviderInitializer();

                // Gets embedded xml resources from the given assembly creating a new xml provider
                Assembly kpiAssembly = Assembly.GetAssembly(typeof(IKpi));
                XmlLocalizationProvider xmlProvider  = localizationProviderInitializer.GetInitializedProvider(ProviderName,kpiAssembly);
                
                //Inserts the provider first in the provider list so that it is prioritized over default providers.
                localizationService.Providers.Insert(0, xmlProvider);
            }
        }

        public void Uninitialize(InitializationEngine context)
        {
            ProviderBasedLocalizationService localizationService = context.Locate.Advanced.GetInstance<LocalizationService>() as ProviderBasedLocalizationService;
            if (localizationService != null)
            {
                //Gets any provider that has the same name as the one initialized.
                LocalizationProvider localizationProvider = localizationService.Providers.FirstOrDefault(p => p.Name.Equals(ProviderName, StringComparison.Ordinal));
                if (localizationProvider != null)
                {
                    //If found, remove it.
                    localizationService.Providers.Remove(localizationProvider);
                }
            }
        }
    }
}
