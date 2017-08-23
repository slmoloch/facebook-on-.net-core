using System;
using System.Globalization;
using System.Threading.Tasks;
using facebook_demo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace facebook_demo.Controllers
{
    [Authorize]
    [Route("[controller]/[action]")]
    public class PageController : Controller
    {
        private IFacebookService facebookService;
        private AuthManager authManager;
        private IConfiguration configuration;

        public PageController(IFacebookService facebookService, IConfiguration configuration, AuthManager authManager)
        {
            this.facebookService = facebookService;
            this.configuration = configuration;
            this.authManager = authManager;
        }

        public async Task<ActionResult> Index()
        {
            var pageToken = authManager.GetPageToken(User);
            var pageId = configuration["PageId"];

            var posts = await facebookService.GetPagePostsAsync(pageToken, pageId);
          //  var impressions = await facebookService.GetPostImpressions(pageToken, pageId);

            return View(posts);
        }
      
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                var isPublished = checkBoxToBool(collection["Published"]);
                var publishOnTimestamp = 0;

                if (!isPublished)
                {
                    var publishOn = DateTime.ParseExact(collection["PublishOn"], "d", CultureInfo.InvariantCulture);
                    publishOnTimestamp = (Int32)(publishOn.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

                }

                var pageToken = authManager.GetPageToken(User);
                var pageId = configuration["PageId"];
                
                var postOnWallAsync = facebookService.PostOnPage(
                    pageToken, 
                    pageId, 
                    collection["Message"], 
                    isPublished,
                    publishOnTimestamp);


                Task.WaitAll(postOnWallAsync);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        bool checkBoxToBool(string cbVal)
        {

            if (string.Compare(cbVal, "false") == 0)
                return false;
            if (string.Compare(cbVal, "true,false") == 0)
                return true;
            else
                throw new ArgumentNullException(cbVal);
        }

        public ActionResult Edit(string id)
        {
            var pageToken = authManager.GetPageToken(User);

            var postOnWallAsync = facebookService.PublishPostOnPage(pageToken, id);
            Task.WaitAll(postOnWallAsync);

            return RedirectToAction(nameof(Index));
        }

        public async Task<ActionResult> PostVideo()
        {
            var appToken = authManager.GetAppToken(User);

            var objectId = await facebookService.AddVideoObject(
                appToken,
                "http://yauheni.me/1",
                "https://images-na.ssl-images-amazon.com/images/M/MV5BYjFkMTlkYWUtZWFhNy00M2FmLThiOTYtYTRiYjVlZWYxNmJkXkEyXkFqcGdeQXVyNTAyODkwOQ@@._V1_SY1000_CR0,0,666,1000_AL_.jpg",
                "My Movie app");

            var userToken = authManager.GetUserToken(User);

            await facebookService.AddWatchAction(userToken, objectId);
            await facebookService.AddLikeAction(userToken, objectId);

            return RedirectToAction(nameof(Index));
        }
    }
}