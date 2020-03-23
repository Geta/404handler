// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

namespace BVNetwork.NotFound.Core.NotFoundPage
{
    /// <summary>
    /// Summary description for PageContent.
    /// </summary>
    public class PageContent
    {
        private const string DefTitle = "Page Not Found";
        private const string DefToptext = "We're sorry, but the page you requested could not be found<br><br>";
        private const string DefLookingFor = "You were looking for:<br>";
        private const string DefCameFrom = "<br>You came from:<br>";
        private const string DefBottomText = "<br><br><b>Visit <a href=\"/\">the front page</a><b>";

        private string _title = DefTitle;
        private string _topText = DefToptext;
        private string _lookingFor = DefLookingFor;
        private string _cameFrom = DefCameFrom;
        private string _bottomText = DefBottomText;

        public PageContent()
        {
            InitializeFromLanguageFile();
        }

        /// <summary>
        /// Initializes text strings from the language file.
        /// </summary>
        private void InitializeFromLanguageFile()
        {
            var languageService = EPiServer.Framework.Localization.LocalizationService.Current;

            var cultureInfo = EPiServer.Globalization.ContentLanguage.PreferredCulture;

            if (cultureInfo == null) return;

            languageService.TryGetStringByCulture("/templates/notfound/title", cultureInfo, out _title);
            languageService.TryGetStringByCulture("/templates/notfound/toptext", cultureInfo, out _topText);
            languageService.TryGetStringByCulture("/templates/notfound/lookingfor", cultureInfo, out _lookingFor);
            languageService.TryGetStringByCulture("/templates/notfound/referer", cultureInfo, out _cameFrom);
            languageService.TryGetStringByCulture("/templates/notfound/bottomtext", cultureInfo, out _bottomText);
        }

        /// <summary>
        /// Gets or sets the bottom text.
        /// </summary>
        /// <value></value>
        public string BottomText
        {
            get => _bottomText;
            set => _bottomText = value;
        }

        /// <summary>
        /// Gets or sets the came from text.
        /// </summary>
        /// <value></value>
        public string CameFrom
        {
            get => _cameFrom;
            set => _cameFrom = value;
        }
        /// <summary>
        /// Gets or sets the looking for text.
        /// </summary>
        /// <value></value>
        public string LookingFor
        {
            get => _lookingFor;
            set => _lookingFor = value;
        }
        /// <summary>
        /// Gets or sets the top text.
        /// </summary>
        /// <value></value>
        public string TopText
        {
            get => _topText;
            set => _topText = value;
        }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value></value>
        public string Title
        {
            get => _title;
            set => _title = value;
        }
    }
}
