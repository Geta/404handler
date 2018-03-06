using BVNetwork.NotFound.Core.NotFoundPage;
using BVNetwork.NotFound.Tests.Base.Http;
using Xunit;

namespace BVNetwork.NotFound.Tests.NotFoundPage
{
    public class NotFoundPageUtilTests
    {

        [Fact]
        public void GetUrlNotFound_returns_null_when_no_query_string_provided()
        {
            var request = new FakeHttpRequest();

            var actual = NotFoundPageUtil.GetUrlNotFound(request);

            Assert.Null(actual);
        }

        [Fact]
        public void GetUrlNotFound_returns_null_when_query_string_is_null()
        {
            var request = new FakeHttpRequest().WithQueryString(null);

            var actual = NotFoundPageUtil.GetUrlNotFound(request);

            Assert.Null(actual);
        }

        [Fact]
        public void GetUrlNotFound_returns_null_when_query_starts_with_aspxerrorpath_and_url_is_null()
        {
            var request = new FakeHttpRequest().WithUrl(null).WithQueryString("aspxerrorpath=/missing");

            var actual = NotFoundPageUtil.GetUrlNotFound(request);

            Assert.Null(actual);
        }

        [Theory]
        [InlineData("404;http://mytest.localtest.me/missing", "http://mytest.localtest.me/missing")]
        [InlineData("aspxerrorpath=/missing", "http://mytest.localtest.me/missing")]
        public void GetUrlNotFound_returns_url(string queryString, string expected)
        {
            var request = new FakeHttpRequest().WithUrl(expected).WithQueryString(queryString);

            var actual = NotFoundPageUtil.GetUrlNotFound(request);

            Assert.Equal(expected, actual);
        }
    }
}