using System.Reflection;

namespace EtAlii.OpenAI;

/// <summary>
/// Represents the necessary information required to invoke a specific C# method for a chat function tool.
/// </summary>
public class FunctionToolInvocationInfo
{
    /// <summary>
    /// Gets the method information to be invoked, representing the delegate or method
    /// associated with a functional operation within a chat tool.
    /// </summary>
    public required MethodInfo Method { get; init; }

    /// <summary>
    /// Gets the array of parameters required by the method to execute, describing the
    /// metadata and characteristics of each parameter.
    /// </summary>
    public required ParameterInfo[] Parameters { get; init; }

    /// <summary>
    /// Gets or sets the instance of the object on which the associated method will be invoked.
    /// This is required for calling instance methods and can be null for static methods.
    /// </summary>
    public required object? Instance { get; init; } 
}