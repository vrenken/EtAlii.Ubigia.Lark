using System.ClientModel;
using OpenAI;
using OpenAI.Chat;
using Xunit.Abstractions;

namespace EtAlii.Ubigia.OpenAI.Tests;

public class UnitTest1
{
    private readonly ITestOutputHelper _testOutputHelper;

    public UnitTest1(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    private ChatClient CreateClient()
    {
        var key = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "ollama";
        var client = new ChatClient(
            model: "gpt-oss:20b",
            credential: new ApiKeyCredential(key),
            options: new OpenAIClientOptions
            { 
                Endpoint = new Uri("http://localhost:11434/v1")
            }
        );
        return client;
    }

    [Fact]
    public void Test1()
    {
        // Arrange.
        var client = CreateClient();
        
        // Act.
        ChatCompletion completion = client.CompleteChat("Say 'this is a test.'");

        // Assert.
        Assert.Equal("this is a test.", completion.Content[0].Text);
        //_testOutputHelper.WriteLine($"[ASSISTANT]: {completion.Content[0].Text}");
    }
    

    [Fact]
    public async Task Test2()
    {
        // Arrange.
        var client = CreateClient();
        var messages = new ChatMessage[] { new UserChatMessage("What's the weather like today?") };
        var options = new ChatCompletionOptions { Tools = { ChatCompletionProcessor.GetCurrentLocationTool, ChatCompletionProcessor.GetCurrentWeatherTool } };
        var processor = new ChatCompletionProcessor(_testOutputHelper);

        // Act.
        await processor.Process(client, messages, options);
    }

    [Fact]
    public async Task Sidc_Generation_Tank_01()
    {
        // Arrange.
        var client = CreateClient();
        var messages = new ChatMessage[] { new UserChatMessage("Please provide me with a SIDC for a friendly tank. Do not provide an explanation but only return the SIDC code.") };
        var options = new ChatCompletionOptions { Tools = { ChatCompletionProcessor.GetSidcRefinementOptionsTool } };
        var processor = new ChatCompletionProcessor(_testOutputHelper);

        // Act.
        await processor.Process(client, messages, options);
    }

    [Fact]
    public async Task Sidc_Generation_Aircraft_01()
    {
        // Arrange.
        var client = CreateClient();
        var messages = new ChatMessage[] { new UserChatMessage("Please provide me with a SIDC for a hostile aircraft. Do not provide an explanation but only return the SIDC code.") };
        var options = new ChatCompletionOptions { Tools = { ChatCompletionProcessor.GetSidcRefinementOptionsTool } };
        var processor = new ChatCompletionProcessor(_testOutputHelper);

        // Act.
        await processor.Process(client, messages, options);
    }
}