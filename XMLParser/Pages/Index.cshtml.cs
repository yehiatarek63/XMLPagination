using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection;
using System.Security;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Text.Json;
using System.Net.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace XMLParser.Pages;

[BindProperties]
public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public int PageIndex { get; set; } = 0;
    public int PageSize { get; set; } = 5;
    public List<string> FeedTitles { get; set; } = new();
    public List<string> XmlUrls { get; set; } = new();
    public List<string> HtmlUrls { get; set; } = new();
    public int TotalPages { get; set; } = 0;
    
    public IndexModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task OnGet(int? pageIndex)
    {
        if(pageIndex is not null)
        {
            PageIndex = (int)pageIndex;
        }
        else
        {
            PageIndex = 1;
        }
        XmlDocument xmlDoc = new XmlDocument();
        var client = _httpClientFactory.CreateClient();
        (HtmlUrls, FeedTitles, XmlUrls) = await GetOutline(client, "https://blue.feedland.org/opml?screenname=dave");
        TotalPages = (int)Math.Ceiling((double)XmlUrls.Count / PageSize);
        XmlUrls = XmlUrls.Skip((PageIndex - 1) * PageSize).Take(PageSize).ToList();
        FeedTitles = FeedTitles.Skip((PageIndex - 1) * PageSize).Take(PageSize).ToList();
        HtmlUrls = HtmlUrls.Skip((PageIndex - 1) * PageSize).Take(PageSize).ToList();
    }
    
    public IActionResult OnPostStar(string xmlUrl, string htmlUrl, string feedTitle, int pageIndex)
    {
        if (Request.Cookies["Favourites"] is null)
        {
            Response.Cookies.Append("Favourites", JsonSerializer.Serialize(new List<Feed> { new Feed { XmlUrl = xmlUrl, HtmlUrl = htmlUrl, FeedTitle = feedTitle } }), new CookieOptions
            {
                Secure = true,
            });
            return RedirectToPage("Index", new { PageIndex = pageIndex });
        }
        List<Feed> currentFavourites = JsonSerializer.Deserialize<List<Feed>>(Request.Cookies["Favourites"]);
        Feed newFeed = new Feed
        {
            XmlUrl = xmlUrl,
            HtmlUrl = htmlUrl,
            FeedTitle = feedTitle
        };
        currentFavourites.Add(newFeed);
        Response.Cookies.Append("Favourites", JsonSerializer.Serialize(currentFavourites), new CookieOptions
        {
            Secure = true,
        });
        return RedirectToPage("Index", new { PageIndex = pageIndex });
    }

    public IActionResult OnPostDeleteStar(string xmlUrl, string htmlUrl, string feedTitle, int pageIndex)
    {
        List<Feed> currentFavourites = JsonSerializer.Deserialize<List<Feed>>(Request.Cookies["Favourites"]);
        Feed currentFeed = currentFavourites.Find(x => x.XmlUrl == xmlUrl && x.HtmlUrl == htmlUrl && x.FeedTitle == feedTitle);
        if(currentFeed is not null)
        {
            currentFavourites.Remove(currentFeed);
            Response.Cookies.Append("Favourites", JsonSerializer.Serialize(currentFavourites), new CookieOptions
            {
                Secure = true,
            });
        }
        return RedirectToPage("Index", new {PageIndex = pageIndex});
    }

    async Task<(List<string>, List<string>, List<string>)> GetOutline(HttpClient client, string url)
    {
        HttpResponseMessage response = await client.GetAsync(url);
        List<string> htmlUrls = new List<string>();
        List<string> title = new List<string>();
        List<string> xmlUrls = new List<string>();
        if (response.IsSuccessStatusCode)
        {
            string content = await response.Content.ReadAsStringAsync();
            XDocument doc = XDocument.Parse(content);
            htmlUrls = doc.Descendants("outline").Select(x => x.Attribute("htmlUrl").Value).ToList();
            title = doc.Descendants("outline").Select(x => x.Attribute("text").Value).ToList();
            xmlUrls = doc.Descendants("outline").Select(x => x.Attribute("xmlUrl").Value).ToList();
        }
        return (htmlUrls, title, xmlUrls);
    }
}

public class Feed
{
    public string XmlUrl { get; set; }
    public string HtmlUrl { get; set; }
    public string FeedTitle { get; set; }
}
    



