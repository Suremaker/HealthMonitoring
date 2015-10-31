using System.Web.Http.Filters;
using Common.Logging;

namespace HealthMonitoring.SelfHost.Filters
{
    class ExceptionFilter : ExceptionFilterAttribute
    {
        private static readonly ILog Logger = LogManager.GetLogger<ExceptionFilter>();
        public override void OnException(HttpActionExecutedContext ctx)
        {
            Logger.ErrorFormat("Api {0} {1} exception: {2}", ctx.Request.Method, ctx.Request.RequestUri, ctx.Exception);
        }
    }
}