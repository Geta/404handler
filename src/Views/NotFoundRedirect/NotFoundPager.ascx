<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BVNetwork.NotFound.Models.RedirectIndexViewData>" %>
<%@ Import Namespace="EPiServer.Cms.Shell" %>
<%@ Import Namespace="EPiServer.Shell.Web.Mvc.Html" %>
<div class="pagerwrapper">


    <div class="pager">
    <% if (Model.IsSuggestions)
       {  %>
        <%= Html.TranslateFormat("/gadget/redirects/suggestionscount", Model.MinIndexOfItem, Model.MaxIndexOfItem, Model.TotalItemsCount)%>
        <% }
       else {  %>
        <%= Html.TranslateFormat("/gadget/redirects/redirectscount", Model.MinIndexOfItem, Model.MaxIndexOfItem, Model.TotalItemsCount)%>
       <%} %>
    </div>
    <div class="pagercenter">
   
     <% if (Model.PageNumber > 1)
           { %>
        <%= Html.ViewLink(
                        "<img src='/App_Themes/Default/Images/Tools/ArrowLeft.gif' alt='Previous' /></a> ",  // html helper
                        "Previous",  // title
                        "Index", // Action name
                        "arrow", // css class
                        "Previous",
                                                                      new { pageNumber = Model.PageNumber - 1, searchWord = Model.SearchWord, pageSize = Model.PageSize, isSuggestions = Model.IsSuggestions })%>
        <% }
           foreach (int page in Model.Pages)
           {
               if (page == 0)
               { %>
        <span>
            <%= Html.Translate("/gadget/redirects/split")%>
        </span>
        <% continue;
               }

               if (page == Model.PageNumber)
               {
                   if (Model.TotalPagesCount > 1)
                   { %>
        <span><b>
            <%= Model.PageNumber%>
        </b></span>
        <% }
                   continue;
               } %>
        <%= Html.ViewLink(
                        page.ToString(),  // html helper
                        page.ToString(),  // title
                        "Index", // Action name
                        "", // css class
                        page.ToString(),
                                                             new { pageNumber = page, searchWord = Model.SearchWord, pageSize = Model.PageSize, isSuggestions = Model.IsSuggestions })%>
        <% }
           if (Model.PageNumber < Model.TotalPagesCount)
           { %>
        <%= Html.ViewLink(
                        "<img src='/App_Themes/Default/Images/Tools/ArrowRight.gif' alt='Next' /></a> ",  // html helper
                        "Next",  // title
                        "Index", // Action name
                        "arrow", // css class
                                         "Next",
                                       new { pageNumber = Model.PageNumber + 1, searchWord = Model.SearchWord, pageSize = Model.PageSize, isSuggestions = Model.IsSuggestions })%>
     <% } %>
    </div>
  

   
      <div class="pagerright">
        <% Html.BeginGadgetForm("Index"); %>
        <input type="submit" class="refresh" value="" />
        <input type="text" name="pageSize" class="refreshinput" value="<%=Model.PageSize %>" />
        <input type="text" class="redirecthidden" name="searchWord" value="<%=Model.SearchWord %>" />
        <input type="text" class="redirecthidden" name="isSuggestions" value ="<%=Model.IsSuggestions %>" />
        <label class="refreshlabel">
            Page size:
        </label>
        <% Html.EndForm(); %>
    </div>
    </div>