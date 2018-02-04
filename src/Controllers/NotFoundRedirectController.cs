using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using BVNetwork.NotFound.Core;
using BVNetwork.NotFound.Core.CustomRedirects;
using BVNetwork.NotFound.Core.Data;
using BVNetwork.NotFound.Models;
using EPiServer.Logging;
using EPiServer.Framework.Localization;
using EPiServer.Security;
using EPiServer.Shell.Gadgets;

namespace BVNetwork.NotFound.Controllers
{
    [EPiServer.Shell.Web.ScriptResource("ClientResources/Scripts/jquery.blockUI.js")]
    [Gadget(ResourceType = typeof(NotFoundRedirectController),
           NameResourceKey = "GadgetName", DescriptionResourceKey = "GadgetDescription")]
    [EPiServer.Shell.Web.CssResource("ClientResources/Content/RedirectGadget.css")]
    [EPiServer.Shell.Web.ScriptResource("ClientResources/Scripts/jquery.form.js")]
    [Authorize]
    public class NotFoundRedirectController : Controller
    {

        private static readonly ILogger Logger = LogManager.GetLogger();
        private void CheckAccess()
        {
            if (!PrincipalInfo.HasEditAccess)
            {
                throw new SecurityException("Access denied");
            }
        }

        public ActionResult Index(int? pageNumber, string searchWord, int? pageSize, bool? isSuggestions, bool? showRedirects)
        {

            CheckAccess();

            if (!string.IsNullOrEmpty(CustomRedirectHandler.CustomRedirectHandlerException))
            {
                return Content("An error has occured in the dynamic data store: " + CustomRedirectHandler.CustomRedirectHandlerException);
            }
            var suggestion = false; ;
            List<CustomRedirect> customRedirectList;

            if (isSuggestions.HasValue && isSuggestions.Value)
            {
                customRedirectList = GetSuggestions(searchWord);

                suggestion = true;
                var viewData = GetRedirectIndexViewData(pageNumber, customRedirectList, GetSearchResultInfo(searchWord, customRedirectList.Count, suggestion), searchWord, pageSize, suggestion, showRedirects);
                if (customRedirectList != null && customRedirectList.Count > 0)
                {
                    viewData.HighestSuggestionValue = customRedirectList.First().NotfoundErrorCount;
                    viewData.LowestSuggestionValue = customRedirectList.Last().NotfoundErrorCount;
                }
                return View("Index", viewData);
            }
            else
            {
                if (!(showRedirects.HasValue && showRedirects.Value))
                {
                    customRedirectList = new List<CustomRedirect>();
                    return View("Index", GetRedirectIndexViewData(pageNumber, customRedirectList, LocalizationService.Current.GetString("/gadget/redirects/inactiveredirects"), searchWord, pageSize, suggestion, showRedirects));
                }
                customRedirectList = GetData(searchWord);
            }
            return View("Index", GetRedirectIndexViewData(pageNumber, customRedirectList, GetSearchResultInfo(searchWord, customRedirectList.Count, suggestion), searchWord, pageSize, suggestion, showRedirects));
        }

        public ActionResult SaveSuggestion(string oldUrl, string newUrl, string skipWildCardAppend, string exactMatch, string skipQueryString, int? pageNumber, int? pageSize)
        {
            CheckAccess();
            SaveRedirect(oldUrl, newUrl, skipWildCardAppend, exactMatch, skipQueryString);

            // delete rows from DB
            var dbAccess = DataAccessBaseEx.GetWorker();
            dbAccess.DeleteRowsForRequest(oldUrl);

            //
            List<CustomRedirect> customRedirectList = GetSuggestions(null);
            string actionInfo = string.Format(LocalizationService.Current.GetString("/gadget/redirects/saveredirect"), oldUrl, newUrl);
            DataStoreEventHandlerHook.DataStoreUpdated();
            var viewData = GetRedirectIndexViewData(pageNumber, customRedirectList, actionInfo, null, pageSize, true, true);
            viewData.HighestSuggestionValue = customRedirectList.First().NotfoundErrorCount;
            viewData.LowestSuggestionValue = customRedirectList.Last().NotfoundErrorCount;
            return View("Index", viewData);
        }

        public ActionResult Suggestions()
        {
            CheckAccess();
            return Index(null, "", null, true, true);
        }

        [GadgetAction(Text = "Administer")]
        public ActionResult Administer()
        {
            CheckAccess();
            return View();
        }

