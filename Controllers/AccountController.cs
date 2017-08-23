using System.Collections.Generic;
using System.Threading.Tasks;
using facebook_demo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace facebook_demo.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly ILogger logger;
        private readonly AuthManager authManager;
        private readonly IConfiguration configuration;

        public AccountController(ILogger<AccountController> logger, IConfiguration configuration, AuthManager authManager)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.authManager = authManager;
        }

        [TempData]
        public string ErrorMessage { get; set; }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login()
        {
            await authManager.SignOut(HttpContext);

            var facebookClient = new FacebookClient();

            string returnUrl = GetReturnUrl();

            var url = facebookClient.GetLoginUrl(
                configuration["Authentication:Facebook:AppId"], 
                returnUrl, 
                new List<string>() { "manage_pages", "publish_pages", "read_insights", "publish_actions", "user_actions.video" });

            return Redirect(url);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> FacebookCallback(string code)
        {
            var facebookService = new FacebookService(new FacebookClient(), configuration);

            var userToken = await facebookService.GetToken(code, GetReturnUrl());

            var account = await facebookService.GetAccountAsync(userToken);

            var pageId = configuration["PageId"];

            var pageToken = await facebookService.GetPageToken(userToken, pageId);
            var appToken = await facebookService.GetAppToken(userToken, pageId);

            var userName = account.FirstName + " " + account.LastName;

            await authManager.SignIn(HttpContext, userName, pageToken, userToken, appToken);

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await authManager.SignOut(HttpContext);
            logger.LogInformation("User logged out.");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private string GetReturnUrl()
        {
            return configuration["BaseUrl"] + "Account/FacebookCallback";
        }

       
    }
}