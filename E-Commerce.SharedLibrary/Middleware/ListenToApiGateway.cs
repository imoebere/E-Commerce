﻿using Microsoft.AspNetCore.Http;

namespace E_Commerce.SharedLibrary.Middleware
{
    public class ListenToApiGateway(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            // Extract specific header from the request
            var signedHeader = context.Request.Headers["Api-Gateway"];

            // Null means, the request is not coming from the Api Gateway 
            // 503 Service Unavailable
            if (signedHeader.FirstOrDefault() is null)
            {
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                await context.Response.WriteAsync("Sorry! Service is Unavailable");
                return;
            }
            else
            {
                await next(context);
            }
        }
    }
}