        [ValidateInput(false)]
        public ActionResult Save(string oldUrl, string newUrl, string skipWildCardAppend, string exactMatch, string skipQueryString, int? pageNumber, int? pageSize)
        {
            CheckAccess();
            SaveRedirect(oldUrl, newUrl, skipWildCardAppend, exactMatch, skipQueryString);
            List<CustomRedirect> redirectList = GetData(null);
            string actionInfo = string.Format(LocalizationService.Current.GetString("/gadget/redirects/saveredirect"), oldUrl, newUrl);
            return View("Index", GetRedirectIndexViewData(pageNumber, redirectList, actionInfo, null, pageSize, false, true));

        }

        public void SaveRedirect(string oldUrl, string newUrl, string skipWildCardAppend, string exactMatch, string skipQueryString)
        {
            oldUrl = UrlStandardizer.Standardize(oldUrl?.Trim());
            Logger.Debug("Adding redirect: '{0}' -> '{1}'", oldUrl, newUrl);
            // Get hold of the datastore
            DataStoreHandler dsHandler = new DataStoreHandler();
            dsHandler.SaveCustomRedirect(new CustomRedirect(oldUrl, newUrl.Trim(),
                skipWildCardAppend == null ? false : true,
                exactMatch == null ? false : true,
                skipQueryString == null ? false : true));
            DataStoreEventHandlerHook.DataStoreUpdated();

        }

        public ActionResult IgnoreRedirect(string oldUrl, int pageNumber, string searchWord, int pageSize)
        {
            CheckAccess();
            // delete rows from DB
            var dbAccess = DataAccessBaseEx.GetWorker();
            dbAccess.DeleteRowsForRequest(oldUrl);

            // add redirect to dds with state "ignored"
            var redirect = new CustomRedirect();
            redirect.OldUrl = oldUrl;
            redirect.State = Convert.ToInt32(DataStoreHandler.State.Ignored);
            DataStoreHandler dsHandler = new DataStoreHandler();
            dsHandler.SaveCustomRedirect(redirect);
            DataStoreEventHandlerHook.DataStoreUpdated();

            List<CustomRedirect> customRedirectList = GetSuggestions(searchWord);
            string actionInfo = string.Format(LocalizationService.Current.GetString("/gadget/redirects/ignoreredirect"), oldUrl);
            RedirectIndexViewData viewData = GetRedirectIndexViewData(pageNumber, customRedirectList, actionInfo, searchWord, pageSize, true, true);
            viewData.HighestSuggestionValue = customRedirectList.First().NotfoundErrorCount;
            viewData.LowestSuggestionValue = customRedirectList.Last().NotfoundErrorCount;
            return View("Index", viewData);
        }

        [ValidateInput(false)]
        public ActionResult Delete(string oldUrl, int? pageNumber, string searchWord, int? pageSize)
        {
            CheckAccess();

            Logger.Debug("Deleting redirect: '{0}'", oldUrl);

            DataStoreHandler dsHandler = new DataStoreHandler();
            dsHandler.DeleteCustomRedirect(oldUrl);
            DataStoreEventHandlerHook.DataStoreUpdated();
            List<CustomRedirect> customRedirectList = GetData(searchWord);
            //Make sure that the searchinfo is contained after an item has been deleted - if there is any.
            return View("Index", GetRedirectIndexViewData(pageNumber, customRedirectList, GetSearchResultInfo(searchWord, customRedirectList.Count, false), searchWord, pageSize, false, true));
        }

        /// <summary>
        /// Get the data that will be presented in the view(s).
        /// </summary>
        /// <param name="pageNumber">The current page number for the pager view</param>
        /// <param name="redirectList">The List of redirects</param>
        /// <param name="actionInformation">Text that will be presented in the view</param>
        /// <returns></returns>
        public RedirectIndexViewData GetRedirectIndexViewData(int? pageNumber, List<CustomRedirect> redirectList, string actionInformation, string searchWord, int? pageSize, bool isSuggestions, bool? showRedirects)
        {
            RedirectIndexViewData indexData = new RedirectIndexViewData();
            indexData.IsSuggestions = isSuggestions;
            indexData.ActionInformation = actionInformation;
            indexData.ShowRedirects = showRedirects.HasValue ? showRedirects.Value : false;
            indexData.SearchWord = searchWord;
            indexData.TotalItemsCount = redirectList.Count;
            indexData.PageNumber = pageNumber ?? 1;
            //TODO: read pagersize and pagesize from configuration.
            indexData.PagerSize = 4;
            indexData.PageSize = pageSize ?? 30;
            if (redirectList.Count > indexData.PageSize)
                indexData.CustomRedirectList = redirectList.GetRange(indexData.MinIndexOfItem - 1, indexData.MaxIndexOfItem - indexData.MinIndexOfItem + 1);
            else
                indexData.CustomRedirectList = redirectList;
            return indexData;

        }

