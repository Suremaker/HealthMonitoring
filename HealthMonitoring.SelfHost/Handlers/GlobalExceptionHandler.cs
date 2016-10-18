using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
using HealthMonitoring.Security;
using HealthMonitoring.SelfHost.Security;

namespace HealthMonitoring.SelfHost.Handlers
{
    class GlobalExceptionHandler : ExceptionHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger<GlobalExceptionHandler>();
        public Dictionary<Type, Action<ExceptionHandlerContext>> Handlers { get; set; }

        public GlobalExceptionHandler()
        {
            Handlers = new Dictionary<Type, Action<ExceptionHandlerContext>>
            {
                {typeof(UnauthorizedAccessException),  HandleUnauthorizedAccessException},
                {typeof(AuthenticationException), HandleAuthenticationException},
                {typeof(ValidationException), HandleValidationException}
            };
        }

        public override void Handle(ExceptionHandlerContext context)
        {
            var exceptionType = context.Exception.GetType();

            Action<ExceptionHandlerContext> handler;
            if (Handlers.TryGetValue(exceptionType, out handler))
            {
                handler(context);
                return;
            }

            context.Result = new GlobalExceptionResponse(context.ExceptionContext.Request, HttpStatusCode.InternalServerError)
            {
                Message = context.Exception.Message
            };

            Logger.ErrorFormat("Api {0} {1} exception: {2}", context.Request.Method, context.Request.RequestUri, context.Exception);
        }

        public override Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            Handle(context);
            return Task.FromResult(0);
        }

        public override bool ShouldHandle(ExceptionHandlerContext context) => true;

        private void HandleUnauthorizedAccessException(ExceptionHandlerContext context)
        {
            var credentials = context.ParseAuthorizationHeader();
            var encryptedInfo = credentials != null
                ? $"{credentials.MonitorId}:{credentials.PrivateToken.ToSha256Hash()}"
                : "none";
            Logger.WarnFormat("Api {0} {1} ivalid credentials: {3}", context.Request.Method, context.Request.RequestUri, encryptedInfo);

            context.Result = new StatusCodeResult(HttpStatusCode.Forbidden, context.ExceptionContext.Request);
        }

        private void HandleAuthenticationException(ExceptionHandlerContext context)
        {
            var credentials = context.ParseAuthorizationHeader();
            Logger.WarnFormat("Api {0} {1} unauthenticated request!", context.Request.Method, context.Request.RequestUri);
            var authHeader = credentials != null
                ? context.ExceptionContext.Request.Headers.Authorization
                : new AuthenticationHeaderValue("Basic", "MonitorId:PrivateToken".ToBase64String());
            context.Result = new UnauthorizedResult(new[] { authHeader }, context.ExceptionContext.Request);
        }

        private void HandleValidationException(ExceptionHandlerContext context)
        {
            context.Result = new GlobalExceptionResponse(context.ExceptionContext.Request, HttpStatusCode.BadRequest)
            {
                Message = context.Exception.Message
            };
        }
    }

    class GlobalExceptionResponse : IHttpActionResult
    {
        public string Message { get; set; }

        public HttpStatusCode StatusCode { get; }

        public HttpRequestMessage Request { get; }

        public GlobalExceptionResponse(HttpRequestMessage request, HttpStatusCode status)
        {
            Request = request;
            StatusCode = status;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage
            {
                Content = new StringContent(Message),
                RequestMessage = Request,
                StatusCode = StatusCode
            };

            return Task.FromResult(response);
        }
    }
}
