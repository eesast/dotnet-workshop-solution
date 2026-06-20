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
builder.Services.AddSingleton<LogFileAnalyzer>();   // 依赖注入
builder.Services.AddSingleton<AgentService>();      // 有状态服务需要单例

var app = builder.Build();
app.MapGrpcService<AgentService>();

app.Run();
