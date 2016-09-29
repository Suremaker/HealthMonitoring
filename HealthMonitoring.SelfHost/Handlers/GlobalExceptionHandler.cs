using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Common.Logging;

namespace HealthMonitoring.SelfHost.Handlers
{
    class GlobalExceptionHandler : ExceptionHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger<GlobalExceptionHandler>();

        public override void Handle(ExceptionHandlerContext context)
        {
            context.Result = new GlobalExceptionResponse
            {
                Message = context.Exception.Message,
                Request = context.ExceptionContext.Request
            };

            Logger.ErrorFormat("Api {0} {1} exception: {2}", context.Request.Method, context.Request.RequestUri, context.Exception);
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
}
