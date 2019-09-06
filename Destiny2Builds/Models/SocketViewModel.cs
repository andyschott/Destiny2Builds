using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Destiny2Builds.Models
{
    public class SocketViewModel
    {
        public IList<SelectListItem> Perks { get; set; }
    }
}