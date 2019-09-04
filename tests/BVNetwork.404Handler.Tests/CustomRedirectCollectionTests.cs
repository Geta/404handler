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
        private static readonly string RelativeUrlWithParams = "/old?param1=value1&param2=value2";
        private static readonly string RelativeUrlWithSlashAndParams = "/old/?param1=value1&param2=value2";
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
            new object[] { HttpUtility.HtmlEncode(RelativeUrlWithParams), RelativeUrlWithParams },
            new object[] { HttpUtility.HtmlEncode(RelativeUrlWithSlashAndParams), RelativeUrlWithSlashAndParams }
        };

        [Theory]
        [MemberData(nameof(OldUrls))]
        public void Find_finds_redirect(string storedUrl, string notFoundUrl)
        {
            var redirect = new CustomRedirect(storedUrl, DefaultNewUri.ToString());
            _sut.Add(redirect);

            var actual = _sut.Find(notFoundUrl.ToUri());

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

            var actual = _sut.Find($"{storedUrl}/page".ToUri());

            Assert.Equal(redirect.NewUrl, actual.NewUrl);
        }

        [Fact]
        public void Find_finds_most_specific_redirect_when_not_found_url_starts_with_stored_url()
        {
            var collection = new CustomRedirectCollection
            {
                new CustomRedirect("/oldsegment1/", "/newsegment1/"),
                new CustomRedirect("/oldsegment1/oldsegment2/", "/newsegment1/newsegment2/"),
            };

            var urlToFind = "/oldsegment1/oldsegment2/?test=q";
            var expected = "/newsegment1/newsegment2/?test=q";

            var actual = collection.Find(urlToFind.ToUri());

            Assert.Equal(expected, actual.NewUrl);
        }

        [Fact]
        public void Find_uses_exact_protocol_and_domain_match_for_old_absolute_urls()
        {
            var collection = new CustomRedirectCollection
            {
                new CustomRedirect("http://domain.com/oldurl", "/newurl1_http_absolute/"),
                new CustomRedirect("http://domain.com/oldurl-http", "/newurl2_http_absolute/"),
                new CustomRedirect("https://domain.com/oldurl", "/newurl1_https_absolute/"),
                new CustomRedirect("https://domain.com/oldurl-https", "/newurl2_https_absolute/"),
            };

            Assert.Equal("/newurl1_http_absolute/", collection.Find("http://domain.com/oldurl"));
            Assert.Equal("/newurl1_http_absolute/additional_segment/", collection.Find("http://domain.com/oldurl/additional_segment/"));
            Assert.Null(collection.Find("http://another-domain.com/oldurl"));

            Assert.Equal("/newurl1_https_absolute/", collection.Find("https://domain.com/oldurl"));
            Assert.Equal("/newurl1_https_absolute/additional_segment/", collection.Find("https://domain.com/oldurl/additional_segment/"));
            Assert.Null(collection.Find("https://another-domain.com/oldurl"));

            Assert.Equal("/newurl2_http_absolute/", collection.Find("http://domain.com/oldurl-http"));
            Assert.Null(collection.Find("https://domain.com/oldurl-http"));

            Assert.Equal("/newurl2_https_absolute/", collection.Find("https://domain.com/oldurl-https"));
            Assert.Null(collection.Find("http://domain.com/oldurl-https"));
        }

        // TODO: order of parameters in query string should not be respected during match
        [Fact]
        public void Find_uses_exact_match_for_query_parameters_order()
        {
            var collection = new CustomRedirectCollection
            {
                new CustomRedirect("/oldurl?q1=value1&q2=value2", "/newurl"),
            };

            Assert.Equal("/newurl", collection.Find("/oldurl?q1=value1&q2=value2"));
            Assert.Null(collection.Find("/oldurl?name=value&q1=value1&q2=value2"));
            Assert.Null(collection.Find("/oldurl?q2=value2&q1=value1"));
        }

        [Fact]
        public void Find_appends_additional_params_during_pattern_matching_with_query()
        {
            var collection = new CustomRedirectCollection
            {
                new CustomRedirect("/oldurl1", "/newurl1"),
                new CustomRedirect("/oldurl1?q1=value1", "/newurl2?newparam=value"),
                new CustomRedirect("/oldurl1?q1=value1&q2=value2", "/newurl3"),
            };

            // Exact matches
            Assert.Equal("/newurl1", collection.Find("/oldurl1"));
            Assert.Equal("/newurl2?newparam=value", collection.Find("/oldurl1?q1=value1"));
            Assert.Equal("/newurl3", collection.Find("/oldurl1?q1=value1&q2=value2"));

            // Pattern matching in query

            // TODO: Incorrectly appends `/` before `?` now
            Assert.Equal("/newurl1/?name=value", collection.Find("/oldurl1?name=value"));

            // TODO: Incorrectly appends `/&` instead of `&`
            Assert.Equal("/newurl2?newparam=value/&q3=value3", collection.Find("/oldurl1?q1=value1&q3=value3"));

            // TODO: Incorrectly starts query with `/&` instead of `?`
            Assert.Equal("/newurl3/&q3=value", collection.Find("/oldurl1?q1=value1&q2=value2&q3=value"));
        }

        [Fact]
        public void Find_doesnot_find_url_with_the_same_segment_start()
        {
            var collection = new CustomRedirectCollection
            {
                new CustomRedirect("/oldurl", "/newurl"),
            };

            Assert.Equal("/newurl", collection.Find("/oldurl"));
            Assert.Null(collection.Find("/oldurl_segment"));
        }

        [Fact]
        public void Find_doesnot_find_old_url_without_slash_at_the_start()
        {
            var collection = new CustomRedirectCollection
            {
                new CustomRedirect("oldurl", "newurl"),
            };

            // TODO: should not be like this - either disable adding in UI (migration needed?) or treat that as "usual" relative URL
            Assert.Null(collection.Find("http://domain.com/oldurl"));
        }

        [Fact]
        public void Find_doesnot_append_leading_slash_to_new_relative_url()
        {
            var collection = new CustomRedirectCollection
            {
                new CustomRedirect("/oldurl", "newurl"),
            };

            // TODO: while it is allowed, having NEW URL to NOT start with slash make not much sense -
            // it will produce "relative" redirect, so new url in client's browser will always be /oldurl/newurl
            Assert.Equal("newurl", collection.Find("/oldurl"));
            Assert.Equal("newurl/segment", collection.Find("/oldurl/segment"));
        }

        // TODO: URL segments are case sensitive by RFC - need a setting on CustomRedirect level of how that should be treated?
        [Fact]
        public void Find_ignores_url_case()
        {
            var collection = new CustomRedirectCollection
            {
                new CustomRedirect("/oldurl", "/newurl"),
            };

            Assert.Equal("/newurl", collection.Find("http://domain.com/oldurl"));
            Assert.Equal("/newurl", collection.Find("http://domain.com/OldUrl"));
            Assert.Equal("/newurl", collection.Find("http://domain.com/OLDURL"));
        }

        // TODO: query case (both params and values) are case sensitive by RFC - need a setting on CustomRedirect level of how that should be treated?
        [Fact]
        public void Find_ignores_query_case()
        {
            var collection = new CustomRedirectCollection
            {
                new CustomRedirect("/oldurl?query1=test", "/newurl_query_without_slash"),
                new CustomRedirect("/oldurl/?query2=test", "/newurl_query_with_slash"),
            };

            Assert.Null(collection.Find(new Uri("http://domain.com/oldurl")));
            Assert.Equal("/newurl_query_without_slash", collection.Find("http://domain.com/oldurl?query1=test"));
            Assert.Equal("/newurl_query_without_slash", collection.Find("http://domain.com/oldurl?QuErY1=TeSt"));
            Assert.Equal("/newurl_query_with_slash", collection.Find("http://domain.com/oldurl/?query2=test"));
            Assert.Equal("/newurl_query_with_slash", collection.Find("http://domain.com/oldurl/?QuErY2=TeSt"));
        }

        [Fact]
        public void Find_finds_redirect_and_appends_relative_path_when_not_found_url_starts_with_stored_url()
        {
            var storedUrl = "/old";
            var redirect = new CustomRedirect(storedUrl, DefaultNewUri.PathAndQuery);
            var newRedirectUrlWithPath = $"{redirect.NewUrl}/page";
            _sut.Add(redirect);

            var actual = _sut.Find($"{storedUrl}/page".ToUri());

            Assert.Equal(newRedirectUrlWithPath, actual.NewUrl);
        }

        [Fact]
        public void Find_doesnt_find_redirect_when_not_found_url_starts_with_stored_url_and_redirect_is_ignored()
        {
            var storedUrl = "/old";
            var redirect = new CustomRedirect(storedUrl, DefaultNewUri.PathAndQuery)
            {
                State = (int)RedirectState.Ignored
            };
            _sut.Add(redirect);

            var actual = _sut.Find($"{storedUrl}/page".ToUri());

            Assert.Null(actual);
        }

        [Fact]
        public void Find_finds_redirect_in_provider()
        {
            var oldUrl = "http://example.com/old";
            var newUrl = "http://example.com/new";
            WithProvider(oldUrl, newUrl);

            var actual = _sut.Find(oldUrl.ToUri());

            Assert.Equal(newUrl, actual.NewUrl);
        }

        [Fact]
        public void Find_does_not_find_redirect_in_provider_when_not_found_in_provider()
        {
            var oldUrl = "http://example.com/old";
            WithProvider(oldUrl, null);

            var actual = _sut.Find(oldUrl.ToUri());

            Assert.Null(actual);
        }

        // Regression tests

        /// <summary>
        /// https://github.com/Geta/404handler/issues/46
        /// </summary>
        [Theory]
        [InlineData("/en/about-us/", "/en/about-us/news-events", "http://localhost:80/en/about-us/news-events-wrong-word",
                    "/en/about-us/news-events/news-events-wrong-word")]
        [InlineData("/en/about-us/", "/en/about-us/news-events", "http://localhost:80/en/about-us/news-events/wrong-page",
                    "/en/about-us/news-events/news-events/wrong-page")]
        public void Find_should_append_sub_segment(string fromUrl, string toUrl, string notFoundUrl, string expected)
        {
            var redirect = new CustomRedirect(fromUrl, toUrl);
            _sut.Add(redirect);

            var actual = _sut.Find(notFoundUrl.ToUri());

            Assert.Equal(expected, actual.NewUrl);
        }

        /// <summary>
        /// https://github.com/Geta/404handler/issues/90
        /// </summary>
        [Theory]
        [InlineData("/resources/b", "/resources/blog", "http://localhost:80/resources/blog/thisisablog")]
        [InlineData("/product/health", "/product/health-insurance", "http://localhost:80/product/health-insurance-article/what-hipaa-and-how-will-it-affect-your-small-business?q=article/what-hipaa-and-how-will-it-affect-your-small-business")]
        [InlineData("/resources/articles/smal", "/resources/articles/small-business", "http://localhost:80/resources/articles/small-business/understanding-financial-statements")]
        [InlineData("/bar", "/blog", "http://localhost:80/barr")]
        public void Find_should_not_cause_redirect_loop(string fromUrl, string toUrl, string notFoundUrl)
        {
            var redirect = new CustomRedirect(fromUrl, toUrl);
            _sut.Add(redirect);

            var actual = _sut.Find(notFoundUrl.ToUri());

            Assert.Null(actual);
        }

        /// <summary>
        /// https://github.com/Geta/404handler/issues/38
        /// </summary>
        [Fact]
        public void Add_allows_similar_culture_specific_urls()
        {
            var first = new CustomRedirect("/digtalpaent", "/");
            _sut.Add(first);
            var second = new CustomRedirect("/digtalpænt", "/");

            var ex = Record.Exception(() => _sut.Add(second));

            Assert.Null(ex);
        }

        /// <summary>
        /// https://github.com/Geta/404handler/issues/36
        /// </summary>
        [Fact]
        public void Find_does_not_add_trailing_slash()
        {
            var expected = "https://domain.mydomain.dk/thisismypdf.pdf";
            var redirect = new CustomRedirect("/something", expected);
            _sut.Add(redirect);

            var actual = _sut.Find(redirect.OldUrl.ToUri());

            Assert.Equal(expected, actual.NewUrl);
        }

        [Fact]
        public void Find_finds_redirect_ignoring_query_string()
        {
            var requestUrl = "/old/?param1=value1&param2=value2";
            var storedUrl = "/old/";
            var redirect = new CustomRedirect(storedUrl, DefaultNewUri.PathAndQuery);
            _sut.Add(redirect);

            var actual = _sut.Find(requestUrl.ToUri());

            Assert.NotNull(actual);
        }

        [Fact]
        public void Find_finds_redirect_with_sub_segment_ignoring_query_string()
        {
            var requestUrl = "/old/segment/?param1=value1&param2=value2";
            var storedUrl = "/old/";
            var redirect = new CustomRedirect(storedUrl, DefaultNewUri.PathAndQuery);
            _sut.Add(redirect);

            var actual = _sut.Find(requestUrl.ToUri());

            Assert.NotNull(actual);
        }

        private void WithProvider(string oldUrl, string newUrl)
        {
            var provider = A.Fake<INotFoundHandler>();
            A.CallTo(() => provider.RewriteUrl(A<string>._)).Returns(null);
            A.CallTo(() => provider.RewriteUrl(oldUrl)).Returns(newUrl);
            A.CallTo(() => _configuration.Providers).Returns(new[] { provider });
        }

        /// <summary>
        /// https://github.com/Geta/404handler/issues/117
        /// </summary>
        [Fact]
        public void Find_should_not_throw_on_query_string_prefix_for_same_segment()
        {
            var requestUrl = "/old/?param1=value1&param2=value2";
            var storedUrl = "/old/?param1=value1";
            var redirect = new CustomRedirect(storedUrl, DefaultNewUri.PathAndQuery);
            _sut.Add(redirect);

            var actual = _sut.Find(requestUrl.ToUri());

            Assert.NotNull(actual);
        }

        /// <summary>
        /// https://github.com/Geta/404handler/issues/118
        /// </summary>
        [Fact]
        public void Find_does_not_give_nullreferenceexception_with_exact_deleted_hit()
        {
            var storedUrl = "/contentassets";
            var requesttUrl = "/contentassets";
            var redirect = new CustomRedirect(storedUrl, (int)RedirectState.Deleted, 1);
            _sut.Add(redirect);

            var actual = _sut.Find(requesttUrl.ToUri());

            Assert.NotNull(actual);
        }

        [Fact]
        public void Find_gives_nullreferenceexception_with_partial_deleted_hit()
        {
            var storedUrl = "/contentassets";
            var requesttUrl = "/contentassets/moreurl";
            var redirect = new CustomRedirect(storedUrl, (int)RedirectState.Deleted, 1);
            _sut.Add(redirect);

            var actual = _sut.Find(requesttUrl.ToUri());

            Assert.NotNull(actual);
        }
    }
}