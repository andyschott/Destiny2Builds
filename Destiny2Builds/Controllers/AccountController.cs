using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Destiny2;
using Destiny2Builds.Helpers;
using Destiny2Builds.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Destiny2Builds.Controllers
{
    [Route("[controller]")]
    public class AccountController : Controller
    {
        private readonly IDestiny2 _destiny2;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly BungieSettings _bungie;
        private readonly ILogger _logger;

        public AccountController(IDestiny2 destiny2, IHttpContextAccessor contextAccessor,
            IOptions<BungieSettings> bungie, ILogger<AccountController> logger)
        {
            _destiny2 = destiny2;
            _contextAccessor = contextAccessor;
            _bungie = bungie.Value;
            _logger = logger;
        }
        
        [HttpGet("login")]
        public IActionResult Login(string returnUrl = "/")
        {
            _logger.LogInformation("Login");
            return Challenge(new AuthenticationProperties() { RedirectUri = returnUrl });
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            _logger.LogInformation("Logut");
            Response.Cookies.Delete(_bungie.LoginCookieName);

            var url = Url.Action("Index", "Home");
            return Redirect(url);
        }

        [HttpGet(Name = "AccountIndex")]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Index");
            var accessToken = _contextAccessor.HttpContext.GetTokenAsync("access_token");

            var value = User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            long.TryParse(value, out long membershipId);

            var linkedProfiles = await _destiny2.GetLinkedProfiles(await accessToken, membershipId);

            var accounts = linkedProfiles.Profiles.Select(profile => new Account(profile.MembershipType, profile.MembershipId))
                                                    .ToList();

            if (1 == accounts.Count)
            {
                _logger.LogInformation("Only one account - redirecting to account page");

                // If there is only one account, redirect to the page for it.
                var url = Url.RouteUrl("CharacterIndex", new
                {
                    type = (int)accounts[0].Type,
                    id = accounts[0].Id
                });
                return Redirect(url);
            }

            var model = new AccountsViewModel();
            model.Accounts = accounts;
            return View(model);
        }
    }
}