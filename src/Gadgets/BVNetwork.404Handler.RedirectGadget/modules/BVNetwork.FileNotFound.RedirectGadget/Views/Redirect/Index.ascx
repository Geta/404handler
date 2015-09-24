<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BVNetwork.FileNotFound.RedirectIndexViewData>" %>
<div class="notfound">
    <%
        if(Model.IsSuggestions)
            Html.RenderPartial("Suggestions", Model);
        else
            Html.RenderPartial("Redirects", Model);

        Html.RenderPartial("Pager", Model); 
    %>
</div>
    