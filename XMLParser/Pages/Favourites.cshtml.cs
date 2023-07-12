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

    public IActionResult OnPostDeleteStar(string xmlUrl, string htmlUrl, string feedTitle)
    {
        List<Feed> currentFavourites = JsonSerializer.Deserialize<List<Feed>>(Request.Cookies["Favourites"]);
        Feed currentFeed = currentFavourites.Find(x => x.XmlUrl == xmlUrl && x.HtmlUrl == htmlUrl && x.FeedTitle == feedTitle);
        if (currentFeed is not null)
        {
            currentFavourites.Remove(currentFeed);
            Response.Cookies.Append("Favourites", JsonSerializer.Serialize(currentFavourites), new CookieOptions
            {
                Secure = true,
            });
        }
        return RedirectToPage("Favourites");
    }
}

