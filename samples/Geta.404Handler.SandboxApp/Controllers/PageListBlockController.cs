using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EPiServer.Core;
using EPiServer.Filters;
using Geta._404Handler.SandboxApp.Business;
using Geta._404Handler.SandboxApp.Models.Blocks;
using Geta._404Handler.SandboxApp.Models.ViewModels;
using EPiServer.Web.Mvc;
using EPiServer;

namespace Geta._404Handler.SandboxApp.Controllers
{
    public class PageListBlockController : BlockController<PageListBlock>
    {
        private ContentLocator contentLocator;
        private IContentLoader contentLoader;
        public PageListBlockController(ContentLocator contentLocator, IContentLoader contentLoader)
        {
            this.contentLocator = contentLocator;
            this.contentLoader = contentLoader;
        }

        public override ActionResult Index(PageListBlock currentBlock)
        {
            var pages = FindPages(currentBlock);

            pages = Sort(pages, currentBlock.SortOrder);

            if(currentBlock.Count > 0)
            {
                pages = pages.Take(currentBlock.Count);
            }

            var model = new PageListModel(currentBlock)
                {
                    Pages = pages
                };

            ViewData.GetEditHints<PageListModel, PageListBlock>()
                .AddConnection(x => x.Heading, x => x.Heading);

            return PartialView(model);
        }

        private IEnumerable<PageData> FindPages(PageListBlock currentBlock)
        {
            IEnumerable<PageData> pages;
            var listRoot = currentBlock.Root;
            if (currentBlock.Recursive)
            {
                if (currentBlock.PageTypeFilter != null)
                {
                    pages = contentLocator.FindPagesByPageType(listRoot, true, currentBlock.PageTypeFilter.ID);
                }
                else
                {
                    pages = contentLocator.GetAll<PageData>(listRoot);
                }
            }
            else
            {
                if (currentBlock.PageTypeFilter != null)
                {
                    pages = contentLoader.GetChildren<PageData>(listRoot)
                        .Where(p => p.ContentTypeID == currentBlock.PageTypeFilter.ID);
                }
                else
                {
                    pages = contentLoader.GetChildren<PageData>(listRoot);
                }
            }

            if (currentBlock.CategoryFilter != null && currentBlock.CategoryFilter.Any())
            {
                pages = pages.Where(x => x.Category.Intersect(currentBlock.CategoryFilter).Any());
            }
            return pages;
        }

        private IEnumerable<PageData> Sort(IEnumerable<PageData> pages, FilterSortOrder sortOrder)
        {
            var asCollection = new PageDataCollection(pages);
            var sortFilter = new FilterSort(sortOrder);
            sortFilter.Sort(asCollection);
            return asCollection;
        }
    }
}
