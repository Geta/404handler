<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Dictionary<string,int>>" %>
<%@ Import Namespace="BVNetwork.NotFound.RedirectGadget" %>
<%@ Import Namespace="EPiServer.Core" %>
<%@ Import Namespace="EPiServer.Shell.Web.Mvc.Html" %>
<div class="notfound">
    <div class="epi-formArea">
        <fieldset>
            Referers for <i>
                <%=ViewData["refererUrl"].ToString() %></i>
        </fieldset>
    </div>
      <div class="epi-tabView">
    <ul>
         <li class="ntab">
            <%= Html.ViewLink(
                    "Back to suggestions", // html helper
                    "", // title
                    "Index", // Action name
                    "", // css class
                    "",
                    new {isSuggestions = true}) %>
        </li>
        </ul>
        </div>
    <br/>
    <table class="epi-default">
        <thead>
            <tr>
                <th>
                    Referers
                </th>
            </tr>
        </thead>
        <%   foreach (var referer in Model)
             {
        %>
        <tr>
            <td class="longer">
                <span><a style="color: #398AC9; cursor: pointer" href="<%= Html.Encode(referer.Key)%>">
                    <%= Html.Encode(referer.Key)%></a><%= string.Format("<i> ({0} errors)</i>", referer.Value)%></span>
            </td>
        </tr>
        <%
             } %>
    </table>
</div>
