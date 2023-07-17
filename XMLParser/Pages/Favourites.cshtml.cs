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

    public IActionResult OnPostDeleteStar([FromBody] Feed deleteFeed)
    {
        List<Feed> currentFavourites = JsonSerializer.Deserialize<List<Feed>>(Request.Cookies["Favourites"]);
        Feed foundFeed = currentFavourites.Find(feed =>
            feed.XmlUrl == deleteFeed.XmlUrl &&
            feed.HtmlUrl == deleteFeed.HtmlUrl &&
             feed.FeedTitle == deleteFeed.FeedTitle);
        if (foundFeed is not null)
        {
            currentFavourites.Remove(foundFeed);
            Response.Cookies.Append("Favourites", JsonSerializer.Serialize(currentFavourites), new CookieOptions
            {
                Secure = true,
            });
            return new OkResult();
        }
        return new BadRequestResult();
    }
}

