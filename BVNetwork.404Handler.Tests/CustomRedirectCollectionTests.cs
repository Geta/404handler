using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BVNetwork.NotFound.Core;
using BVNetwork.NotFound.Core.CustomRedirects;
using EPiServer.ServiceLocation;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace BVNetwork.NotFound
{
    public class CustomRedirectCollectionTests
    {
        private CustomRedirectCollection Collection;

        [SetUp]
        public void BeforeEach()
        {
            var locator = Substitute.For<IServiceLocator>();
            ServiceLocator.SetLocator(locator);
            locator.GetInstance<IUrlStandardizer>().Returns(new DefaultUrlStandardizer());

            Collection = new CustomRedirectCollection();
        }

        protected void CustomRedirect(string oldUrl, string newUrl, bool exactMatch = true, bool skupWildcardAppend = false)
        {

            Collection.Add(new CustomRedirect(oldUrl, newUrl, skupWildcardAppend, exactMatch));
        }

        [TearDown]
        public void AfterEach()
        {
        }


        [Test]
        public void StandardRedirect_RedirectsToANewUrl()
        {
            CustomRedirect("http://mysite/test", "http://mysite");

            var redirect = Collection.Find(new Uri("http://mysite/test"));

            redirect.NewUrl.Should().Be("http://mysite");
        }

        [Test]
        public void StandardRedirect_RedirectsOldUrlWithTrailingSlash_ToANewUrl()
        {
            CustomRedirect("http://mysite/test", "http://mysite");

            var redirect = Collection.Find(new Uri("http://mysite/test/"));

            redirect.NewUrl.Should().Be("http://mysite");
        }

        [Test]
        public void RedirectWithTrailingSlash_RedirectsToANewUrl()
        {
            CustomRedirect("http://mysite/test/", "http://mysite");

            var redirect = Collection.Find(new Uri("http://mysite/test"));

            redirect.NewUrl.Should().Be("http://mysite");
        }
    }
}
