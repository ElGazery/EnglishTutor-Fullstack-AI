using EnglishTutor.Application.Common.ResponseBase.Response;
using EnglishTutor.Domain.Exceptions;
using FluentValidation;
using System.Text.Json;

namespace EnglishTutor.Api.Middleware
{
    public class GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IWebHostEnvironment env)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);

                if (context.Response.StatusCode == 404 && !context.Response.HasStarted)
                    await WriteResponseAsync(context, "The requested resource was not found.", 404);
            }
            catch (Exception error)
            {
                logger.LogError(error, "Unhandled exception: {Message}", error.Message);
                await HandleExceptionAsync(context, error);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";

            string devMessage = env.IsDevelopment()
                ? $"{ex.Message}\n{ex.StackTrace}"
                : "An unexpected error occurred. Please try again later.";

            ApiResponse<string> response = ex switch
            {
                ValidationException ve => new ApiResponse<string>(
                    "Validation failed",
                    StatusCodes.Status400BadRequest,
                    ve.Errors.Select(e => e.ErrorMessage).ToList()),

                BadRequestCustomException br => new ApiResponse<string>(
                    br.Message,
                    StatusCodes.Status400BadRequest,
                    br.Errors?.ToList()),

                NotFoundCustomException => new ApiResponse<string>(
                    ex.Message,
                    StatusCodes.Status404NotFound),

                UnauthorizedCustomException => new ApiResponse<string>(
                    ex.Message,
                    StatusCodes.Status401Unauthorized),

                ForbiddenCustomException => new ApiResponse<string>(
                    ex.Message,
                    StatusCodes.Status403Forbidden),

                _ => new ApiResponse<string>(devMessage, StatusCodes.Status500InternalServerError)
            };

            context.Response.StatusCode = response.StatusCode;
            response.IsSuccess = false;

            var json = JsonSerializer.Serialize(response,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            await context.Response.WriteAsync(json);
        }

        private static async Task WriteResponseAsync(HttpContext context, string message, int statusCode)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var response = new ApiResponse<string>(message, statusCode);
            var json = JsonSerializer.Serialize(response,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            await context.Response.WriteAsync(json);
        }
    }
}
