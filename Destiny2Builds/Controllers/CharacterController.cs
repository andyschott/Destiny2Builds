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
    [Authorize]
    public class CharacterController : Controller
    {
        private readonly IDestiny2 _destiny2;
        private readonly IManifest _manifest;
        private readonly BungieSettings _bungie;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ILogger _logger;

        public CharacterController(IDestiny2 destiny2, IManifest manifest,
            IOptions<BungieSettings> bungie, IHttpContextAccessor contextAccessor,
            ILogger<CharacterController> logger)
        {
            _destiny2 = destiny2;
            _manifest = manifest;
            _bungie = bungie.Value;
            _contextAccessor = contextAccessor;
            _logger = logger;
        }
        
        [HttpGet("{type}/{id}", Name = "CharacterIndex")]
        public async Task<IActionResult> Index(int type, long id)
        {
            var membershipType = (BungieMembershipType)type;
            _logger.LogInformation($"{membershipType}/{id}");

            var accessToken = _contextAccessor.HttpContext.GetTokenAsync("access_token");

            var model = new CharactersViewModel(membershipType, id);

            var profileResponse = await _destiny2.GetProfile(await accessToken, membershipType, id, DestinyComponentType.Characters);
            if (profileResponse == null)
            {
                var url = Url.RouteUrl("AccountIndex");
                return Redirect(url);
            }

            foreach (var item in profileResponse.Characters.Data)
            {
                var classDef = await _manifest.LoadClass(item.Value.ClassHash);
                model.Characters.Add(new Character(item.Key, item.Value, classDef, _bungie.BaseUrl));
            }

            return View(model);
        }
    }
}