        public ActionResult Ignored()
        {
            CheckAccess();
            DataStoreHandler dsHandler = new DataStoreHandler();
            var ignoredRedirects = dsHandler.GetIgnoredRedirect();
            return View("Ignored", ignoredRedirects);
        }
        public ActionResult Deleted()
        {
            CheckAccess();
            DataStoreHandler dsHandler = new DataStoreHandler();
            var deletedRedirects = dsHandler.GetDeletedRedirect();
            return View("Deleted", deletedRedirects);
        }


        public ActionResult Unignore(string url)
        {
            CheckAccess();
            DataStoreHandler dsHandler = new DataStoreHandler();
            dsHandler.DeleteCustomRedirect(url);
            return Ignored();
        }

        public ActionResult Referers(string url)
        {
            CheckAccess();
            var referers = DataHandler.GetReferers(url);
            ViewData.Add("refererUrl", url);
            return View("Referers", referers);
        }

        public ActionResult DeleteAllIgnored()
        {
            CheckAccess();
            var dsHandler = new DataStoreHandler();
            int deleteCount = dsHandler.DeleteAllIgnoredRedirects();
            string infoText = string.Format(LocalizationService.Current.GetString("/gadget/redirects/ignoredremoved"), deleteCount);
            ViewData["information"] = infoText;
            return View("Administer");
        }

        public ActionResult DeleteAllSuggestions()
        {
            CheckAccess();
            DataAccessBaseEx.GetWorker().DeleteAllSuggestions();
            ViewData["information"] = LocalizationService.Current.GetString("/gadget/redirects/suggestionsdeleted");
            return View("Administer");
        }

        public ActionResult DeleteAllRedirects()
        {
            CheckAccess();
            DataStoreHandler dsHandler = new DataStoreHandler();
            dsHandler.DeleteAllCustomRedirects();
            DataStoreEventHandlerHook.DataStoreUpdated();
            ViewData["information"] = LocalizationService.Current.GetString("/gadget/redirects/redirectsdeleted");
            return View("Administer");
        }

