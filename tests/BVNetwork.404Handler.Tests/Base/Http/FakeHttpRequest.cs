using System;
using System.Web;

namespace BVNetwork.NotFound.Tests
{
    public partial class ErrorHandlerTests
    {
        public class FakeHttpRequest : HttpRequestBase
        {
            public override Uri Url { get; } = new Uri("http://example.com");
        }
    }
}