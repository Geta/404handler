# 404 Handler for EPiServer
The popular 404 handler for EPiServer, enabling better control over your 404 page in addition to allowing redirects for old urls that no longer works.

The perfect companion if you're transitioning to EPiServer from another system and cannot keep the url structure, or plan to do major re-structuring of your content.

# Installation
`Install-Package BVN.404Handler`

The package can be found in the [EPiServer Nuget Feed](http://nuget.episerver.com/).

Until we've moved the documentation over, please check out the existing (somewhat outdated) [docs on EPiCode](https://www.coderesort.com/p/epicode/wiki/404Handler).

# Custom Handlers
If you need more advanced or custom logic to create redirects, you can implement an INotFoundHandler.

1. Create a class that implements `BVNetwork.NotFound.Core.INotFoundHandler`
2. In the `public string RewriteUrl(string url)` method, add your custom logic
3. Register the handler in web.config:
```xml
<bvn404Handler handlerMode="On" fileNotFoundPage="/your404pagehere">
    <providers>
        <add name="Custom Handler" type="Your.Name.Space.YourClass, YourAssemblyName" />
    </providers>
</bvn404Handler>
```

This is especially useful for rewrites that follow some kind of logic, like checking the querystring for and id or some other value you can use to look up the page.

Here is an example using EPiServer Find to look up a product by code:

```csharp
public class CustomProductRedirectHandler : INotFoundHandler
{
    public string RewriteUrl(string url)
    {
        if(url.Contains("productid"))
        {
            // Give it a thorough look - see if we can redirect it
            Url uri = new Url(url);
            var productId = uri.QueryCollection.GetValues("productid").FirstOrDefault();
            if (productId != null && string.IsNullOrEmpty(productId) == false)
            {
                SearchResults<FindProduct> results = SearchClient.Instance.Search<FindProduct>()
                    .Filter(p => p.Code.MatchCaseInsensitive(productId))
                    .GetResult();
                if (results.Hits.Any())
                {
                    // Pick the first one
                    SearchHit<FindProduct> product = results.Hits.FirstOrDefault();
                    return product.Document.ProductUrl;
                }
            }
        }
        return null;
    }
}
```

**Note!** Make sure the code you add has good performance, it could be called a lot. If you're querying a database or a search index, you might want to add caching and perhaps Denial Of Service prevention measures.

# Mentions and Resources
* http://world.episerver.com/blogs/Per-Magne-Skuseth/Dates/2013/2/404-handler-for-EPiServer-7/
* http://jondjones.com/episerver-7-installing-bvn-404handler/
* https://github.com/markeverard/BVN.404Handler.MvcContrib
* https://www.coderesort.com/p/epicode/wiki/404Handler
