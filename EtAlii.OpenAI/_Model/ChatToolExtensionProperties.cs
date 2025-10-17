#nullable disable
namespace EtAlii.OpenAI;

/// <summary>
/// Small storage record to store new properties to instances using a ConditionalWeakTable instance.  
/// </summary>
public record ChatToolExtensionProperties
{
    public FunctionToolInvocationInfo Invocation { get; set; } = null!;
}