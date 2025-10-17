
// using ModelContextProtocol.Client;
// using Microsoft.Extensions.AI;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.Logging;
// using ModelContextProtocol;
// using Serilog;
// using ModelContextProtocol.Protocol;

namespace EtAlii.OpenAI.Tests;

public partial class UnitTest1
{

//
//     private async Task<IChatClient> CreateClient2(McpClient mcpClient)
//     {
//         var mcpClientTools = await mcpClient.ListToolsAsync();
//         var tools = mcpClientTools
//             .Select(t => t.AsOpenAIChatTool())
//             .ToArray();
//         
//         var key = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "ollama";
//         var openAiClient = new OpenAIClient(
//             credential: new ApiKeyCredential(key), 
//             options: new OpenAIClientOptions
//             { 
//                 Endpoint = new Uri("http://localhost:11434/v1"),
//             });
//             
// #pragma warning disable OPENAI001
//         var client = openAiClient
//             .GetOpenAIResponseClient("gpt-oss:20b")
// #pragma warning restore OPENAI001
//             .AsIChatClient()
//             .AsBuilder()
//             .UseFunctionInvocation(configure: c =>
//             {
//                 c.AdditionalTools = tools;
//             })
//             .Build();
//         
//         return client;
//     }
//
//     private void Build(IHostApplicationBuilder builder)
//     {
//         var sc = new ServiceCollection();
//         sc
//             .AddMcpServer()
//             .WithStreamServerTransport()
//             .WithTools<SidcTools>(McpJsonUtilities.DefaultOptions);
//             //.WithResources<ProjectResources>();
//     }

    
    // private async Task<IMcpClient> CreateMcpClient()
    // {
    //     // var mcpUrl = _hostingInformation.GetApiUrl("/mcp");
    //     // var httpClient = _httpClientFactory.CreateClient("InsecureLlmClient");
    //     // var options = new SseClientTransportOptions { Endpoint = new Uri(mcpUrl), TransportMode = HttpTransportMode.AutoDetect };
    //     // var transport = new SseClientTransport(options, httpClient);
    //     //
    //     // // Create LoggerFactory and add Serilog
    //     // var loggerFactory = LoggerFactory.Create(builder =>
    //     // {
    //     //     builder.ClearProviders();                // Remove other providers
    //     //     builder.AddSerilog(dispose: true, logger: Log.ForContext("SourceContext","ModelContextProtocol.Client.McpClient"));       // Use Serilog
    //     // });
    //
    //     var clientTransport = new SseClientTransport(clientTransportOptions, loggerFactory);
    //
    //     var client = await McpClientFactory.CreateAsync(
    //         transport,
    //         loggerFactory: loggerFactory);
    //     _logger.Information("Initialized {Type}", client.GetType().FullName);
    //
    //     return client;
    // }

}