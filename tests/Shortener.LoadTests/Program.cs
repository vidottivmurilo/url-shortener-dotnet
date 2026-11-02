using System.Net;
using System.Text;
using System.Text.Json;
using NBomber.CSharp;

Console.WriteLine("==========================================================");
Console.WriteLine("🚀 INICIANDO TESTE DE CARGA & PERFORMANCE (NBOMBER)");
Console.WriteLine("Target API: http://localhost:5030");
Console.WriteLine("==========================================================");

var baseUrl = "http://localhost:5030";

// Reutilização eficiente de conexões HTTP (SocketsHttpHandler com pooling para evitar Socket Exhaustion)
using var handler = new SocketsHttpHandler
{
    PooledConnectionLifetime = TimeSpan.FromMinutes(5),
    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
    MaxConnectionsPerServer = 500,
    AllowAutoRedirect = false // Permite capturar e medir diretamente o HTTP 302 Found!
};

using var httpClient = new HttpClient(handler)
{
    BaseAddress = new Uri(baseUrl),
    Timeout = TimeSpan.FromSeconds(5)
};

// 1. Setup: Cria um link prévio para os testes de redirecionamento concorrente
Console.WriteLine("Criando link base para o teste de redirecionamento...");
var setupPayload = JsonSerializer.Serialize(new { originalUrl = "https://google.com" });
var setupResponse = await httpClient.PostAsync("/api/v1/urls", new StringContent(setupPayload, Encoding.UTF8, "application/json"));

if (!setupResponse.IsSuccessStatusCode)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"[ERRO] A API em {baseUrl} não está respondendo. Certifique-se de que 'dotnet run --project src/Shortener.Api' está rodando!");
    Console.ResetColor();
    return;
}

var setupJson = await setupResponse.Content.ReadAsStringAsync();
using var doc = JsonDocument.Parse(setupJson);
var targetShortCode = doc.RootElement.GetProperty("shortCode").GetString() ?? "q0V";
Console.WriteLine($"[OK] Link base criado com sucesso: {baseUrl}/{targetShortCode}\n");

// =========================================================================
// Cenário 1: Redirecionamento HTTP 302 Ultrarrápido (Cache-Aside + Métricas)
// =========================================================================
var redirectScenario = Scenario.Create("resolve_short_url_redirect", async context =>
{
    var response = await httpClient.GetAsync($"/{targetShortCode}");

    // No encurtador, 302 Found é o status de sucesso do redirecionamento
    if (response.StatusCode == HttpStatusCode.Found || response.StatusCode == HttpStatusCode.Redirect)
    {
        return Response.Ok(statusCode: ((int)response.StatusCode).ToString());
    }

    return Response.Fail(statusCode: ((int)response.StatusCode).ToString());
})
.WithoutWarmUp()
.WithLoadSimulations(
    // Aquece subindo até 50 req/s em 5s, depois sustenta 100 req/s por 10s
    Simulation.RampingInject(rate: 50, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(5)),
    Simulation.Inject(rate: 100, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(10))
);

// =========================================================================
// Cenário 2: Criação Concorrente de Novos Links (POST Base62 + Persistência)
// =========================================================================
var counter = 0;
var createScenario = Scenario.Create("create_short_url", async context =>
{
    var uniqueId = Interlocked.Increment(ref counter);
    var payload = JsonSerializer.Serialize(new { originalUrl = $"https://benchmark-test.com/artigo-{uniqueId}" });
    var content = new StringContent(payload, Encoding.UTF8, "application/json");

    var response = await httpClient.PostAsync("/api/v1/urls", content);

    if (response.StatusCode == HttpStatusCode.Created)
    {
        return Response.Ok(statusCode: ((int)response.StatusCode).ToString());
    }

    return Response.Fail(statusCode: ((int)response.StatusCode).ToString());
})
.WithoutWarmUp()
.WithLoadSimulations(
    Simulation.Inject(rate: 30, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(10))
);

// Executa os cenários e gera relatórios automáticos
NBomberRunner
    .RegisterScenarios(redirectScenario, createScenario)
    .WithTestSuite("Shortener_Performance_Suite")
    .WithTestName("local_load_test")
    .Run();
