using System;
using System.Collections.Generic;
using System.Web;
using BVNetwork.NotFound.Core;
using BVNetwork.NotFound.Core.Configuration;
using BVNetwork.NotFound.Core.CustomRedirects;
using BVNetwork.NotFound.Core.Data;
using BVNetwork.NotFound.Tests.Base;
using FakeItEasy;
using Xunit;

namespace BVNetwork.NotFound.Tests
{
    public class CustomRedirectCollectionTests
    {
        private static readonly Uri DefaultNewUri = new Uri("http://example.com/new");
        private static readonly Uri DefaultOldUri = new Uri("http://example.com/old");
        private static readonly Uri DefaultReferrer = new Uri("http://example.com/referrer");
        private static readonly string RelativeUrlWithParams = "/old?param1=value1&param2=value2";
        private readonly IConfiguration _configuration;

        private readonly CustomRedirectCollection _sut;

        public CustomRedirectCollectionTests()
        {
            _configuration = A.Fake<IConfiguration>();
            _sut = new CustomRedirectCollection(_configuration);
        }

        public static IEnumerable<object[]> OldUrls => new[]
        {
            new object[] { DefaultOldUri.AbsoluteUri, DefaultOldUri.AbsoluteUri },
            new object[] { DefaultOldUri.PathAndQuery, DefaultOldUri.PathAndQuery },
            new object[] { HttpUtility.HtmlEncode(RelativeUrlWithParams), RelativeUrlWithParams }
        };

        [Theory]
        [MemberData(nameof(OldUrls))]
        public void Find_finds_redirect(string storedUrl, string notFoundUrl)
        {
            var redirect = new CustomRedirect(storedUrl, DefaultNewUri.ToString());
            _sut.Add(redirect);

            var actual = _sut.Find(notFoundUrl.ToUri(), DefaultReferrer);

            Assert.Equal(redirect.NewUrl, actual.NewUrl);
        }

        [Fact]
        public void Find_finds_redirect_when_not_found_url_starts_with_stored_url_and_WildCardSkipAppend_enabled()
        {
            var storedUrl = "/old";
            var redirect = new CustomRedirect(storedUrl, DefaultNewUri.PathAndQuery)
            {
                WildCardSkipAppend = true
            };
            _sut.Add(redirect);

            var actual = _sut.Find($"{storedUrl}/page".ToUri(), DefaultReferrer);

            Assert.Equal(redirect.NewUrl, actual.NewUrl);
        }

        [Fact]
        public void Find_finds_redirect_and_appends_relative_path_when_not_found_url_starts_with_stored_url()
        {
            var storedUrl = "/old";
            var redirect = new CustomRedirect(storedUrl, DefaultNewUri.PathAndQuery);
            var newRedirectUrlWithPath = $"{redirect.NewUrl}/page";
            _sut.Add(redirect);

            var actual = _sut.Find($"{storedUrl}/page".ToUri(), DefaultReferrer);

            Assert.Equal(newRedirectUrlWithPath, actual.NewUrl);
        }

        [Fact]
        public void Find_doesnt_find_redirect_when_not_found_url_starts_with_stored_url_and_redirect_is_ignored()
        {
            var storedUrl = "/old";
            var redirect = new CustomRedirect(storedUrl, DefaultNewUri.PathAndQuery)
            {
                State = (int)DataStoreHandler.State.Ignored
            };
            _sut.Add(redirect);

            var actual = _sut.Find($"{storedUrl}/page".ToUri(), DefaultReferrer);

            Assert.Null(actual);
        }

        [Fact]
        public void Find_finds_redirect_in_provider()
        {
            var oldUrl = "http://example.com/old";
            var newUrl = "http://example.com/new";
            WithProvider(oldUrl, newUrl);

            var actual = _sut.Find(oldUrl.ToUri(), DefaultReferrer);

            Assert.Equal(newUrl, actual.NewUrl);
        }

        [Fact]
        public void Find_does_not_find_redirect_in_provider_when_not_found_in_provider()
        {
            var oldUrl = "http://example.com/old";
            WithProvider(oldUrl, null);

            var actual = _sut.Find(oldUrl.ToUri(), DefaultReferrer);

            Assert.Null(actual);
        }

        // Regression tests

        /// <summary>
        /// https://github.com/Geta/404handler/issues/46
        /// </summary>
        [Theory]
        [InlineData("http://localhost:80/en/about-us/news-events-wrong-word")]
        [InlineData("http://localhost:80/en/about-us/news-events/wrong-page")]
        public void Find_should_not_cause_redirect_loop(string notFoundUrl)
        {
            var redirect = new CustomRedirect("/en/about-us/", "/en/about-us/news-events");
            _sut.Add(redirect);

            var first = _sut.Find(notFoundUrl.ToUri(), DefaultReferrer);
            var second = _sut.Find(first.NewUrl.ToUri(), notFoundUrl.ToUri());

            Assert.Null(second);
        }

        private void WithProvider(string oldUrl, string newUrl)
        {
            var provider = A.Fake<INotFoundHandler>();
            A.CallTo(() => provider.RewriteUrl(A<string>._)).Returns(null);
            A.CallTo(() => provider.RewriteUrl(oldUrl)).Returns(newUrl);
            A.CallTo(() => _configuration.Providers).Returns(new[] {provider});
        }
    }
}