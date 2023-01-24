using System.Net;
using Hangman.Models.ErrorModel;
using Hangman.Models.Exceptions;
using Hangman.Models.Exceptions.BaseExceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Hangman.Extensions
{
    public static class ExceptionMiddlewareExtension
    {
        public static void ConfigureExceptionHandler(this WebApplication app
            )
        {
            app.UseExceptionHandler(option =>
            {
                option.Run( async context =>
                {
                    context.Response.ContentType = "application/json";

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if ( contextFeature != null )
                    {
                        //there can be logging
                        context.Response.StatusCode = contextFeature.Error switch
                        {
                            NotFoundException => StatusCodes.Status404NotFound,
                            AlreadyExistsException => StatusCodes.Status400BadRequest,
                            WrongPasswordException => StatusCodes.Status400BadRequest,
                            InvalidRefreshTokenException => StatusCodes.Status400BadRequest,
                            ExpiredRefreshTokenException => StatusCodes.Status400BadRequest,
                            SessionAlreadyEndedException => StatusCodes.Status400BadRequest,
                            InvalidRequestException=> StatusCodes.Status400BadRequest,
                            _ => StatusCodes.Status500InternalServerError
                        };
                        await context.Response.WriteAsync(new ErrorDetail
                        {
                            StatusCode = context.Response.StatusCode,
                            Message = contextFeature.Error.Message
                        }.ToString());
                    }
                });
            }); 
            
        }
    }
}
