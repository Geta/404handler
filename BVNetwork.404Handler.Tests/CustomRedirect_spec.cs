using System;
using BVNetwork.NotFound.Core;
using BVNetwork.NotFound.Core.CustomRedirects;
using FluentAssertions;
using NSpec;

namespace BVNetwork.NotFound
{
    [Tag("CustomRedirects")]
    public class CustomRedirect_spec : nspec
    {
        protected CustomRedirectCollection redirects;
        protected void before_all()
        {
            UrlStandardizer.Accessor = () => new DefaultUrlStandardizer();
        }

        protected RedirectsClause WhenUrlIs(string oldUrl)
        {
            return new RedirectsClause(redirects.Find(new Uri(oldUrl)));
        }

        public class RedirectsClause
        {
            private CustomRedirect _customRedirect;

            public RedirectsClause(CustomRedirect customRedirect)
            {
                _customRedirect = customRedirect;
            }

            public void ThenItRedirectsTo(string newUrl)
            {
                (_customRedirect?.NewUrl ?? "<no redirect>").Should().Be(newUrl);
            }

            public void ThenItDoesNotRedirect()
            {
                _customRedirect.Should().Be(null, "it shouldn't redirect");
            }
        }
    }
}
