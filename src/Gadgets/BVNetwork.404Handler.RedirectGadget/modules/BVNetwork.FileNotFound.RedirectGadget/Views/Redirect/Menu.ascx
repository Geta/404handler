<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<string>" %>
<%@ Import Namespace="EPiServer.Cms.Shell" %>
<%@ Import Namespace="BVNetwork.FileNotFound.Redirects" %>
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
    </ul>
</div>
