using System;
using System.Web;
using System.Web.Routing;

namespace WebProxy
{
    internal class AllRoutes : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new AllHandler();
        }
    }
}