using LogAnalyzer;
using LogAnalyzerAgent.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

builder.Services.AddGrpc();
builder.Services.AddCors();

builder.Services.AddSingleton<LogFileAnalyzer>();   // 依赖注入
builder.Services.AddSingleton<AgentService>();      // 有状态服务需要单例

var app = builder.Build();

var whiteList = new HashSet<string>()
{
    "http://localhost:5235",
    "https://localhost:7169",
    "http://localhost:57814",
    "https://localhost:57815",
    "http://127.0.0.1:5235",
    "https://127.0.0.1:7169",
    "http://127.0.0.1:57814",
    "https://127.0.0.1:57815",
};
app.UseCors(policy =>
{
    policy
        .SetIsOriginAllowed(origin =>
            string.IsNullOrEmpty(origin) || whiteList.Contains(origin))
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
});

app.UseGrpcWeb();

app.MapGrpcService<AgentService>()
    .EnableGrpcWeb();

app.Run();
