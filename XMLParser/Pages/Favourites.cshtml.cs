using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml;
using static XMLParser.Pages.IndexModel;

namespace XMLParser.Pages;
public class FavouritesModel : PageModel
{
    public List<string> Favourites { get; set; } = new List<string>();
    public List<string> XmlUrls { get; set; } = new List<string>();
    public List<string> HtmlUrls { get; set; } = new List<string>();
    public List<string> FeedTitles { get; set; } = new List<string>();
    private readonly IHttpClientFactory _httpClientFactory;
    public FavouritesModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    public void OnGet()
    {
        if (Request.Cookies["XmlUrl"] is not null && Request.Cookies["HtmlUrl"] is not null && Request.Cookies["FeedTitle"] is not null)
        {
            XmlUrls = Request.Cookies["XmlUrl"].Split(',').ToList();
            HtmlUrls = Request.Cookies["HtmlUrl"].Split(',').ToList();
            FeedTitles = Request.Cookies["FeedTitle"].Split(',').ToList();
        }
    }

    public IActionResult OnPostDeleteStar(string xmlUrl, string htmlUrl, string feedTitle)
    {
        if (Request.Cookies["XmlUrl"] is not null && Request.Cookies["HtmlUrl"] is not null && Request.Cookies["FeedTitle"] is not null)
        {
            List<string> xmlUrlList = Request.Cookies["XmlUrl"].Split(',').ToList();
            List<string> htmlUrlList = Request.Cookies["HtmlUrl"].Split(',').ToList();
            List<string> feedTitleList = Request.Cookies["FeedTitle"].Split(',').ToList();
            xmlUrlList.Remove(xmlUrl);
            htmlUrlList.Remove(htmlUrl);
            feedTitleList.Remove(feedTitle);
            Response.Cookies.Append("XmlUrl", string.Join(",", xmlUrlList), new CookieOptions
            {
                Secure = true
            });
            Response.Cookies.Append("HtmlUrl", string.Join(",", htmlUrlList), new CookieOptions
            {
                Secure = true
            });
            Response.Cookies.Append("FeedTitle", string.Join(",", feedTitleList), new CookieOptions
            {
                Secure = true
            });
        }
        return RedirectToPage("Favourites");
    }
}

