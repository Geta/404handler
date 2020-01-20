using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BVNetwork.NotFound.Tests.Base.Http
{
    public class FakeHttpServerUtility : HttpServerUtilityBase
    {
        public readonly IList<Exception> Errors = new List<Exception>
        {
            new Exception()
        };

        public override Exception GetLastError()
        {
            return Errors.FirstOrDefault();
        }

        public override void ClearError()
        {
            Errors.Clear();
        }
    }
}