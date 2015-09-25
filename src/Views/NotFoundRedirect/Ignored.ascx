<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<List<CustomRedirect>>" %>
<%@ Import Namespace="BVNetwork.NotFound.Core.CustomRedirects" %>
<%@ Import Namespace="EPiServer.Core" %>

<div class="notfound">
       <div class="epi-formArea">
            <fieldset>
               <%=string.Format(LanguageManager.Instance.Translate("/gadget/redirects/ignoredsuggestions"), Model.Count)%>
         
            </fieldset>
        </div>

       <% Html.RenderPartial("Menu", "Ignored"); %>
    <table class="epi-default">
        <thead>
            <tr>
                <th>
                    <label>
                        <%=LanguageManager.Instance.Translate("/gadget/redirects/url")%></label>
                </th>
                <th>
                 <%=LanguageManager.Instance.Translate("/gadget/redirects/unignore")%>
                </th>
            </tr>
        </thead>
        <%foreach (CustomRedirect m in Model) { %>
        <tr>
            <td class="redirect-longer">
                <%= Html.Encode(m.OldUrl)%>
            </td>
            <td class="shorter delete">
                <%= Html.ViewLink(
                        "",  // html helper
                        "Delete",  // title
                        "Unignore", // Action name
                        "epi-quickLinksDelete epi-iconToolbar-item-link epi-iconToolbar-delete", // css class
                        "Index",
                        new { url =  m.OldUrl })%>
            </td>
        </tr>
        <%} %>
    </table>
</div>