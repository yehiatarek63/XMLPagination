using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using XMLParser.Pages;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();
var app = builder.Build();

app.MapPost("/post-fav",  (HttpContext context, [FromBody] Feed newFeed) =>
{
    if (context.Request.Cookies["Favourites"] is null)
    {
        context.Response.Cookies.Append("Favourites", JsonSerializer.Serialize(new List<Feed> { new Feed { XmlUrl = newFeed.XmlUrl, HtmlUrl = newFeed.HtmlUrl, FeedTitle = newFeed.FeedTitle } }), new CookieOptions
        {
            Secure = true,
        });
        return Results.Ok();
    }
    List<Feed> currentFavourites = JsonSerializer.Deserialize<List<Feed>>(context.Request.Cookies["Favourites"]);
    currentFavourites.Add(newFeed);
    context.Response.Cookies.Append("Favourites", JsonSerializer.Serialize(currentFavourites), new CookieOptions
    {
        Secure = true,
    });
    return Results.Ok();
});

app.MapPost("/remove-fav", (HttpContext context, [FromBody] Feed deleteFeed) =>
{
    List<Feed> currentFavourites = JsonSerializer.Deserialize<List<Feed>>(context.Request.Cookies["Favourites"]);
    Feed foundFeed = currentFavourites.Find(feed =>
        feed.XmlUrl == deleteFeed.XmlUrl &&
        feed.HtmlUrl == deleteFeed.HtmlUrl &&
         feed.FeedTitle == deleteFeed.FeedTitle);
    if (foundFeed is not null)
    {
        currentFavourites.Remove(foundFeed);
        context.Response.Cookies.Append("Favourites", JsonSerializer.Serialize(currentFavourites), new CookieOptions
        {
            Secure = true,
        });
        return Results.Ok();
    }
    return Results.BadRequest();
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
