using System.ComponentModel;
using OpenAI.Chat;

namespace EtAlii.OpenAI.Tests;

public partial class ChatCompletionProcessor
{
    public static string GetCurrentLocation()
    {
        // Call the location API here.
        return "San Francisco";
    }
    
    
    public static string GetCurrentWeatherOld(
        string location, string unit = "celsius")
    {
        // Call the weather API here.
        return $"31 {unit}";
    }
    
    [Description("Get the current weather in a given location")]
    public static string GetCurrentWeatherNew(
        [Description("The city and state, e.g. Boston, MA")] string location, 
        [Description("The temperature unit to use. Infer this from the specified location.")] string unit)
    {
        // Call the weather API here.
        return $"31 {unit}";
    }

    
    public static readonly ChatTool GetCurrentLocationTool = ChatTool.CreateFunctionTool(
        functionName: nameof(GetCurrentLocation),
        functionDescription: "Get the user's current location"
    );

    public static readonly ChatTool GetCurrentWeatherToolNew = ChatToolEx.CreateFunctionTool<string, string, string>(function: GetCurrentWeatherNew);

    public static readonly ChatTool GetCurrentWeatherToolOld = ChatTool.CreateFunctionTool(
        functionName: nameof(GetCurrentWeatherOld),
        functionDescription: "Get the current weather in a given location",
        functionParameters: BinaryData.FromBytes(
            """
            {
                "type": "object",
                "properties": {
                    "location": {
                        "type": "string",
                        "description": "The city and state, e.g. Boston, MA"
                    },
                    "unit": {
                        "type": "string",
                        "enum": [ "celsius", "fahrenheit" ],
                        "description": "The temperature unit to use. Infer this from the specified location."
                    }
                },
                "required": [ "location" ]
            }
            """u8.ToArray())
    );
}