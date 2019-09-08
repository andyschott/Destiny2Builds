using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Destiny2Builds.Models
{
    public class SocketCategoryViewModel
    {
        public string Name { get; set; }
        public IEnumerable<SocketViewModel> Sockets { get; set; }

        public static IEnumerable<SocketCategoryViewModel> Convert(IEnumerable<SocketCategory> categories)
        {
            return categories.Select(category => new SocketCategoryViewModel
            {
                Name = category.Name,
                Sockets = category.Sockets.Select(socket => new SocketViewModel
                {
                    Perks = socket.Perks.Select(perk => new SelectListItem
                    {
                        Text = perk.Name,
                        Value = perk.Hash.ToString(),
                        Selected = perk.IsSelected,
                    }).ToList()
                }).ToList()
            });
        }
    }
}