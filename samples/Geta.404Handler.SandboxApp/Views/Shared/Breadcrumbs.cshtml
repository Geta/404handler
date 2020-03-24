@using EPiServer.Core
@using EPiServer.Web
@*Helper used as template for a page in the bread crumb, recursively triggering the rendering of the next page*@
@helper ItemTemplate(HtmlHelpers.MenuItem breadCrumbItem) {
    if (breadCrumbItem.Selected)
    {
        if (breadCrumbItem.Page.HasTemplate() && !breadCrumbItem.Page.ContentLink.CompareToIgnoreWorkID(Model.CurrentPage.ContentLink))
        {
            @Html.PageLink(breadCrumbItem.Page)
        }
        else
        {
            @breadCrumbItem.Page.PageName
        }
        if (!breadCrumbItem.Page.ContentLink.CompareToIgnoreWorkID(Model.CurrentPage.ContentLink))
        {
            <span class="divider">/</span>
            @Html.MenuList(breadCrumbItem.Page.ContentLink, ItemTemplate)
        }
    }
}

<div class="row hideMyTracks">
    <div class="span12">
        <ul class="alloyBreadcrumb">
            @Html.ContentLink(SiteDefinition.Current.StartPage)
            <span class="divider">/</span>
            @Html.MenuList(SiteDefinition.Current.StartPage, ItemTemplate, requireVisibleInMenu: false, requirePageTemplate: false)
        </ul>
    </div>
</div>
