using System.Web;

namespace BVNetwork.NotFound.Tests.Base.Http
{
    public class FakeHttpResponse : HttpResponseBase
    {
        public override bool TrySkipIisCustomErrors { get; set; }
        public override void Clear() { }
        public override int StatusCode { get; set; }
        public override string RedirectLocation { get; set; }

        public override void End()
        {
        }

        public override void RedirectPermanent(string url)
        {
            RedirectLocation = url;
            StatusCode = 301;
        }

        public override void RedirectPermanent(string url, bool endResponse)
        {
            RedirectLocation = url;
            StatusCode = 301;
        }
    }
}