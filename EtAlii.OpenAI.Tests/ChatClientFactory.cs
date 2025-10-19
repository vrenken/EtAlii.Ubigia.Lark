
using System.ClientModel;
using OpenAI;
using OpenAI.Chat;

namespace EtAlii.OpenAI.Tests;

// Low GPU systems could use the following powershell command to start the Ollama server:
// $env:OLLAMA_LLM_LIBRARY = "cpu_avx2"
// ollama serve
// But this becomes very, very slow.
public class ChatClientFactory
{
    private const string Model = "gpt-oss:20b";
    //private const string Model = "llama3.1:latest";
    
    public static ChatClient Create()
    {
        var key = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "ollama";
        var client = new ChatClient(
            model: Model,
            credential: new ApiKeyCredential(key),
            options: new OpenAIClientOptions
            { 
                Endpoint = new Uri("http://localhost:11434/v1")
            }
        );
        return client;
    }
}