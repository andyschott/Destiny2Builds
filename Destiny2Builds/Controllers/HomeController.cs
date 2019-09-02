using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Destiny2Builds.Models;
using Microsoft.Extensions.Logging;

namespace Destiny2Builds.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger _logger;
        
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            _logger.LogInformation("Index");

            if(User.Identity.IsAuthenticated)
            {
                _logger.LogInformation("User is already authenticated. Redirecting to Accounts Index");

                var url = Url.RouteUrl("AccountIndex");
                return Redirect(url);
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