        /// <summary>
        /// Removed Deleted (410) redirect
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public ActionResult DeleteDeleted(string url)
        {
            CheckAccess();
            var dsHandler = new DataStoreHandler();
            dsHandler.DeleteCustomRedirect(url);
            return Deleted();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public FileUploadJsonResult ImportRedirects(HttpPostedFileBase xmlfile)
        {
            CheckAccess();
            // Read all redirects from xml file
            RedirectsXmlParser parser = new RedirectsXmlParser(xmlfile.InputStream);
            // Save all redirects from xml file
            CustomRedirectCollection redirects = parser.Load();
            string message;
            if (redirects != null || redirects.Count != 0)
            {
                CustomRedirectHandler.Current.SaveCustomRedirects(redirects);
                message = string.Format(LocalizationService.Current.GetString("/gadget/redirects/importsuccess"), redirects.Count);
            }
            else
            {
                message = LocalizationService.Current.GetString("/gadget/redirects/importnone");
            }
            return new FileUploadJsonResult { Data = new { message = message } };
        }

        public XmlActionResult ExportAllRedirects()
        {
            CheckAccess();
            DataStoreHandler dsHandler = new DataStoreHandler();

            List<CustomRedirect> redirects = dsHandler.GetCustomRedirects(true);

            XmlDocument document = new RedirectsXmlParser().Export(redirects);

            return new XmlActionResult(document);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public FileUploadJsonResult ImportDeleted(HttpPostedFileBase txtFile)
        {
            CheckAccess();
            var redirects = new CustomRedirectCollection();
            using (var streamReader = new StreamReader(txtFile.InputStream))
            {
                while (streamReader.Peek() >= 0)
                {
                    var url = streamReader.ReadLine();
                    if (!string.IsNullOrEmpty(url))
                    {
                        redirects.Add(new CustomRedirect
                        {
                            OldUrl = url,
                            State = (int)DataStoreHandler.State.Deleted,
                        });
                    }
                }
            }
            string message;
            if (redirects.Count != 0)
            {
                CustomRedirectHandler.Current.SaveCustomRedirects(redirects);
                message = string.Format(LocalizationService.Current.GetString("/gadget/redirects/importdeletedsuccess"), redirects.Count);
            }
            else
            {
                message = LocalizationService.Current.GetString("/gadget/redirects/importnone");
            }
            return new FileUploadJsonResult { Data = new { message = message } };
        }


        public ActionResult DeleteSuggestions(int maxErrors, int minimumDays)
        {
            CheckAccess();
            DataAccessBaseEx.GetWorker().DeleteSuggestions(maxErrors, minimumDays);
            ViewData["information"] = LocalizationService.Current.GetString("/gadget/redirects/suggestionsdeleted");
            return View("Administer");
        }

        /// <summary>
        /// Get the tekst that will be displayed in the info area of the gadget.
        /// </summary>
        /// <param name="searchWord"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public string GetSearchResultInfo(string searchWord, int count, bool isSuggestions)
        {
            string actionInfo;
            if (string.IsNullOrEmpty(searchWord) && !isSuggestions)
            {
                actionInfo = string.Format(LocalizationService.Current.GetString("/gadget/redirects/storedredirects"), count);
                actionInfo += " " + string.Format(LocalizationService.Current.GetString("/gadget/redirects/andsuggestions"), DataHandler.GetTotalSuggestionCount());
            }
            else if (string.IsNullOrEmpty(searchWord) && isSuggestions)
                actionInfo = string.Format(LocalizationService.Current.GetString("/gadget/redirects/storedsuggestions"), count);
            else if (isSuggestions)
                actionInfo = string.Format(LocalizationService.Current.GetString("/gadget/redirects/searchsuggestions"), searchWord, count);
            else
                actionInfo = string.Format(LocalizationService.Current.GetString("/gadget/redirects/searchresult"), searchWord, count);
            return actionInfo;
        }

        /// <summary>
        /// Get custom redirect data from dynamic data store.
        /// </summary>
        /// <param name="searchWord"></param>
        /// <returns></returns>
        public List<CustomRedirect> GetData(String searchWord)
        {
            DataStoreHandler dsHandler = new DataStoreHandler();
            List<CustomRedirect> customRedirectList;
            if (string.IsNullOrEmpty(searchWord))
            {
                customRedirectList = dsHandler.GetCustomRedirects(true);
            }
            else
                customRedirectList = dsHandler.SearchCustomRedirects(searchWord);

            return customRedirectList;
        }

        public List<CustomRedirect> GetSuggestions(String searchWord)
        {

            var customRedirectList = new List<CustomRedirect>();
            var dict = DataHandler.GetRedirects();

            foreach (KeyValuePair<string, int> redirect in dict)
            {
                customRedirectList.Add(new CustomRedirect(redirect.Key, Convert.ToInt32(DataStoreHandler.State.Suggestion), redirect.Value));
            }

            return customRedirectList;
        }
        public List<CustomRedirect> GetDeletedUrls()
        {
            DataStoreHandler dsHandler = new DataStoreHandler();
            List<CustomRedirect> customRedirectList;
            customRedirectList = dsHandler.GetDeletedRedirect();

            return customRedirectList;
        }

        public static string GadgetEditMenuName
        {
            get { return LocalizationService.Current.GetString("/gadget/redirects/configure"); }
        }

        public static string GadgetName
        {
            get { return LocalizationService.Current.GetString("/gadget/redirects/name"); }
        }

        public static string GadgetDescription
        {
            get { return LocalizationService.Current.GetString("/gadget/redirects/description"); }
        }

        public ActionResult AddDeletedUrl(string oldUrl)
        {
            CheckAccess();


            // add redirect to dds with state "deleted"
            var redirect = new CustomRedirect();
            redirect.OldUrl = oldUrl;
            redirect.State = Convert.ToInt32(DataStoreHandler.State.Deleted);
            DataStoreHandler dsHandler = new DataStoreHandler();
            dsHandler.SaveCustomRedirect(redirect);
            DataStoreEventHandlerHook.DataStoreUpdated();

            // delete rows from DB
            var dbAccess = DataAccessBaseEx.GetWorker();
            dbAccess.DeleteRowsForRequest(oldUrl);

            //
            List<CustomRedirect> customRedirectList = GetDeletedUrls();
            DataStoreEventHandlerHook.DataStoreUpdated();
            return Deleted();
        }
    }

    public class FileUploadJsonResult : JsonResult
    {

        public override void ExecuteResult(ControllerContext context)
        {
            this.ContentType = "text/html";
            context.HttpContext.Response.Write("<textarea>");
            base.ExecuteResult(context);
            context.HttpContext.Response.Write("</textarea>");
        }
    }
}
