// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.Collections.Generic;
using System.IO;
using System.Xml;
using EPiServer.Logging;

namespace BVNetwork.NotFound.Core.CustomRedirects
{
    /// <summary>
    /// Class for reading and writing to the custom redirects file
    /// </summary>
    public class RedirectsXmlParser
    {
        private static readonly ILogger Logger = LogManager.GetLogger();
        private readonly XmlDocument _customRedirectsXmlFile;

        private const string Newurl = "new";
        private const string Oldurl = "old";
        private const string Skipwildcard = "onWildCardMatchSkipAppend";
        private const string RedirectType = "redirectType";

        /// <summary>
        /// Reads the custom redirects information from the specified xml file
        /// </summary>
        public RedirectsXmlParser(Stream xmlContent)
        {
            _customRedirectsXmlFile = new XmlDocument();
            if (xmlContent != null)
            {
                _customRedirectsXmlFile.Load(xmlContent);
            }
            else
            {
                // Not on disk, not in a vpp, construct an empty one
                _customRedirectsXmlFile = new XmlDocument
                {
                    InnerXml = "<redirects><urls></urls></redirects>"
                };
                Logger.Error("404 Handler: The Custom Redirects file does not exist.");
            }
        }

        public RedirectsXmlParser()
        {
        }

        /// <summary>
        /// Parses the xml file and reads all redirects.
        /// </summary>
        /// <returns>A collection of CustomRedirect objects</returns>
        public CustomRedirectCollection Load()
        {
            const string urlpath = "/redirects/urls/url";

            var redirects = new CustomRedirectCollection();

            // Parse all url nodes
            var nodes = _customRedirectsXmlFile.SelectNodes(urlpath);
            foreach (XmlNode node in nodes)
            {
                // Each url new url can have several old values
                // we need to create a redirect object for each pair
                var newNode = node.SelectSingleNode(Newurl);

                var oldNodes = node.SelectNodes(Oldurl);
                foreach (XmlNode oldNode in oldNodes)
                {
                    var skipWildCardAppend = false;
                    var skipWildCardAttr = oldNode.Attributes[Skipwildcard];
                    if (skipWildCardAttr != null)
                    {
                        // If value parsing fails, it will be false by default. We do
                        // not really care to check if it fails, as we cannot do anything
                        // about it (throwing an exception is not a good idea here)
                        bool.TryParse(skipWildCardAttr.Value, out skipWildCardAppend);
                    }

                    var redirectType = Constants.Permanent;
                    var redirectTypeAttr = oldNode.Attributes[RedirectType];
                    if (redirectTypeAttr != null)
                    {
                        int.TryParse(redirectTypeAttr.Value, out redirectType);
                    }

                    // Create new custom redirect nodes
                    var redirect = new CustomRedirect(oldNode.InnerText, newNode.InnerText, skipWildCardAppend, redirectType);
                    redirects.Add(redirect);
                }
            }

            return redirects;
        }

        public XmlDocument Export(List<CustomRedirect> redirects)
        {
            var document = new XmlDocument();
            var xmlDeclaration = document.CreateXmlDeclaration("1.0", "UTF-8", null);
            var root = document.DocumentElement;
            document.InsertBefore(xmlDeclaration, root);

            var redirectsElement = document.CreateElement(string.Empty, "redirects", string.Empty);
            document.AppendChild(redirectsElement);

            var urlsElement = document.CreateElement(string.Empty, "urls", string.Empty);
            redirectsElement.AppendChild(urlsElement);

            foreach (var redirect in redirects)
            {
                if (string.IsNullOrWhiteSpace(redirect.OldUrl) || string.IsNullOrWhiteSpace(redirect.NewUrl))
                {
                    continue;
                }

                var urlElement = document.CreateElement(string.Empty, "url", string.Empty);

                var oldElement = document.CreateElement(string.Empty, Oldurl, string.Empty);
                oldElement.AppendChild(document.CreateTextNode(redirect.OldUrl.Trim()));
                if (redirect.WildCardSkipAppend)
                {
                    var wildCardAttribute = document.CreateAttribute(string.Empty, Skipwildcard, string.Empty);
                    wildCardAttribute.Value = "true";
                    oldElement.Attributes.Append(wildCardAttribute);
                }

                var redirectTypeAttribute = document.CreateAttribute(string.Empty, RedirectType, string.Empty);
                redirectTypeAttribute.Value = redirect.RedirectType.ToString();
                oldElement.Attributes.Append(redirectTypeAttribute);

                var newElement = document.CreateElement(string.Empty, Newurl, string.Empty);
                newElement.AppendChild(document.CreateTextNode(redirect.NewUrl.Trim()));

                urlElement.AppendChild(oldElement);
                urlElement.AppendChild(newElement);

                urlsElement.AppendChild(urlElement);
            }

            return document;
        }
    }
}
