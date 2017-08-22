using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using facebook_demo.Models;

namespace facebook_demo.Controllers
{

    //App Assignment:  Create an application for the management of a Facebook Page.
    //The app will be able to create regular posts to a Facebook Page as well as Unpublished Page Posts.
    //The app will be able to list posts from a page (both published and unpublished), 
    //and show the number of people who have viewed each post.

    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}