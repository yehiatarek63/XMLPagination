using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection;
using System.Security;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Net.Http;

namespace XMLParser.Pages
{
    [BindProperties]
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public List<List<ItemProperties>> RssItems { get; set; } = new List<List<ItemProperties>>();
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public List<string> FeedTitles { get; set; } = new List<string>();
        public List<string> XmlUrls { get; set; } = new List<string>();
        public List<string> HtmlUrls { get; set; } = new List<string>();
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
            var paginatedXmlUrls = XmlUrls.Skip((PageIndex - 1) * PageSize).Take(PageSize).ToList();
            FeedTitles = FeedTitles.Skip((PageIndex - 1) * PageSize).Take(PageSize).ToList();
            HtmlUrls = HtmlUrls.Skip((PageIndex - 1) * PageSize).Take(PageSize).ToList();
            foreach (var url in paginatedXmlUrls)
            {
               List<ItemProperties> ItemsProperties = new List<ItemProperties>();
               HttpResponseMessage response = await client.GetAsync(url);
               if (response.IsSuccessStatusCode)
               {
                   string content = await response.Content.ReadAsStringAsync();
                   xmlDoc.LoadXml(content);
                   foreach (var node in xmlDoc.SelectNodes("rss/channel/item").Cast<XmlNode>())
                   {
                       ItemProperties item = new ItemProperties();
                       if (node.SelectSingleNode("title") is not null)
                       {
                           item.Title = node.SelectSingleNode("title").InnerText;
                       }
                       item.Description = node.SelectSingleNode("description").InnerText;
                       item.PubDate = node.SelectSingleNode("pubDate").InnerText;
                       item.Link = node.SelectSingleNode("link").InnerText;
                       item.Guid = node.SelectSingleNode("guid").InnerText;
                       ItemsProperties.Add(item);
                   }
                   RssItems.Add(ItemsProperties);
               }
            }
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
}


public class ItemProperties
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string PubDate { get; set; }
    public string Link { get; set; }
    public string Guid { get; set; }
}
