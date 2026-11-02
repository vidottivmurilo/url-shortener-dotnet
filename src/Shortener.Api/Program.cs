using Microsoft.AspNetCore.Mvc;
using Shortener.Application.Common.Interfaces;
using Shortener.Application.Urls;
using Shortener.Application.Urls.DTOs;
using Shortener.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Repositórios e Serviços de Aplicação
builder.Services.AddSingleton<IShortenedUrlRepository, InMemoryShortenedUrlRepository>();
builder.Services.AddScoped<CreateShortUrlHandler>(sp =>
{
    var repo = sp.GetRequiredService<IShortenedUrlRepository>();
    var httpContext = sp.GetService<IHttpContextAccessor>()?.HttpContext;
    var baseUrl = httpContext != null
        ? $"{httpContext.Request.Scheme}://{httpContext.Request.Host}"
        : "http://localhost:5000";
    return new CreateShortUrlHandler(repo, baseUrl);
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();

// Endpoint: createShortUrl
app.MapPost("/api/v1/urls", async ([FromBody] CreateShortUrlRequest request, [FromServices] CreateShortUrlHandler handler) =>
{
    try
    {
        var response = await handler.HandleAsync(request);
        return Results.Created($"/api/v1/urls/{response.ShortCode}", response);
    }
    catch (ArgumentException ex)
    {
        return Results.Problem(
            statusCode: StatusCodes.Status400BadRequest,
            title: "Validation Error",
            detail: ex.Message
        );
    }
    catch (Exception)
    {
        return Results.Problem(
            statusCode: StatusCodes.Status500InternalServerError,
            title: "Internal Server Error",
            detail: "Ocorreu um erro inesperado ao processar a requisição."
        );
    }
})
.WithName("CreateShortUrl")
.WithOpenApi();

app.Run();
