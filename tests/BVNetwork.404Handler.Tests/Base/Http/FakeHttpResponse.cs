using System.Web;

namespace BVNetwork.NotFound.Tests.Base.Http
{
    public class FakeHttpResponse : HttpResponseBase
    {
        public override bool TrySkipIisCustomErrors { get; set; }
        public override void Clear() { }
        public override int StatusCode { get; set; }
    }
}