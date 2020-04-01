<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<string>" %>
<%@ Import Namespace="EPiServer.Cms.Shell" %>
<%@ Import Namespace="EPiServer.Shell.Web.Mvc.Html" %>




<div class="epi-tabView">
    <ul>
        <li class="<%= Model == "Index" ? "selected" : "" %> ntab">
            <%= Html.ViewLink(
                    "Custom Redirects", // html helper
                    "", // title
                    "Index", // Action name
                    "", // css class
                    "",
                    new {}) %>
        </li>
        <li class="<%= Model == "Suggestions" ? "selected" : "" %> ntab">
            <%= Html.ViewLink(
                    "Suggestions", // html helper
                    "", // title
                    "Index", // Action name
                    "", // css class
                    "",
                    new {isSuggestions = true}) %>
        </li>
        <li class="<%= Model == "Ignored" ? "selected" : "" %> ntab">
            <%= Html.ViewLink(
                    "Ignored", // html helper
                    "", // title
                    "Ignored", // Action name
                    "", // css class
                    "",
                    new {isSuggestions = true}) %>
        </li>
        <li class="<%= Model == "Deleted" ? "selected" : "" %> ntab">
            <%= Html.ViewLink(
                    "Deleted", // html helper
                    "", // title
                    "Deleted", // Action name
                    "", // css class
                    "",
                    new {}) %>
        </li>
    </ul>
</div>
