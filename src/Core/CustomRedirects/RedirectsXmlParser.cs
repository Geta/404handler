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
        private XmlDocument _customRedirectsXmlFile = null;

        /// <summary>
        /// Reads the custom redirects information from the specified xml file
        /// </summary>
        /// <param name="virtualFile">The virtual path to the xml file containing redirect settings</param>
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
                _customRedirectsXmlFile = new XmlDocument();
                _customRedirectsXmlFile.InnerXml = "<redirects><urls></urls></redirects>";
                Logger.Error("404 Handler: The Custom Redirects file '{0}' does not exist.", xmlContent);
            }
        }

        /// <summary>
        /// Parses the xml file and reads all redirects.
        /// </summary>
        /// <returns>A collection of CustomRedirect objects</returns>
        public CustomRedirectCollection Load()
        {
            const string URLPATH = "/redirects/urls/url";
            const string NEWURL = "new";
            const string OLDURL = "old";
            const string SKIPWILDCARD = "onWildCardMatchSkipAppend";

            CustomRedirectCollection redirects = new CustomRedirectCollection();

            // Parse all url nodes
            XmlNodeList nodes = _customRedirectsXmlFile.SelectNodes(URLPATH);
            foreach (XmlNode node in nodes)
            {
                // Each url new url can have several old values
                // we need to create a redirect object for each pair
                XmlNode newNode = node.SelectSingleNode(NEWURL);

                XmlNodeList oldNodes = node.SelectNodes(OLDURL);
                foreach (XmlNode oldNode in oldNodes)
                {
                    bool skipWildCardAppend = false;
                    XmlAttribute skipWildCardAttr = oldNode.Attributes[SKIPWILDCARD];
                    if (skipWildCardAttr != null)
                    {
                        // If value parsing fails, it will be false by default. We do
                        // not really care to check if it fails, as we cannot do anything
                        // about it (throwing an exception is not a good idea here)
                        bool.TryParse(skipWildCardAttr.Value, out skipWildCardAppend);
                    }

                    // Create new custom redirect nodes
                    CustomRedirect redirect = new CustomRedirect(oldNode.InnerText, newNode.InnerText, skipWildCardAppend);
                    redirects.Add(redirect);
                }
            }

            return redirects;
        }
    }
}
