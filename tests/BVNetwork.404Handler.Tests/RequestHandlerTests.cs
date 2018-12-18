using System;
using System.Web;
using BVNetwork.NotFound.Core;
using BVNetwork.NotFound.Core.Configuration;
using BVNetwork.NotFound.Core.CustomRedirects;
using BVNetwork.NotFound.Core.Data;
using BVNetwork.NotFound.Core.Logging;
using BVNetwork.NotFound.Tests.Base;
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
        private static readonly Uri DefaultOldUri = new Uri("http://example.com/old");

        public RequestHandlerTests()
        {
            _redirectHandler = A.Fake<IRedirectHandler>();
            _requestLogger = A.Fake<IRequestLogger>();
            _configuration = A.Fake<IConfiguration>();
            _sut = A.Fake<RequestHandler>(
                o => o.WithArgumentsForConstructor(new object[] { _redirectHandler, _requestLogger, _configuration })
                    .CallsBaseMethods());
            _httpContext = new FakeHttpContext();
            _httpContext.Response.StatusCode = 404;
            WhenIsRemote();
            WhenIsNotResourceFile();
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
        public void Handle_returns_when_is_off()
        {
            WhenIsOff();

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
            WhenRedirectRequestNotHandled();

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
            var redirect = new CustomRedirect("http://example.com/missing", (int)RedirectState.Saved, 1)
            {
                NewUrl = "http://example.com/page"
            };
            WhenRedirectUrlFound(redirect);

            _sut.Handle(_httpContext);

            AssertRedirected(_httpContext, redirect);
        }

        [Fact]
        public void Handle_handles_request_only_once()
        {
            WhenRedirectRequestNotHandled();

            _sut.Handle(_httpContext);
            _sut.Handle(_httpContext);

            AssertRequestHandledOnce();
        }


        [Fact]
        public void HandleRequest_returns_false_when_redirect_not_found()
        {
            WhenRedirectNotFound();

            var actual = _sut.HandleRequest(DefaultOldUri, new Uri("http://example.com/path"), out var _);

            Assert.False(actual);
        }

        [Fact]
        public void HandleRequest_logs_request_when_redirect_not_found_and_logging_is_on()
        {
            WhenRedirectNotFound();
            WhenLoggingIsOn();
            var referrer = "http://example.com/home".ToUri();
            var urlNotFound = "http://example.com/path".ToUri();

            _sut.HandleRequest(referrer, urlNotFound, out var _);

            AssertRequestLogged(referrer, urlNotFound);
        }

        [Fact]
        public void HandleRequest_doesnt_log_request_when_redirect_not_found_and_logging_is_off()
        {
            WhenRedirectNotFound();
            WhenLoggingIsOff();
            var referrer = "http://example.com/home".ToUri();
            var urlNotFound = "http://example.com/path".ToUri();

            _sut.HandleRequest(referrer, urlNotFound, out var _);

            AssertRequestNotLogged(referrer, urlNotFound);
        }

        [Fact]
        public void HandleRequest_returns_true_when_redirect_found_with_deleted_state()
        {
            var redirect = new CustomRedirect("http://example.com/missing", (int) RedirectState.Deleted, 1);
            WhenRedirectFound(redirect);

            var actual = _sut.HandleRequest(DefaultOldUri, new Uri("http://example.com/path"), out var _);

            Assert.True(actual);
        }

        [Fact]
        public void HandleRequest_returns_redirect_when_redirect_found_with_deleted_state()
        {
            var redirect = new CustomRedirect("http://example.com/missing", (int) RedirectState.Deleted, 1);
            WhenRedirectFound(redirect);

            _sut.HandleRequest(DefaultOldUri, new Uri("http://example.com/path"), out var actual);

            Assert.Equal(redirect.OldUrl, actual.OldUrl);
        }

        [Fact]
        public void HandleRequest_returns_true_when_redirect_found_with_saved_state()
        {
            var redirect = new CustomRedirect("http://example.com/found", (int) RedirectState.Saved, 1);
            WhenRedirectFound(redirect);

            var actual = _sut.HandleRequest(DefaultOldUri, new Uri("http://example.com/path"), out var _);

            Assert.True(actual);
        }

        [Fact]
        public void HandleRequest_returns_redirect_when_redirect_found_with_saved_state()
        {
            var redirect = new CustomRedirect("http://example.com/found", (int)RedirectState.Saved, 1);
            WhenRedirectFound(redirect);

            _sut.HandleRequest(DefaultOldUri, new Uri("http://example.com/path"), out var actual);

            Assert.Equal(redirect.OldUrl, actual.OldUrl);
        }

        [Fact]
        public void HandleRequest_returns_false_when_redirect_is_same_as_not_found()
        {
            var sameUri = new Uri("http://example.com/same");
            var redirect = new CustomRedirect(sameUri.ToString(), (int) RedirectState.Saved, 1)
            {
                NewUrl = sameUri.PathAndQuery
            };
            WhenRedirectFound(redirect);

            var actual = _sut.HandleRequest(DefaultOldUri, sameUri, out var _);

            Assert.False(actual);
        }

        private void WhenRedirectFound(CustomRedirect redirect)
        {
            A.CallTo(() => _redirectHandler.Find(A<Uri>._)).Returns(redirect);
        }

        private void AssertRequestNotLogged(Uri referrer, Uri urlNotFound)
        {
            A.CallTo(() => _requestLogger.LogRequest(urlNotFound.PathAndQuery, referrer.ToString()))
                .MustNotHaveHappened();
        }

        private void AssertRequestLogged(Uri referrer, Uri urlNotFound)
        {
            A.CallTo(() => _requestLogger.LogRequest(urlNotFound.PathAndQuery, referrer.ToString()))
                .MustHaveHappened();
        }

        private void WhenLoggingIsOn()
        {
            A.CallTo(() => _configuration.Logging).Returns(LoggerMode.On);
        }

        private void WhenLoggingIsOff()
        {
            A.CallTo(() => _configuration.Logging).Returns(LoggerMode.Off);
        }

        private void WhenRedirectNotFound()
        {
            A.CallTo(() => _redirectHandler.Find(A<Uri>._)).Returns(null);
        }

        private void AssertRequestHandledOnce()
        {
            CustomRedirect _;
            A.CallTo(() => _sut.HandleRequest(A<Uri>._, A<Uri>._, out _)).MustHaveHappened(Repeated.Exactly.Once);
        }

        private void WhenRedirectUrlFound(CustomRedirect redirect)
        {
            A.CallTo(() => _sut.HandleRequest(A<Uri>._, A<Uri>._, out redirect)).Returns(true);
        }

        private void WhenResourceDeleted()
        {
            var redirect = new CustomRedirect("http://example.com", (int)RedirectState.Deleted, 1);
            A.CallTo(() => _sut.HandleRequest(A<Uri>._, A<Uri>._, out redirect)).Returns(true);
        }

        private void WhenRedirectRequestNotHandled()
        {
            CustomRedirect outRedirect;
            A.CallTo(() => _sut.HandleRequest(A<Uri>._, A<Uri>._, out outRedirect)).Returns(false);
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

        private void WhenIsOff()
        {
            A.CallTo(() => _sut.IsLocalhost(A<HttpContextBase>._)).Returns(false);
            A.CallTo(() => _configuration.FileNotFoundHandlerMode).Returns(FileNotFoundMode.Off);
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