using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml;


namespace XMLParser.Pages;
[BindProperties]
public class FeedModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public List<FeedItem> FeedData { get; set; } = new();
    public FeedModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    public async Task OnGet(string xmlUrl)
    {
        XmlDocument xmlDoc = new XmlDocument();
        var client = _httpClientFactory.CreateClient();
        HttpResponseMessage response = await client.GetAsync(xmlUrl);
        if (response.IsSuccessStatusCode)
        {
            string content = await response.Content.ReadAsStringAsync();
            xmlDoc.LoadXml(content);
            foreach (var node in xmlDoc.SelectNodes("rss/channel/item").Cast<XmlNode>())
            {
                FeedItem item = new FeedItem();
                if (node.SelectSingleNode("title") is not null)
                {
                    item.Title = node.SelectSingleNode("title").InnerText;
                }
                item.Description = node.SelectSingleNode("description").InnerText;
                item.PubDate = node.SelectSingleNode("pubDate").InnerText;
                item.Link = node.SelectSingleNode("link").InnerText;
                item.Guid = node.SelectSingleNode("guid").InnerText;
                FeedData.Add(item);
            }
        }
    }
}

public class FeedItem
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string PubDate { get; set; }
    public string Link { get; set; }
    public string Guid { get; set; }
}