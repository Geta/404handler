using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using BVNetwork.NotFound.Core;
using BVNetwork.NotFound.Tests.Base.Http;
using EPiServer.Core;
using FakeItEasy;
using Xunit;

namespace BVNetwork.NotFound.Tests
{
    public class ErrorHandlerTests
    {
        private readonly ErrorHandler _sut;
        private readonly FakeHttpContext _httpContext;

        public ErrorHandlerTests()
        {
            _sut = A.Fake<ErrorHandler>(o => o.CallsBaseMethods());
            _httpContext = new FakeHttpContext();
        }

        [Fact]
        public void IsNotFoundException_is_false_when_exception_is_null()
        {
            var actual = _sut.IsNotFoundException(null, new Uri("http://example.com"));

            Assert.False(actual);
        }

        [Fact]
        public void IsNotFoundException_is_false_when_url_is_null()
        {
            var actual = _sut.IsNotFoundException(new HttpException(404, "Not found!"), null);

            Assert.False(actual);
        }

        [Fact]
        public void IsNotFoundException_is_false_when_not_404_HttpException()
        {
            var actual = _sut.IsNotFoundException(new HttpException(400, "Bad request!"), new Uri("http://example.com"));

            Assert.False(actual);
        }

        [Theory]
        [MemberData(nameof(NotFoundExceptions))]
        public void IsNotFoundException_is_true_when_expected_exception(Exception exception)
        {
            var uri = new Uri("http://example.com");

            var actual = _sut.IsNotFoundException(exception, uri);

            Assert.True(actual);
        }

        [Fact]
        public void HandleNotFoundException_sets_not_found_response_when_not_found_exception_is_thrown()
        {
            WhenNotFoundExceptionIsThrown();

            _sut.Handle(_httpContext);

            AssertNotFoundResponseSet(_httpContext);
        }

        [Fact]
        public void HandleNotFoundException_does_not_set_not_found_response_when_not_found_exception_is_not_thrown()
        {
            WhenNoNotFoundExceptionIsThrown();

            _sut.Handle(_httpContext);

            AssertNotFoundResponseNotSet(_httpContext);
        }

        [Fact]
        public void HandleNotFoundException_does_not_throw_when_context_is_null()
        {
            _sut.Handle(null);
        }

        public static IEnumerable<object[]> NotFoundExceptions => new []
        {
            new object[] {new ContentNotFoundException()},
            new object[] {new FileNotFoundException()},
            new object[] {new HttpException(404, "Not Found!")}
        };

        private void AssertNotFoundResponseNotSet(FakeHttpContext context)
        {
            Assert.False(context.Response.TrySkipIisCustomErrors);
        }

        private static void AssertNotFoundResponseSet(FakeHttpContext context)
        {
            Assert.True(context.Response.TrySkipIisCustomErrors);
            Assert.Equal(404, context.Response.StatusCode);
        }

        private void WhenNoNotFoundExceptionIsThrown()
        {
            A.CallTo(() => _sut.IsNotFoundException(A<Exception>._, A<Uri>._)).Returns(false);
        }

        private void WhenNotFoundExceptionIsThrown()
        {
            A.CallTo(() => _sut.IsNotFoundException(A<Exception>._, A<Uri>._)).Returns(true);
        }
    }
}