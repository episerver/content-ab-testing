using EPiServer.ServiceLocation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System;
using System.Configuration;

namespace EPiServer.Marketing.Testing.Web.Controllers
{
    /// <summary>
    /// AuthorizeAttribute class that allows inserting addtional roles from a key in the app settings.
    /// </summary>
    public class AppSettingsAuthorizeAttribute : AuthorizeAttribute
    {
        /// <summary>
        ///  Gets or sets the user roles that are authorized to access the controller or action method.
        /// </summary>
        public new string Roles
        {
            get { return base.Roles ?? String.Empty; }
            set
            {
                var sRoles = ServiceLocator.Current.GetInstance<IConfiguration>()["EPiServer:Marketing:Testing:Roles"]?.ToString();
                if (!String.IsNullOrWhiteSpace(sRoles))
                {
                    base.Roles = value + ',' + sRoles;
                }
                else
                {
                    base.Roles = value;
                }
            }
        }
    }
}
