using EnglishTutor.Api.Extensions;
using EnglishTutor.Api.Middleware;
using EnglishTutor.Application;
using EnglishTutor.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ─── Services ───────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpsRedirection(options =>
    {
        options.HttpsPort = 44355;
    });
}

// CORS — allow Angular dev server
builder.Services.AddCors(options =>
    options.AddPolicy("AllowAngular", policy =>
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()));

// ─── Pipeline ───────────────────────────────────────────────────────────────
var app = builder.Build();

app.UseCors("AllowAngular");

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Enable interactive API reference
app.MapOpenApi();
app.MapScalarApiReference();

// Root status endpoint so navigating to http://my-chatbot123.runasp.net/ returns 200 OK
app.MapGet("/", () => Results.Ok(new
{
    status = "Online",
    service = "English Tutor AI Backend API",
    version = "1.0",
    docs = "/scalar/v1",
    endpoints = new[] { "/api/account/login", "/api/account/register", "/api/sessions" }
}));

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
