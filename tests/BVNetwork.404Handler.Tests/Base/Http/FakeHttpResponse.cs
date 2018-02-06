using System.Web;

namespace BVNetwork.NotFound.Tests
{
    public partial class ErrorHandlerTests
    {
        public class FakeHttpResponse : HttpResponseBase
        {
            public override bool TrySkipIisCustomErrors { get; set; }
            public override void Clear() { }
            public override int StatusCode { get; set; }
        }
    }
}