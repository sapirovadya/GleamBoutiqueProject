using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GleamBoutiqueProject.Filters
{
    public class NoCacheAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            context.HttpContext.Response.Headers["Cache-Control"] = "no-store,no-cache";
            context.HttpContext.Response.Headers["Pragma"] = "no-cache";
            context.HttpContext.Response.Headers["Expires"] = "0";
            base.OnResultExecuting(context);
        }
    }
}

