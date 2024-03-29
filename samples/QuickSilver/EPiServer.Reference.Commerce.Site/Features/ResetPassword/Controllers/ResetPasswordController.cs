﻿using EPiServer.Cms.UI.AspNetIdentity;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Site.Features.Login.Services;
using EPiServer.Reference.Commerce.Site.Features.ResetPassword.Pages;
using EPiServer.Reference.Commerce.Site.Features.ResetPassword.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Shared.Controllers;
using EPiServer.Reference.Commerce.Site.Features.Shared.Pages;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace EPiServer.Reference.Commerce.Site.Features.ResetPassword.Controllers
{
    public class ResetPasswordController : IdentityControllerBase<ResetPasswordPage>
    {
        private readonly IContentLoader _contentLoader;
        private readonly IMailService _mailService;
        private readonly LocalizationService _localizationService;

        public ResetPasswordController(ApplicationSignInManager<ApplicationUser> signinManager,
            ApplicationUserManager<ApplicationUser> userManager,
            UserService userService,
            IContentLoader contentLoader,
            IMailService mailService,
            LocalizationService localizationService)

            : base(signinManager, userManager, userService)
        {
            _contentLoader = contentLoader;
            _mailService = mailService;
            _localizationService = localizationService;
        }

        [AllowAnonymous]
        public ActionResult Index(ResetPasswordPage currentPage)
        {
            ForgotPasswordViewModel viewModel = new ForgotPasswordViewModel() { CurrentPage = currentPage };
            return View("ForgotPassword", viewModel);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model, string language)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist or is not confirmed
                return View("ForgotPasswordConfirmation");
            }

            var startPage = _contentLoader.Get<StartPage>(ContentReference.StartPage);
            var body = _mailService.GetHtmlBodyForMail(startPage.ResetPasswordMail, new NameValueCollection(), language);
            var mailPage = _contentLoader.Get<MailBasePage>(startPage.ResetPasswordMail);
            var code = await UserManager.GeneratePasswordResetTokenAsync(user);
            var url = Url.Action("ResetPassword", "ResetPassword", new { userId = user.Id, code = HttpUtility.UrlEncode(code), language = language }, Request.Scheme);

            body = body.Replace("[MailUrl]",
                String.Format("{0}<a href=\"{1}\">{2}</a>",
                    _localizationService.GetString("/ResetPassword/Mail/Text"),
                    url,
                    _localizationService.GetString("/ResetPassword/Mail/Link")));

            _mailService.Send(user.Id, mailPage.MailTitle, body);

            return RedirectToAction("ForgotPasswordConfirmation");
        }

        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View("ForgotPasswordConfirmation");
        }

        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            var viewModel = new ResetPasswordViewModel { Code = code };
            return code == null ? View("Error") : View("ResetPassword", viewModel);
        }

        [HttpPost]
        [AllowDBWrite]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation");
            }

            var result = await UserManager.ResetPasswordAsync(user, HttpUtility.UrlDecode(model.Code), model.Password);

            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }

            AddErrors(result.Errors.Select(x=> x.Description));

            return View();
        }

        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View("ResetPasswordConfirmation");
        }
    }
}