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
            return new RedirectsClause(this, oldUrl);
        }

        public class RedirectsClause
        {
            private string OldUrl { get; }
            private CustomRedirect_spec Spec { get; }

            public RedirectsClause(CustomRedirect_spec spec, string oldUrl)
            {
                OldUrl = oldUrl;
                Spec = spec;
            }

            public void ThenItRedirectsTo(string newUrl)
            {
                Spec.CheckRedirect(OldUrl, newUrl);
            }

            public void ThenItDoesNotRedirect()
            {
                Spec.CheckNoRedirect(OldUrl);
            }
        }

        CustomRedirect FindRedirect(string oldUrl)
        {
            return redirects.Find(new Uri(oldUrl));
        }

        private void CheckRedirect(string oldUrl, string newUrl)
        {
            it[$"when url is '{oldUrl}' then it's redirected to '{newUrl}'"] = () =>
            {
                var customRedirect = FindRedirect(oldUrl);

                (customRedirect?.NewUrl ?? "<no redirect>").Should().Be(newUrl);
            };
        }

        private void CheckNoRedirect(string oldUrl)
        {
            it[$"when url is '{oldUrl}' then it does not redirect"] = () =>
            {
                var customRedirect = FindRedirect(oldUrl);

                customRedirect.Should().BeNull("it shouldn't redirect");
            };
        }
    }
}
