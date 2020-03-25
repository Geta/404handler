<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<BVNetwork.NotFound.Models.RedirectIndexViewData>" %>
<%@ Import Namespace="BVNetwork.NotFound.Core" %>
<%@ Import Namespace="BVNetwork.NotFound.Core.CustomRedirects" %>
<%@ Import Namespace="BVNetwork.NotFound.Core.Data" %>
<%@ Import Namespace="EPiServer.Framework.Localization" %>
<script type="text/javascript">

    function replaceArrow(inputid) {



        if ($('#' + inputid).hasClass('down')) {
            $('#' + inputid).removeClass('down');
            $('#' + inputid).addClass('up');
        }
        else {
            $('#' + inputid).removeClass('up');
            $('#' + inputid).addClass('down');
        }

    }

</script>
<div>

    <div class="epi-formArea">
        <fieldset>
            <%=Model.ActionInformation as string %>
        </fieldset>
    </div>

    <input type="hidden" name="pageSize" value='<%=Model.PageSize %>' />
    <% Html.RenderPartial("Menu", "Suggestions"); %>

    <table class="epi-default">
        <thead>
            <tr>
                <th>
                    <label>
                        <%= Html.Translate("/gadget/redirects/oldurl")%></label>
                </th>
                <th>
                    <label>
                        <%= Html.Translate("/gadget/redirects/newurl")%></label>
                </th>

                <th style="text-align: center">
                    <label>
                        <%= Html.Translate("/gadget/redirects/ignore")%></label>
                </th>
            </tr>
        </thead>

        <tr>
             <td class="longer" style="display: none">

                 <%--fix to avoid double input validation in table--%>
              <% Html.BeginGadgetForm(""); %>
              <% Html.EndForm(); %>
            </td>

        </tr>
        <% if (Model.CustomRedirectList.Count > 0)
           {
               int i = 0;
               foreach (CustomRedirect m in Model.CustomRedirectList)
               {

        %>

        <%  var referers = DataHandler.GetReferers(m.OldUrl);
            bool showReferers = true;

            if (referers.Count == 1 && referers.First().Key == DataHandler.UknownReferer)
                showReferers = false;

        %>
        <tr>

            <td class="longer">
                <span style="color: #<%=ColorHelper.GetRedTone(Model.HighestSuggestionValue, Model.LowestSuggestionValue, m.NotfoundErrorCount)%>0000">
                    <%= Html.Encode(m.OldUrl) + string.Format("<i> ({0} errors)</i>", m.NotfoundErrorCount)%></span>
                <input id="showreferers_<%=i %>" type="button" style="display: <%= showReferers ? "" : "none"%>"
                       class="notfound showreferers down" onclick="$('#referer<%=i %>    ').slideToggle('fast');replaceArrow('showreferers_<%=i %>    ');"
                       value="Known referrers" />
                <br />
                <span id="referer<%=i %>" style="display: none;" />
                    <ul class="notfound refererlist">
                        <%
                   int j = 0;
                   foreach (var referer in referers)
                   {
                       if (j < 10)
                       {
                        %>
                        <li>
                            <%if (referer.Key != DataHandler.UknownReferer)
                              {  %>
                            <a style="color: #398AC9; cursor: pointer" href="<%=referer.Key %>">
                                <%=referer.Key%></a> (<%=referer.Value%>)
                                            <%}
                              else
                              {  %>
                            <%=referer.Key%>
                                                (<%=referer.Value%>)
                                            <%} %></li>
                        <%
                                        } j++;
                                    } %>
                        <%if (referers.Count > 10)
                          {  %>
                        <%= Html.ViewLink(
                                            "Show all referers (" + (referers.Count - 10) +" more)",  // html helper
                                            "Ignore",  // title
                                            "Referers", // Action name
                                            "notfound allreferers", // css class
                                            "",
                                            new { url = Uri.EscapeDataString(m.OldUrl) })%>
                        <%} %>
                    </ul>

            </td>

          <td class="longer">
              <% Html.BeginGadgetForm("SaveSuggestion"); %>


            <div class="longer">

                <input name="newUrl" class="required redirect-longer" />

            </div>
            <div class="shorter delete">
                  <input value="<%=m.OldUrl %>" name="oldUrl" class="redirect-longer" style="display: none" />
                <button class="notfoundbutton" name="submit" type="submit">Add</button>



            </div>
                <% Html.EndForm(); %>

            </td>
            <td class="shorter delete">
                <%= Html.ViewLink(
                                "",  // html helper
                                LocalizationService.Current.GetString("/gadget/redirects/ignoreexplanation"),  // title
                                "IgnoreRedirect", // Action name
                                "epi-quickLinksDelete epi-iconToolbar-item-link epi-iconToolbar-delete", // css class
                                "Index",
                                new { oldUrl = Uri.EscapeDataString(m.OldUrl), pageNumber = Model.PageNumber, searchWord = Model.SearchWord, pageSize = Model.PageSize })%>
            </td>


        </tr>
        <%
               i++;
               }

           } %>
    </table>

</div>
