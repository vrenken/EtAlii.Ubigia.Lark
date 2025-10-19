using OpenAI.Chat;
using ChatMessage = OpenAI.Chat.ChatMessage;

namespace EtAlii.OpenAI.Tests;

public class OrbatTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public OrbatTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task Orbat_Add_Orbat_1()
    {
        // Arrange.
        var orbatTestTools = new OrbatTestTools();
        var client = ChatClientFactory.Create();
        var messages = new ChatMessage[] { new UserChatMessage("Add a realistic military orbat with different levels of organisation and plenty of units.") };
        var options = new ChatCompletionOptions 
        { 
            Tools = 
            { 
                ChatToolEx.CreateFunctionTool(orbatTestTools, ott => ott.GetOrbat), 
                ChatToolEx.CreateFunctionTool(orbatTestTools, ott => ott.AddOrganisation), 
                ChatToolEx.CreateFunctionTool(orbatTestTools, ott => ott.AddUnit) 
            } 
        };
        var processor = new TestChatCompletionProcessor(_testOutputHelper);

        // Act.
        await processor.Process(client, messages, options);
        
        // Assert.
        Assert.NotEmpty(orbatTestTools.Orbat);
    }
    
    [Fact]
    public async Task Orbat_Add_Orbat_2()
    {
        // Arrange.
        var orbatTestTools = new OrbatTestTools();
        var client = ChatClientFactory.Create();
        var messages = new ChatMessage[] { new UserChatMessage("Add a realisting military orbat with different levels of organisation and plenty of units.") };
        var options = new ChatCompletionOptions 
        { 
            Tools = 
            { 
                ChatToolEx.CreateFunctionTool(() => orbatTestTools.GetOrbat), 
                ChatToolEx.CreateFunctionTool(() => orbatTestTools.AddOrganisation), 
                ChatToolEx.CreateFunctionTool(() => orbatTestTools.AddUnit) 
            } 
        };
        var processor = new TestChatCompletionProcessor(_testOutputHelper);

        // Act.
        await processor.Process(client, messages, options);
        
        // Assert.
        Assert.NotEmpty(orbatTestTools.Orbat);
    }
}