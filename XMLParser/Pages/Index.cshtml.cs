using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection;
using System.Security;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace XMLParser.Pages;

[BindProperties]
public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 5;
    public List<string> FeedTitles { get; set; } = new List<string>();
    public List<string> XmlUrls { get; set; } = new List<string>();
    public List<string> HtmlUrls { get; set; } = new List<string>();
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
        string currentValue = Request.Cookies["XmlUrl"];
        if (currentValue is not null)
        {
            currentValue += "," + xmlUrl;
        }
        else
        {
            currentValue = xmlUrl;
        }
        Response.Cookies.Append("XmlUrl", currentValue, new CookieOptions
        {
            Secure = true,
        });
        string currentFeedTitle = Request.Cookies["FeedTitle"];
        if (currentFeedTitle is not null)
        {
            currentFeedTitle += "," + feedTitle;
        }
        else
        {
            currentFeedTitle = feedTitle;
        }
        Response.Cookies.Append("FeedTitle", currentFeedTitle, new CookieOptions
        {
            Secure = true,
        });
        string currentHtmlUrl = Request.Cookies["HtmlUrl"];
        if (currentHtmlUrl is not null)
        {
            currentHtmlUrl += "," + htmlUrl;
        }
        else
        {
            currentHtmlUrl = htmlUrl;
        }
        Response.Cookies.Append("HtmlUrl", currentHtmlUrl, new CookieOptions
        {
            Secure = true
        });
        return RedirectToPage("Index", new { PageIndex = pageIndex });
    }

    public IActionResult OnPostDeleteStar(string xmlUrl, string htmlUrl, string feedTitle, int pageIndex)
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

    



