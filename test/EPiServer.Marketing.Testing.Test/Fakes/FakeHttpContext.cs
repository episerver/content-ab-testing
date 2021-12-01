using HttpContextMoq;
using HttpContextMoq.Extensions;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;

namespace EPiServer.Marketing.Testing.Test.Fakes
{
    /// <summary>
    /// Fakes the httpcontext for unit testing
    /// </summary>
    public class FakeHttpContext
    {
        private Mock<HttpContext> _httpContextMock = new Mock<HttpContext>();

        public HttpContext Current
        {
            get
            {
                return _httpContextMock.Object;
            }
        }

        public FakeHttpContext(string url)
        {
            var uri = new Uri(url);

            var _httpRequest = new Mock<HttpRequest>();
            _httpRequest.Setup(x=>x.Path).Returns(uri.AbsolutePath);
            var requestCookieMock = new Mock<IRequestCookieCollection>();
            _httpRequest.Setup(x => x.Cookies).Returns(requestCookieMock.Object);

            _httpContextMock.Setup(x => x.Request).Returns(_httpRequest.Object);

            var sessionMock = new Mock<ISession>();
            _httpContextMock.Setup(x => x.Session).Returns(sessionMock.Object);

            var _httpResponse = new Mock<HttpResponse>();
            var responseCookieMock = new Mock<IResponseCookies>();
            _httpResponse.Setup(x => x.Cookies).Returns(responseCookieMock.Object);
            _httpContextMock.Setup(x => x.Response).Returns(_httpResponse.Object);

            _httpContextMock.Setup(x => x.Items).Returns(new Dictionary<object, object>());
        }

        public void AddCookie(string name, string value = null)
        {
            var contextMock = new HttpContextMock();
            contextMock.SetupRequestCookies(new Dictionary<string, string> {
                { name, value }
            });

            _httpContextMock.Setup(x => x.Request.Cookies).Returns(contextMock.Request.Cookies);
        }
    }
}
