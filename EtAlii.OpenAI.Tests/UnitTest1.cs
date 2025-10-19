using System.ClientModel;
using OpenAI;
using OpenAI.Chat;
using ChatMessage = OpenAI.Chat.ChatMessage;

namespace EtAlii.OpenAI.Tests;

public partial class UnitTest1
{
    private readonly ITestOutputHelper _testOutputHelper;

    public UnitTest1(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void Test1()
    {
        // Arrange.
        var client = ChatClientFactory.Create();
        
        // Act.
        ChatCompletion completion = client.CompleteChat("Say 'this is a test.'");

        // Assert.
        Assert.Equal("this is a test.", completion.Content[0].Text);
        //_testOutputHelper.WriteLine($"[ASSISTANT]: {completion.Content[0].Text}");
    }

    [Fact]
    public async Task Test2_Invocation_Manual()
    {
        // Arrange.
        var client = ChatClientFactory.Create();
        var messages = new ChatMessage[] { new UserChatMessage("What's the weather like today?") };
        var options = new ChatCompletionOptions { Tools = { TestChatCompletionProcessor.GetCurrentLocationTool, TestChatCompletionProcessor.GetCurrentWeatherToolManual } };
        var processor = new TestChatCompletionProcessor(_testOutputHelper);

        // Act.
        await processor.Process(client, messages, options);
    }

    [Fact]
    public async Task Test2_Invocation_Automatic()
    {
        // Arrange.
        var client = ChatClientFactory.Create();
        var messages = new ChatMessage[] { new UserChatMessage("What's the weather like today?") };
        var options = new ChatCompletionOptions { Tools = { 
            TestChatCompletionProcessor.GetCurrentLocationTool, 
            ChatToolEx.CreateFunctionTool<string, string, string>(function: TestChatCompletionProcessor.GetCurrentWeatherNew) } };
        var processor = new TestChatCompletionProcessor(_testOutputHelper);

        // Act.
        await processor.Process(client, messages, options);
    }

    
    [Fact]
    public async Task Sidc_Generation_Tank_Manual_01()
    {
        // Arrange.
        var client = ChatClientFactory.Create();
        var messages = new ChatMessage[] { new UserChatMessage("Please provide me with a SIDC for a friendly tank. Do not provide an explanation but only return the SIDC code.") };
        var options = new ChatCompletionOptions { Tools = { TestChatCompletionProcessor.GetSidcRefinementOptionsToolManual } };
        var processor = new TestChatCompletionProcessor(_testOutputHelper);

        // Act.
        await processor.Process(client, messages, options);
    }
    
    [Fact]
    public async Task Sidc_Generation_Tank_Automatic_01()
    {
        // Arrange.
        var client = ChatClientFactory.Create();
        var messages = new ChatMessage[] { new UserChatMessage("Please provide me with a SIDC for a friendly tank. Do not provide an explanation but only return the SIDC code.") };
        var options = new ChatCompletionOptions { Tools = { ChatToolEx.CreateFunctionTool<string, string, SidcRefinementResult>(function: TestChatCompletionProcessor.GetSidcRefinementOptionsAutomatic) } };
        var processor = new TestChatCompletionProcessor(_testOutputHelper);

        // Act.
        await processor.Process(client, messages, options);
    }
    
    

    [Fact]
    public async Task Sidc_Generation_Aircraft_Manual_01()
    {
        // Arrange.
        var client = ChatClientFactory.Create();
        var messages = new ChatMessage[] { new UserChatMessage("Please provide me with a SIDC for a hostile aircraft. Do not provide an explanation but only return the SIDC code.") };
        var options = new ChatCompletionOptions { Tools = { TestChatCompletionProcessor.GetSidcRefinementOptionsToolManual } };
        var processor = new TestChatCompletionProcessor(_testOutputHelper);

        // Act.
        await processor.Process(client, messages, options);
    }

    [Fact]
    public async Task Sidc_Generation_Aircraft_Automatic_01()
    {
        // Arrange.
        var client = ChatClientFactory.Create();
        var messages = new ChatMessage[] { new UserChatMessage("Please provide me with a SIDC for a hostile aircraft. Do not provide an explanation but only return the SIDC code.") };
        var options = new ChatCompletionOptions { Tools = { ChatToolEx.CreateFunctionTool<string, string, SidcRefinementResult>(function: TestChatCompletionProcessor.GetSidcRefinementOptionsAutomatic) } };
        var processor = new TestChatCompletionProcessor(_testOutputHelper);

        // Act.
        await processor.Process(client, messages, options);
    }
}