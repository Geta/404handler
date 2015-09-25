<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BVNetwork.NotFound.Models.RedirectIndexViewData>" %>
<div class="notfound">
    <%
        if(Model.IsSuggestions)
            Html.RenderPartial("Suggestions", Model);
        else
            Html.RenderPartial("Redirects", Model);

        Html.RenderPartial("NotFoundPager", Model); 
    %>
</div>
    