using System;
using System.Web;
using BVNetwork.NotFound.Core;
using BVNetwork.NotFound.Core.Configuration;
using BVNetwork.NotFound.Core.CustomRedirects;
using BVNetwork.NotFound.Core.Data;
using BVNetwork.NotFound.Core.Logging;
using BVNetwork.NotFound.Tests.Base.Http;
using FakeItEasy;
using Xunit;

namespace BVNetwork.NotFound.Tests
{
    public class RequestHandlerTests
    {
        private readonly IRedirectHandler _redirectHandler;
        private readonly IRequestLogger _requestLogger;
        private readonly IConfiguration _configuration;
        private readonly FakeHttpContext _httpContext;
        private readonly RequestHandler _sut;

        public RequestHandlerTests()
        {
            _redirectHandler = A.Fake<IRedirectHandler>();
            _requestLogger = A.Fake<IRequestLogger>();
            _configuration = A.Fake<IConfiguration>();
            _sut = A.Fake<RequestHandler>(
                o => o.WithArgumentsForConstructor(new object[] {_redirectHandler, _requestLogger, _configuration})
                    .CallsBaseMethods());
            _httpContext = new FakeHttpContext();
            _httpContext.Response.StatusCode = 404;
            WhenIsRemote();
            WhenIsNotResourceFile();
            WhenHasReferrer();
        }

        [Fact]
        public void Handle_returns_when_response_is_not_404()
        {
            _httpContext.Response.StatusCode = 400;

            _sut.Handle(_httpContext);

            AssertNotHandled(_httpContext);
        }

        [Fact]
        public void Handle_returns_when_is_localhost()
        {
            WhenIsLocalhost();

            _sut.Handle(_httpContext);

            AssertNotHandled(_httpContext);
        }

        [Fact]
        public void Handle_returns_when_is_resource_file()
        {
            WhenIsResourceFile();

            _sut.Handle(_httpContext);

            AssertNotHandled(_httpContext);
        }

        [Fact]
        public void Handle_returns_when_query_string_variable_is_404()
        {
            WhenQueryStringValueIs404();

            _sut.Handle(_httpContext);

            AssertNotHandled(_httpContext);
        }

        [Fact]
        public void Handle_sets_404_response_when_unable_redirect()
        {
            WhenUnableRedirect();

            _sut.Handle(_httpContext);

            Assert404ResponseSet(_httpContext);
        }

        [Fact]
        public void Handle_sets_410_response_when_resource_deleted()
        {
            WhenResourceDeleted();

            _sut.Handle(_httpContext);

            Assert410ResponseSet(_httpContext);
        }

        [Fact]
        public void Handle_redirects_when_redirect_url_found()
        {
            var redirect = new CustomRedirect("http://example.com/missing", (int)DataStoreHandler.State.Saved, 1)
            {
                NewUrl = "http://example.com/page"
            };
            WhenRedirectUrlFound(redirect);

            _sut.Handle(_httpContext);

            AssertRedirected(_httpContext, redirect);
        }

        private void WhenRedirectUrlFound(CustomRedirect redirect)
        {
            A.CallTo(() => _sut.HandleRequest(A<string>._, A<Uri>._, out redirect)).Returns(true);
        }

        private void WhenResourceDeleted()
        {
            var redirect = new CustomRedirect("http://example.com", (int)DataStoreHandler.State.Deleted, 1);
            A.CallTo(() => _sut.HandleRequest(A<string>._, A<Uri>._, out redirect)).Returns(true);
        }

        private void WhenHasReferrer()
        {
            A.CallTo(() => _sut.GetReferer(A<Uri>._)).Returns("http://example.com/home");
        }

        private void WhenUnableRedirect()
        {
            CustomRedirect outRedirect;
            A.CallTo(() => _sut.HandleRequest(A<string>._, A<Uri>._, out outRedirect)).Returns(false);
        }

        private void WhenQueryStringValueIs404()
        {
            _httpContext.Request.ServerVariables["QUERY_STRING"] = "404;";
        }

        private void WhenIsResourceFile()
        {
            A.CallTo(() => _sut.IsResourceFile(A<Uri>._)).Returns(true);
        }

        private void WhenIsNotResourceFile()
        {
            A.CallTo(() => _sut.IsResourceFile(A<Uri>._)).Returns(false);
        }

        private void WhenIsLocalhost()
        {
            A.CallTo(() => _sut.IsLocalhost(A<HttpContextBase>._)).Returns(true);
            A.CallTo(() => _configuration.FileNotFoundHandlerMode).Returns(FileNotFoundMode.RemoteOnly);
        }

        private void WhenIsRemote()
        {
            A.CallTo(() => _sut.IsLocalhost(A<HttpContextBase>._)).Returns(false);
            A.CallTo(() => _configuration.FileNotFoundHandlerMode).Returns(FileNotFoundMode.On);
        }

        private void AssertRedirected(FakeHttpContext context, CustomRedirect redirect)
        {
            Assert.True(context.Response.TrySkipIisCustomErrors);
            Assert.NotEqual(404, context.Response.StatusCode);
            Assert.Equal(redirect.NewUrl, context.Response.RedirectLocation);
        }

        private void Assert404ResponseSet(HttpContextBase context)
        {
            Assert.True(context.Response.TrySkipIisCustomErrors);
            Assert.Equal(404, context.Response.StatusCode);
        }

        private void Assert410ResponseSet(HttpContextBase context)
        {
            Assert.True(context.Response.TrySkipIisCustomErrors);
            Assert.Equal(410, context.Response.StatusCode);
        }

        private void AssertNotHandled(HttpContextBase context)
        {
            Assert.False(context.Response.TrySkipIisCustomErrors);
        }
    }
}