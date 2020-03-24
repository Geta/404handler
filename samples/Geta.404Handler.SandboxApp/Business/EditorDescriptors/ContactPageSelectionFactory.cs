using System.Collections.Generic;
using System.Linq;
using EPiServer.ServiceLocation;
using EPiServer.Shell.ObjectEditing;

namespace Geta._404Handler.SandboxApp.Business.EditorDescriptors
{
    /// <summary>
    /// Provides a list of options corresponding to ContactPage pages on the site
    /// </summary>
    /// <seealso cref="ContactPageSelector"/>
    public class ContactPageSelectionFactory : ISelectionFactory
    {
        private Injected<ContentLocator> ContentLocator { get; set; }

        public IEnumerable<ISelectItem> GetSelections(ExtendedMetadata metadata)
        {
            var contactPages = ContentLocator.Service.GetContactPages();

            return new List<SelectItem>(contactPages.Select(c => new SelectItem {Value = c.PageLink, Text = c.Name}));
        }
    }
}
