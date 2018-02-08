using System;
using System.Web;

namespace BVNetwork.NotFound.Tests.Base.Http
{
    public class FakeHttpServerUtility : HttpServerUtilityBase
    {
        public override Exception GetLastError()
        {
            return new Exception();
        }

        public override void ClearError()
        {
        }
    }
}