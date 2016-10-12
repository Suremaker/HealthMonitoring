using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;
using Common.Logging;

namespace HealthMonitoring.SelfHost.Handlers
{
    class GlobalExceptionHandler : ExceptionHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger<GlobalExceptionHandler>();

        public override void Handle(ExceptionHandlerContext context)
        {
            var exceptionType = context.Exception.GetType();
            if (exceptionType == typeof(UnauthorizedAccessException))
            {
                Logger.WarnFormat("Api {0} {1} ivalid credentials: {3}", context.Request.Method, context.Request.RequestUri, context.Request.Headers.Authorization);
                context.Result = new StatusCodeResult(HttpStatusCode.Forbidden, context.ExceptionContext.Request);
            }
            else if (exceptionType == typeof(AuthenticationException))
            {
                Logger.WarnFormat("Api {0} {1} unauthenticated request!", context.Request.Method, context.Request.RequestUri);
                context.Result = new StatusCodeResult(HttpStatusCode.Unauthorized, context.ExceptionContext.Request);
            }
            else
            {
                context.Result = new GlobalExceptionResponse
                {
                    Message = context.Exception.Message,
                    Request = context.ExceptionContext.Request
                };

                Logger.ErrorFormat("Api {0} {1} exception: {2}", context.Request.Method, context.Request.RequestUri, context.Exception);
            }
        }

        public override Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            Handle(context);
            return Task.FromResult(0);
        }

        public override bool ShouldHandle(ExceptionHandlerContext context) => true;
    }

    class GlobalExceptionResponse : IHttpActionResult
    {
        public string Message { get; set; }

        public HttpRequestMessage Request { get; set; }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage
            {
                Content = new StringContent(Message),
                RequestMessage = Request,
                StatusCode = HttpStatusCode.InternalServerError
            };

            return Task.FromResult(response);
        }
    }

    class UnauthorizedExceptionResponse : IHttpActionResult
    {
        private HttpRequestMessage Request { get; }

        public UnauthorizedExceptionResponse(HttpRequestMessage request)
        {
            Request = request;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            var credentials = Request.Headers.Authorization ?? new AuthenticationHeaderValue("Basic");

            response.Headers.WwwAuthenticate.Add(credentials);
            response.Content = new StringContent("Invalid monitor credentials!");
            return Task.FromResult(response);
        }
    }
}
