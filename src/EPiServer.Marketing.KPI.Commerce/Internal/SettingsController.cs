using EPiServer.Authorization;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Marketing.KPI.Commerce.Config;
using EPiServer.Marketing.KPI.Commerce.ViewModels;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Web.Mvc;
using Mediachase.Commerce.Markets;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace EPiServer.Marketing.KPI.Commerce.Internal
{
    class SettingsController : Controller
    {
        private readonly Injected<IMarketService> _marketService;
        private readonly Injected<LocalizationService> _localizationService;
        private readonly Injected<ICommerceKpiConfig> _commerceKpiConfig;

        public SettingsController()
        {
        }

        public ActionResult Index() => View();

        [HttpGet]
        public ActionResult Get()
        {
            // Security validation: user should have Edit access to view this page
            if (!User.IsInRole(Roles.CmsAdmins))
            {
                throw new AccessDeniedException();
            }

            var model = new SettingsViewModel
            {
                MarketList = GetMarketOptions(),
                PreferredMarket = _commerceKpiConfig.Service.PreferredMarket,
                KpiCommerceConfigTitle = _localizationService.Service.GetString("/commercekpi/admin/displayname"),
                PreferredMarketDescription = _localizationService.Service.GetString("/commercekpi/admin/description"),
                PreferredMarketLabel = _localizationService.Service.GetString("/commercekpi/admin/financialculturepreference"),
                KpiCommerceSaveButton = _localizationService.Service.GetString("/commercekpi/admin/save"),
            };
            return this.JsonData(model);
        }

        [HttpPost]
        public ActionResult Save([FromBody] SettingsRequest request)
        {
            try
            {
                CommerceKpiSettings.Current.PreferredMarket = _marketService.Service.GetMarket(request.PreferredMarket);

                CommerceKpiSettings.Current.Save();

                return Ok(_localizationService.Service.GetString("/abtesting/admin/success"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public IEnumerable<MarketOption> GetMarketOptions()
        {
            var MarketList = new List<MarketOption>();

            var availableMarkets = _marketService.Service.GetAllMarkets();

            foreach (var market in availableMarkets)
            {
                MarketList.Add(new MarketOption 
                { 
                    Name = market.MarketName, 
                    IdValue = market.MarketId.Value
                });
            }
            return MarketList;
        }

        public class SettingsRequest
        {
            public string PreferredMarket { get; set; }
        }
}
}
