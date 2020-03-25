<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<List<CustomRedirect>>" %>
<%@ Import Namespace="BVNetwork.NotFound.Core.CustomRedirects" %>
<%@ Import Namespace="EPiServer.Framework.Localization" %>

<div class="notfound">
       <div class="epi-formArea">
            <fieldset>
               <%=string.Format(LocalizationService.Current.GetString("/gadget/redirects/deletedurls"), Model.Count)%>

            </fieldset>
        </div>

       <% Html.RenderPartial("Menu", "Deleted"); %>

    <% Html.BeginGadgetForm("AddDeletedUrl"); %>

    <table class="epi-default">
        <thead>
            <tr>
                <th>
                    <label>
                        <%= Html.Translate("/gadget/redirects/url")%></label>
                </th>
                <th>&nbsp;</th>
            </tr>
        </thead>

        <tr>
            <td class="longer">
                <input style="word-wrap: break-word" name="oldUrl" class="required redirect-longer" />
            </td>
            <td class="shorter delete">
                    <button type="submit" class="notfoundbutton">Add</button>
            </td>
        </tr>

        <%foreach (CustomRedirect m in Model) { %>
        <tr>
            <td class="redirect-longer">
                <%= Html.Encode(m.OldUrl)%>
            </td>
            <td class="shorter delete">
                <%= Html.ViewLink(
                        "",  // html helper
                        "Delete",  // title
                        "DeleteDeleted", // Action name
                        "epi-quickLinksDelete epi-iconToolbar-item-link epi-iconToolbar-delete", // css class
                        "Index",
                        new { url =  m.OldUrl })%>
            </td>
        </tr>
        <%} %>
    </table>
    <% Html.EndForm(); %>
</div>