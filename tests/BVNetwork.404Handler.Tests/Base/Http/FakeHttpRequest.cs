using System;
using System.Collections.Specialized;
using System.Web;

namespace BVNetwork.NotFound.Tests.Base.Http
{
    public class FakeHttpRequest : HttpRequestBase
    {
        public override Uri Url { get; } = new Uri("http://example.com");
        public override NameValueCollection ServerVariables { get; } = new NameValueCollection();
        public override Uri UrlReferrer { get; } = new Uri("http://example.com/home");
    }
}