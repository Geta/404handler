<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BVNetwork.FileNotFound.RedirectIndexViewData>" %>
<%@ Import Namespace="BVNetwork.FileNotFound.Redirects" %>
<%@ Import Namespace="BVNetwork.FileNotFound.RedirectGadget" %>
<%@ Import Namespace="EPiServer.Cms.Shell" %>
<%@ Import Namespace="EPiServer.Core" %>
<%@ Import Namespace="EPiServer.Shell.Web.Mvc.Html" %>
<%@ Import Namespace="BVNetwork.FileNotFound.RedirectGadget.Models" %>
<%@ Import Namespace="BVNetwork.FileNotFound.DataStore" %>

    <!-- <div class="notfound about"> 
    </div>
    -->
    <div>
        <% Html.BeginGadgetForm("Index"); %>
        <div class="epi-formArea">
            <fieldset>
                <div class="rightsearch">
                    <input type="text" class="notfound-search" name="searchWord" />
                     <button type="submit" class="notfoundbutton search"><%=Html.Translate("/gadget/redirects/search")%></button>
                    <input type="hidden" name="pageSize" value='<%=Model.PageSize %>' />
                    <input type="hidden" name="isSuggestions" value='<%=Model.IsSuggestions %>' />
                </div>
                <%=Model.ActionInformation as string %>
            </fieldset>
        </div>
        <% Html.EndForm(); %>
       
        <input type="hidden" name="pageSize" value='<%=Model.PageSize %>' />
        <% Html.RenderPartial("Menu", "Index"); %>
        <% Html.BeginGadgetForm("Save"); %>
        <table class="epi-default">
            <thead>
                <tr>
                    <th>
                        <label>
                            <%=LanguageManager.Instance.Translate("/gadget/redirects/oldurl")%></label>
                    </th>
                    <th>
                        <label>
                            <%=LanguageManager.Instance.Translate("/gadget/redirects/newurl")%></label>
                    </th>
                    <th style="text-align: center">
                        <label>
                            <%=LanguageManager.Instance.Translate("/gadget/redirects/wildcard")%></label>
                    </th>
                    <th>
                    </th>
                </tr>
            </thead>
          
            <tr>
                <td class="longer">
                    <input style="word-wrap: break-word" name="oldUrl" class="required redirect-longer" />
                </td>
                <td class="longer">
                    <input name="newUrl" class="required redirect-longer" />
                </td>
                <td class="shorter delete">
                    <input name="skipWildCardAppend" type="checkbox" />
                </td>
                <td class="shorter delete">
                     <button type="submit" class="notfoundbutton">Add</button>
                </td>
            </tr>
         
            <% if (Model.CustomRedirectList.Count > 0)
               {
                   foreach (CustomRedirect m in Model.CustomRedirectList)
                   {
            %>
            <tr>
                <td class="longer">
                    <%= Html.Encode(m.OldUrl)%>
                </td>
                <td class="longer">
                    <%if (m.State.Equals((int)BVNetwork.FileNotFound.DataStore.DataStoreHandler.GetState.Ignored))
                      {%>
                    <i>[<%=LanguageManager.Instance.Translate("/gadget/redirects/ignored")%>]</i>
                    <%}
                      else
                      {  %>
                    <%= m.NewUrl%>
                    <% } %>
                </td>
                <td class="shorter delete">
                    <img src="/App_Themes/Default/Images/Tools/<%=m.WildCardSkipAppend ? "CheckBoxOn.gif" : "CheckBoxOff.gif" %>" />
                </td>
                <td class="shorter delete">
                    <%= Html.ViewLink(
                        "",  // html helper
                        "Delete",  // title
                        "Delete", // Action name
                                        "epi-quickLinksDelete epi-iconToolbar-item-link epi-iconToolbar-delete", // css class
                        "Index",
                        new { oldUrl =  Uri.EscapeDataString(m.OldUrl), pageNumber = Model.PageNumber, searchWord = Model.SearchWord, pageSize = Model.PageSize })%>
                </td>
            </tr>
            <%} %>
            <% 
               } %>
        </table>
        <% Html.EndForm(); %>
    </div>
