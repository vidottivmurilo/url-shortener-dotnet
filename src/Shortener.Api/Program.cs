using Microsoft.AspNetCore.Mvc;
using Shortener.Application.Common.Interfaces;
using Shortener.Application.Urls;
using Shortener.Application.Urls.DTOs;
using Shortener.Infrastructure.Caching;
using Shortener.Infrastructure.Messaging;
using Shortener.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Repositórios, Cache e Mensageria
builder.Services.AddSingleton<IShortenedUrlRepository, InMemoryShortenedUrlRepository>();
builder.Services.AddSingleton<IClickMetricRepository, InMemoryClickMetricRepository>();
builder.Services.AddSingleton<ICacheService, InMemoryCacheService>();
builder.Services.AddSingleton<IMessagePublisher>(sp =>
{
    var metrics = sp.GetRequiredService<IClickMetricRepository>();
    return new InMemoryMessagePublisher(metrics);
});

// Handlers de Aplicação
builder.Services.AddScoped<CreateShortUrlHandler>(sp =>
{
    var repo = sp.GetRequiredService<IShortenedUrlRepository>();
    var httpContext = sp.GetService<IHttpContextAccessor>()?.HttpContext;
    var baseUrl = httpContext != null
        ? $"{httpContext.Request.Scheme}://{httpContext.Request.Host}"
        : "http://localhost:5000";
    return new CreateShortUrlHandler(repo, baseUrl);
});

builder.Services.AddScoped<ResolveShortUrlHandler>();
builder.Services.AddScoped<GetUrlStatsHandler>();

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

// Endpoint: createShortUrl (POST /api/v1/urls)
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

// Endpoint: getUrlStats (GET /api/v1/urls/{shortCode}/stats)
app.MapGet("/api/v1/urls/{shortCode}/stats", async (string shortCode, [FromServices] GetUrlStatsHandler handler) =>
{
    try
    {
        var response = await handler.HandleAsync(new GetUrlStatsQuery(shortCode));
        return Results.Ok(response);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Link Not Found",
            detail: ex.Message
        );
    }
    catch (Exception)
    {
        return Results.Problem(
            statusCode: StatusCodes.Status500InternalServerError,
            title: "Internal Server Error",
            detail: "Erro ao consultar estatísticas do link."
        );
    }
})
.WithName("GetUrlStats")
.WithOpenApi();

// Endpoint: resolveShortUrl (GET /{shortCode})
app.MapGet("/{shortCode}", async (string shortCode, HttpContext httpContext, [FromServices] ResolveShortUrlHandler handler) =>
{
    try
    {
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = httpContext.Request.Headers.UserAgent.ToString();

        var query = new ResolveShortUrlQuery(shortCode, ipAddress, userAgent);
        var result = await handler.HandleAsync(query);

        return Results.Redirect(result.OriginalUrl, permanent: false);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Link Not Found",
            detail: ex.Message
        );
    }
    catch (InvalidOperationException ex)
    {
        return Results.Problem(
            statusCode: StatusCodes.Status410Gone,
            title: "Link Deactivated or Expired",
            detail: ex.Message
        );
    }
    catch (Exception)
    {
        return Results.Problem(
            statusCode: StatusCodes.Status500InternalServerError,
            title: "Internal Server Error",
            detail: "Erro inesperado ao resolver o redirecionamento."
        );
    }
})
.WithName("ResolveShortUrl")
.WithOpenApi();

app.Run();
