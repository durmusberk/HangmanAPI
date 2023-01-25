using System.Net;
using System.Text.Json;
using Hangman.Models.ErrorModel;
using Hangman.Models.Exceptions;
using Hangman.Models.Exceptions.BaseExceptions;

namespace Hangman.Middlewares
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;
        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc.ToString());
                await HandleExceptionAsync(context, exc);                
            }
        }

        public async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var response = context.Response;

            var errorResponse = new ErrorDetail();
            switch (exception)
            {
                
                case CustomException ex:
                    response.StatusCode = ex.StatusCode;
                    errorResponse.Message = ex.Message;
                    errorResponse.StatusCode = ex.StatusCode;
                    break;
                default:
                    response.StatusCode = StatusCodes.Status500InternalServerError;
                    errorResponse.StatusCode = StatusCodes.Status500InternalServerError;
                    errorResponse.Message = "Internal Server Error! Contact with Durmus Berk!";
                    break;
            }
            _logger.LogError(message: exception.Message);
            await context.Response.WriteAsync(errorResponse.ToString());
        }
    }
}
