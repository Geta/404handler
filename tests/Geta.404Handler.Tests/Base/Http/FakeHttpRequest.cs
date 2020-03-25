using System;
using System.Collections.Specialized;
using System.Web;

namespace BVNetwork.NotFound.Tests.Base.Http
{
    public class FakeHttpRequest : HttpRequestBase
    {
        private const string QueryStringVariable = "QUERY_STRING";

        private Uri _uri = new Uri("http://example.com");
        public override Uri Url => _uri;
        public override NameValueCollection ServerVariables { get; } = new NameValueCollection();
        public override Uri UrlReferrer { get; } = new Uri("http://example.com/home");

        public FakeHttpRequest WithQueryString(string value)
        {
            ServerVariables.Add(QueryStringVariable, value);
            return this;
        }

        public FakeHttpRequest WithUrl(string expected)
        {
            _uri = string.IsNullOrEmpty(expected) ? null : new Uri(expected);
            return this;
        }
    }
}