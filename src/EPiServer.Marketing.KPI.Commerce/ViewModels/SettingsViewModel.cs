using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Marketing.KPI.Commerce.ViewModels
{
    class SettingsViewModel
    {
        #region Selection options
        public IEnumerable<MarketOption> MarketList { get; set; }
        #endregion

        #region Localization
        public string KpiCommerceConfigTitle { get; set; }
        public string PreferredMarketDescription { get; set; }
        public string PreferredMarketLabel { get; set; }
        public string KpiCommerceSaveButton { get; set; }

        #endregion

        public string PreferredMarket { get; set; }
    }
}
