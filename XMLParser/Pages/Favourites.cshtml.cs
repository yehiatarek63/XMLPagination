using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Xml;
using static XMLParser.Pages.IndexModel;

namespace XMLParser.Pages;
public class FavouritesModel : PageModel
{

    public List<Feed> AllFavourites { get; set; } = new();
    public void OnGet()
    {
        if (Request.Cookies["Favourites"] is not null)
        {
            AllFavourites = JsonSerializer.Deserialize<List<Feed>>(Request.Cookies["Favourites"]);
        }
    }

}

