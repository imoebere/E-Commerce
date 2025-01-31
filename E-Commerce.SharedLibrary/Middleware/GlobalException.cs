using E_Commerce.SharedLibrary.Logs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;


namespace E_Commerce.SharedLibrary.Middleware
{
    internal class GlobalException(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            //Declare default variable
            string message = "Sorry, internal server error occurred. Kindly try again";
            int statusCode = (int)HttpStatusCode.InternalServerError;
            string title = "Error";

            try
            {
                await next(context);

                // Check if Response is Too Many Request // 429 Status Code. 
                if (context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
                {
                    title = "Warning";
                    message = "Too many request made. ";
                    statusCode = (int)StatusCodes.Status429TooManyRequests;

                    await ModifyHeader(context, title, message, statusCode);
                }

                // If Response is UnAuthorized // 401 StatusCode.
                if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    title = "Alert";
                    message = "Not authorized to access";
                    statusCode = StatusCodes.Status401Unauthorized;
                    await ModifyHeader(context, title, message, statusCode);
                }

                // If Response is Forbidden // 403 statuscode
                if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
                {
                    title = "Out of access";
                    message = "Not allowed/required to access";
                    statusCode = StatusCodes.Status403Forbidden;
                    await ModifyHeader(context, title, message, statusCode);
                }
            }
            catch (Exception ex)
            {
                /*Log Original Exceptions 
                 *LogToFile
                 *LogToDebug
                 *LogToConsole
                 */
                LogException.LogExceptions(ex);

                // Check if Exception is Timeout // 408 Request Timeout
                if (ex is TaskCanceledException || ex is TimeoutException)
                {
                    title = "Out of time";
                    message = "Request timeout... Please try again";
                    statusCode = StatusCodes.Status408RequestTimeout;
                }
                // If none of the Exception then do the default
                await ModifyHeader(context, title, message, statusCode);
            }
        }

        private static async Task ModifyHeader(HttpContext context, string title, string message, int statusCode)
        {
            // Display scary-free message to client
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new ProblemDetails()
            {
                Detail = message,
                Status = statusCode,
                Title = title
            }), CancellationToken.None);
            return;
        }
    }
}
