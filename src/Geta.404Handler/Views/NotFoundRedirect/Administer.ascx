<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%@ Import Namespace="System.Web.Mvc.Html" %>
<%@ Import Namespace="EPiServer.Shell" %>
<%@ Import Namespace="EPiServer.Shell.Web.Mvc.Html" %>
<script type="text/javascript">

    function notfoundvalidator() {
        //$('#notfoundajaxUploadForm').submit(function (event) {
        var file = $('#notfoundajaxUploadForm input[type=file]').val();
        if (!file) {
            $("#notfoundajaxUploadForm label.error").text("No file selected!");
            return false;
        }
        $("#notfoundajaxUploadForm label.error").text("");
        return true;
    }

    function notfoundDeletedvalidator() {
        var file = $('#notfoundDeletedajaxUploadForm input[type=file]').val();
        if (!file) {
            $("#notfoundDeletedajaxUploadForm label.error").text("No file selected!");
            return false;
        }
        $("#notfoundDeletedajaxUploadForm label.error").text("");
        return true;
    }
    $(function () {
        $("#notfoundajaxUploadForm").ajaxForm({
            iframe: true,
            dataType: "json",
            beforeSubmit: notfoundvalidator,
            success: function (result) {
                $("#notfoundajaxUploadForm").unblock();
                $("#notfoundajaxUploadForm").resetForm();
                $(".notfound-viewInfo").text(result.message);
            },
            error: function (xhr, textStatus, errorThrown) {
                $("#notfoundajaxUploadForm").unblock();
                $("#notfoundajaxUploadForm").resetForm();
                $(".notfound-viewInfo").text('Error uploading file');
            }
        });
        $("#notfoundDeletedajaxUploadForm").ajaxForm({
            iframe: true,
            dataType: "json",
            beforeSubmit: notfoundDeletedvalidator,
            success: function (result) {
                $("#notfoundDeletedajaxUploadForm").unblock();
                $("#notfoundDeletedajaxUploadForm").resetForm();
                $(".notfound-viewInfo").text(result.message);
            },
            error: function (xhr, textStatus, errorThrown) {
                $("#notfoundDeletedajaxUploadForm").unblock();
                $("#notfoundDeletedajaxUploadForm").resetForm();
                $(".notfound-viewInfo").text('Error uploading file');
            }
        });
    });
</script>

<div class="notfound">
    <div class="epi-formArea">
        <fieldset>
            <span class="notfound-viewInfo">
                <%=ViewData["information"] ?? Html.Translate("/gadget/redirects/Administerinfo") %>
            </span>
        </fieldset>
    </div>
    <div class="epi-tabView">
        <ul>
            <li class="<%= Model == "Index" ? "selected" : "" %> ntab">
                <%= Html.ViewLink(
                    "Back to redirects", // html helper
                    "", // title
                    "Index", // Action name
                    "", // css class
                    "",
                    new {}) %>
        </li>
        </ul>
    </div>
    <br />
    <table class="epi-default">
        <tr>
            <td>
                <div class="longer">
                    <%= Html.Translate("/gadget/redirects/deleteignoreinfo") %>
                </div>
                <div class="shorter delete">
                    <% Html.BeginGadgetForm("DeleteAllIgnored"); %>
                    <button type="submit" class="notfoundbutton" style="float: right">Delete</button>
                    <% Html.EndForm(); %>
                </div>
            </td>
        </tr>

        <tr>
            <td>
                <div class="longer">
                    <%= Html.Translate("/gadget/redirects/deleteallSuggestions") %>
                </div>
                <div class="shorter delete">
                    <% Html.BeginGadgetForm("DeleteAllSuggestions"); %>
                    <button type="submit" class="notfoundbutton" style="float: right">Delete</button>
                    <% Html.EndForm(); %>
                </div>
            </td>
        </tr>

        <tr>
            <td>
                <div class="longer">
                    <%= Html.Translate("/gadget/redirects/deleteallredirects") %>
                </div>
                <div class="shorter delete">
                    <% Html.BeginGadgetForm("DeleteAllRedirects"); %>
                    <button type="submit" class="notfoundbutton" style="float: right">Delete</button>
                    <% Html.EndForm(); %>
                </div>
            </td>
        </tr>

        <tr>
            <td>
                <% Html.BeginGadgetForm("DeleteSuggestions"); %>
                <div class="delete suggestions">
                    <%= Html.Translate("/gadget/redirects/suggesteionsdeleteinfo1") %>
                    <input name="maxErrors" value="5" class="required" />
                    <%= Html.Translate("/gadget/redirects/suggesteionsdeleteinfo2") %>
                    <input name="minimumDays" value="30" class="required" />
                    <%= Html.Translate("/gadget/redirects/suggesteionsdeleteinfo3") %>
                </div>

                <div class="shorter delete">
                    <button type="submit" class="notfoundbutton" style="float: right">Delete</button>

                </div>
                <% Html.EndForm(); %>
            </td>
        </tr>

        <tr>
            <td class="notfound-full">
                <form id="notfoundajaxUploadForm" action="<%=Paths.ProtectedRootPath %>BVNetwork.404Handler/NotFoundRedirect/ImportRedirects" method="post" enctype="multipart/form-data">
                    <div class="longer">
                        <%= Html.Translate("/gadget/redirects/importinfo") %>
                        <label class="error"></label>
                        <input type="file" name="xmlfile" />

                    </div>
                    <div class="shorter delete">

                        <button type="submit" class="notfoundbutton" style="float: right">Import</button>
                    </div>

                </form>
            </td>

        </tr>



        <tr>
            <td class="notfound-full">
                <form id="notfoundDeletedajaxUploadForm" action="<%=Paths.ProtectedRootPath %>BVNetwork.404Handler/NotFoundRedirect/ImportDeleted" method="post" enctype="multipart/form-data">
                    <div class="longer">
                        <%= Html.Translate("/gadget/redirects/importDeleted") %>
                        <label class="error"></label>
                        <input type="file" name="txtFile" />

                    </div>
                    <div class="shorter delete">

                        <button type="submit" class="notfoundbutton" style="float: right">Import</button>
                    </div>

                </form>
            </td>

        </tr>

        <tr>
            <td>
                <div class="longer">
                    <%= Html.Translate("/gadget/redirects/exportredirects") %>
                </div>
                <div class="shorter delete">
                    <% Html.BeginForm("ExportAllRedirects", "NotFoundRedirect", new {@class = "epi-gadgetform", target="_blank"}); %>
                    <button type="submit" class="notfoundbutton" style="float: right">Export</button>
                    <% Html.EndForm(); %>
                </div>
            </td>
        </tr>

        <tr>
            <td>
                <div class="longer">
                    <%= Html.Translate("/gadget/redirects/migrateddstosql") %>
                </div>
                <div class="shorter delete">
                    <% Html.BeginGadgetForm("MigrateDdsToSql"); %>
                    <button type="submit" class="notfoundbutton" style="float: right">Migrate</button>
                    <% Html.EndForm(); %>
                </div>
            </td>
        </tr>
    </table>

</div>
