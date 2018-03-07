using System.Collections;
using System.Collections.Generic;
using System.Web;

namespace BVNetwork.NotFound.Tests.Base.Http
{
    public class FakeHttpContext : HttpContextBase
    {
        public override HttpRequestBase Request { get; } = new FakeHttpRequest();
        public override HttpResponseBase Response { get; } = new FakeHttpResponse();
        public override HttpServerUtilityBase Server { get; } = new FakeHttpServerUtility();
        public override IDictionary Items { get; } = new Dictionary<string, object>();
    }
}