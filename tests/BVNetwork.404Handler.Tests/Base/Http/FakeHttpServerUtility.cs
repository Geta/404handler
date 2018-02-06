using System;
using System.Web;

namespace BVNetwork.NotFound.Tests
{
    public partial class ErrorHandlerTests
    {
        public class FakeHttpServerUtility : HttpServerUtilityBase
        {
            public override Exception GetLastError()
            {
                return new Exception();
            }
        }
    }
}