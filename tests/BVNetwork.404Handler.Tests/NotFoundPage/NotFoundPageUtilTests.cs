using BVNetwork.NotFound.Core.NotFoundPage;
using BVNetwork.NotFound.Tests.Base.Http;
using Xunit;

namespace BVNetwork.NotFound.Tests.NotFoundPage
{
    public class NotFoundPageUtilTests
    {

        [Fact]
        public void GetUrlNotFound_returns_empty_string_when_no_query_string_provided()
        {
            var request = new FakeHttpRequest();

            var actual = NotFoundPageUtil.GetUrlNotFound(request);

            Assert.Equal(string.Empty, actual);
        }

        [Fact]
        public void GetUrlNotFound_returns_empty_string_when_query_string_is_null()
        {
            var request = new FakeHttpRequest().WithQueryString(null);

            var actual = NotFoundPageUtil.GetUrlNotFound(request);

            Assert.Equal(string.Empty, actual);
        }

        [Fact]
        public void GetUrlNotFound_returns_empty_string_when_query_starts_with_aspxerrorpath_and_url_is_null()
        {
            var request = new FakeHttpRequest().WithUrl(null).WithQueryString("aspxerrorpath=/missing");

            var actual = NotFoundPageUtil.GetUrlNotFound(request);

            Assert.Equal(string.Empty, actual);
        }

        [Theory]
        [InlineData("404;http://mytest.localtest.me/missing", "http://mytest.localtest.me/missing")]
        [InlineData("410;http://mytest.localtest.me/missing", "http://mytest.localtest.me/missing")]
        [InlineData("aspxerrorpath=/missing", "http://mytest.localtest.me/missing")]
        public void GetUrlNotFound_returns_url(string queryString, string expected)
        {
            var request = new FakeHttpRequest().WithUrl(expected).WithQueryString(queryString);

            var actual = NotFoundPageUtil.GetUrlNotFound(request);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("300;http://mytest.localtest.me/something")]
        [InlineData("400;http://mytest.localtest.me/something")]
        [InlineData("401;http://mytest.localtest.me/something")]
        [InlineData("40;http://mytest.localtest.me/something")]
        public void GetUrlNotFound_returns_empty_string_when_query_does_not_start_with_404_or_410(string queryString)
        {
            var request = new FakeHttpRequest().WithQueryString(queryString);

            var actual = NotFoundPageUtil.GetUrlNotFound(request);

            Assert.Equal(string.Empty, actual);
        }

        [Theory]
        [InlineData("404;")]
        [InlineData("404")]
        public void GetUrlNotFound_returns_empty_string_when_query_has_no_url_after_error_code(string queryString)
        {
            var request = new FakeHttpRequest().WithQueryString(queryString);

            var actual = NotFoundPageUtil.GetUrlNotFound(request);

            Assert.Equal(string.Empty, actual);
        }

        [Fact]
        public void GetUrlNotFound_returns_empty_string_when_query_aspxerrorpath_is_empty()
        {
            var request = new FakeHttpRequest().WithUrl("http://mysite.localtest.me/missing").WithQueryString("aspxerrorpath=");

            var actual = NotFoundPageUtil.GetUrlNotFound(request);

            Assert.Equal(string.Empty, actual);
        }
    }
}