
## Important Security Notice!
The default [notfound.aspx](https://github.com/BVNetwork/404handler/blob/master/src/NotFound.aspx) page has an XSS vulnerability. If you are using this page, please change it according to [this commit](https://github.com/BVNetwork/404handler/commit/02419c904db096a607ba6775b04db7fdf042002a#diff-4cf2534cd91830ac43ef09100c7a18d8).

# 404 Handler for EPiServer
**Build status:** 
![Build status](http://tc.geta.no/app/rest/builds/buildType:(id:TeamFrederik_404handler_RestoreBuildPackagePublish)/statusIcon)
[![Platform](https://img.shields.io/badge/Platform-.NET%204.6.1-blue.svg?style=flat)](https://msdn.microsoft.com/en-us/library/w0x726c2%28v=vs.110%29.aspx)
[![Platform](https://img.shields.io/badge/Episerver%20-%2011-orange.svg?style=flat)](https://world.episerver.com/cms/)

The popular 404 handler for EPiServer, enabling better control over your 404 page in addition to allowing redirects for old urls that no longer works.

The perfect companion if you're transitioning to EPiServer from another system and cannot keep the url structure, or plan to do major re-structuring of your content.

![](https://raw.githubusercontent.com/BVNetwork/404handler/master/doc/img/Redirects.png)

- The 404 handler stores the redirects in the database, not web.config. Editors can add redirects without any deployments.
  - All redirects are edited in the gadget as shown above. After the add-on is installed and a proper 404 page has been created, no changes to the application is needed to add new redirects.
  - You can import and export redirects as XML from the admin interface.
- Handles partial and "full" urls and can redirect out of the site by using fully qualified urls for the "New url" field.
- Supports wildcard redirects.
- By using fully qualified urls in the "Old url" field, they will only apply for that specific site. Editing redirects is done for all sites in the same UI.
- You need to be authorized to work with the gadget, and have access to edit mode, but there is no restriction on which editors can edit.

# Installation
`Install-Package BVN.404Handler`

The package can be found in the [EPiServer Nuget Feed](http://nuget.episerver.com/).

# Configuration
When installed, the following sections are added to your web.config:

```xml
<configSections>
	<section name="bvn404Handler" type="BVNetwork.NotFound.Configuration.Bvn404HandlerConfiguration, BVNetwork.EPi404" />
</configSections>
<bvn404Handler handlerMode="On">
</bvn404Handler>
<system.webServer>        
	<httpErrors errorMode="Custom" existingResponse="Replace">
        <remove statusCode="404" />
        <error statusCode="404" path="/notfound.aspx" responseMode="ExecuteURL" />
    </httpErrors>
</system.webServer>
```

You can turn off the redirects by setting `handlerMode` to `Off`.

The `httpErrors` section is responsible for showing a custom 404 page. If you do not already have this section in your config, it will be added with the default settings as shown above. If the section exists, it will not be changed, and you might have to change it manually.

**Important!** `errorMode` needs to be set to `Custom` and `existingResponse` needs to be set to `Replace`, or the 404 page will not be shown, or will only be shown for urls not ending with `.aspx`. 

## Logging
Suggestions for 404 rules require 404 requests to be logged to the database.

Logging of 404 requests is buffered to shield your application from Denial of Service attacks. By default, logging will happen for every 30'th error. You can edit the `bvn404Handler` section in `web.config` and set `bufferSize="0"` to log the errors immediately. This is not recommended as you will be vulnerable to massive logging to your database. You can control how much you would like to log by specifying a threshold value. This value determines how frequent 404 errors are allowed to be logged.

**Important!** Even if the threshold is set low, you can still receive a lot of requests in the 404 log. In the Admin view (follow "Administer" link in gadget) you can delete suggestions (logged 404 requests). You can find all the logged items in the `BVN.NotFoundRequests` table in your CMS database if you want to manually clear the logged requests (this will not remove any redirects).

![](https://raw.githubusercontent.com/BVNetwork/404handler/master/doc/img/Administer.png)

```xml
<bvn404Handler handlerMode="On" logging="On" bufferSize="30" threshold="5">
</bvn404Handler>
```

**logging**: Turn logging `On` or `Off`. Default is `On`

**bufferSize**: Size of memory buffer to hold 404 requests. Default is 30

**threshold**: Average maximum allowed requests per second. Default is 5

 * Example 1:
   * bufferSize is set to 100, threshold is set to 10
   * Case: 100 errors in 5 seconds - (diff = seconds between first logged request and the last logged request in the buffer).
   * 100 / 5 = 20. Error frequency is higher than threshold value. Buffered requests will not get logged, the entire buffer will be discarded.
 * Example 2:
   * bufferSize is 100, threshold is 10
   * Case: 100 errors in 15 seconds
   * 100 / 15 = 6. Error frequency is within threshold value. Buffered requests will get logged.
   
If the `bufferSize` is set to 0, the `threshold` value will be ignored, and every request will be logged immediately.

## Specifying ignored resources
By default, requests to files with the following extensions will be ignored by the redirect module: `jpg,gif,png,css,js,ico,swf,woff`

If you want to specify this yourself, add `ignoredResourceExtensions` to the configuration:
```xml
<bvn404Handler handlerMode="On" 
               ignoredResourceExtensions="jpg,gif,png,css,js,ico,swf,woff,eot,otf">
</bvn404Handler>
```

# Custom 404 Page
You probably want to change the path to the 404 page to something else, typically a view in your project, or even a page in Episerver. Example:

```xml
<error statusCode="404" path="/Error/Error404" responseMode="ExecuteURL" />
```

In this case, the `/Error/Error404` is a MVC Action (`Error404`) on a `Error` controller. This will work out of the box (you no longer need Mark Everard's [BVN.404Handler.MvcContrib](https://github.com/markeverard/BVN.404Handler.MvcContrib) package), but the Http status code will be 200, not 404. You can set this status code yourself, or decorate the Action with the `NotFoundPage` attribute.

Example:
```C#
public class ErrorController : PageControllerBase<PageData>
{
    
    [BVNetwork.NotFound.Core.NotFoundPage.NotFoundPage]
    public ActionResult Error404()
    {
		// Custom implementation
        ErrorPageViewModel model = GetViewModel();

        // The Action Filter will add the following to the ViewBag:
        // Referrer, NotFoundUrl and StatusCode
        model.NotFoundUrl = ViewBag.NotFoundUrl;
        model.Referer = ViewBag.Referrer;

        return View("Error404", model);
    }
}
```

The `NotFoundPageAttribute` action filter will set the http status code to 404 and populate the ViewBag with the following properties: `Referrer`, `NotFoundUrl` and `StatusCode`. You can use these properties in your view or for additional logging.

The filter will also attempt to set the language by matching the language segment in the beginning of the url (if used.)

# Custom Handlers
If you need more advanced or custom logic to create redirects, you can implement an INotFoundHandler.

1. Create a class that implements `BVNetwork.NotFound.Core.INotFoundHandler`
2. In the `public string RewriteUrl(string url)` method, add your custom logic
3. Register the handler in web.config:
```xml
<bvn404Handler handlerMode="On">
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

# Troubleshooting
The module has extensive logging. Turn on debug logging for the `BVNetwork.NotFound` namespace in your episerverlog.config:
```xml
<logger name="BVNetwork.NotFound"
            additivity="false">
    <level value="All" />
    <appender-ref ref="debugLogAppender"/>
</logger>
```

# Mentions and Resources
* http://world.episerver.com/blogs/Per-Magne-Skuseth/Dates/2013/2/404-handler-for-EPiServer-7/
* http://jondjones.com/episerver-7-installing-bvn-404handler/
* https://github.com/markeverard/BVN.404Handler.MvcContrib
* https://www.coderesort.com/p/epicode/wiki/404Handler
