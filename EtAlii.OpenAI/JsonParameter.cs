#nullable disable

using System.Reflection;
using System.Text.Json;

namespace EtAlii.OpenAI;

/// Provides utilities for working with JSON representations of method parameters.
public static class JsonParameter
{
    /// Determines the JSON type representation of a parameter based on its type and the context of the provided method.
    /// <param name="parameter">
    /// Metadata about the parameter, including its name, type, and attributes.
    /// </param>
    /// <param name="method">
    /// The method that contains the parameter, used for context in error messages.
    /// </param>
    /// <returns>
    /// A string representation of the JSON type for the parameter (e.g., "string", "number", "boolean").
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the parameter's type is not supported for JSON serialization.
    /// </exception>
    public static string GetJsonType(ParameterInfo parameter, MethodInfo method)
    {
        if (parameter.ParameterType.IsEnum)
        {
            return "string";
        }
        return parameter.ParameterType.Name switch
        {
            "String" => "string",
            "Char" => "string",
            "Int16" => "number",
            "Int32" => "number",
            "Int64" => "number",
            "UInt16" => "number",
            "UInt32" => "number",
            "UInt64" => "number",
            "Single" => "number",
            "Double" => "number",
            "Decimal" => "number",
            "Boolean" => "boolean",
            "DateTime" => "string",
            "DateTimeOffset" => "string",
            "TimeSpan" => "string",
            "DateOnly" => "string",
            "TimeOnly" => "string",
            // _ => "object",
            _ => throw new ArgumentException($"Type {parameter.ParameterType.Name} is not supported for parameters on method {method.Name}.", nameof(parameter.ParameterType))
        };
    }

    /// Retrieves the value of a parameter from a JsonElement based on its type.
    /// <param name="parameterJsonElement">
    /// The JsonElement containing the parameter value.
    /// </param>
    /// <param name="parameter">
    /// Metadata about the parameter including its type.
    /// </param>
    /// <returns>
    /// The deserialized value of the parameter, cast to the appropriate type.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting to cast an empty string to an enum or when the parameter type is not supported.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the parameter type is unsupported.
    /// </exception>
    public static object GetValue(JsonElement parameterJsonElement, ParameterInfo parameter)
    {
        if (parameter.ParameterType.IsEnum)
        {
            var value = parameterJsonElement.GetString();
            return string.IsNullOrWhiteSpace(value) 
                ? throw new InvalidOperationException($"Cannot cast an empty string to enum {parameter.ParameterType.Name}.") 
                : Enum.Parse(parameter.ParameterType, value);
        }
        
        return parameter.ParameterType switch
        {
            { } type when type == typeof(string) => parameterJsonElement.GetString(),
            { } type when type == typeof(short) => parameterJsonElement.GetInt16(),
            { } type when type == typeof(int) => parameterJsonElement.GetInt32(),
            { } type when type == typeof(long) => parameterJsonElement.GetInt64(),
            { } type when type == typeof(ushort) => parameterJsonElement.GetUInt16(),
            { } type when type == typeof(uint) => parameterJsonElement.GetUInt32(),
            { } type when type == typeof(ulong) => parameterJsonElement.GetUInt64(),
            { } type when type == typeof(float) => parameterJsonElement.GetSingle(),
            { } type when type == typeof(double) => parameterJsonElement.GetDouble(),
            { } type when type == typeof(decimal) => parameterJsonElement.GetDecimal(),
            { } type when type == typeof(bool) => parameterJsonElement.GetBoolean(),
            { } type when type == typeof(DateTime) => DateTime.Parse(parameterJsonElement.GetString()!),
            { } type when type == typeof(DateTimeOffset) => DateTimeOffset.Parse(parameterJsonElement.GetString()!),
            { } type when type == typeof(DateOnly) => DateOnly.Parse(parameterJsonElement.GetString()!),
            { } type when type == typeof(TimeOnly) => TimeOnly.Parse(parameterJsonElement.GetString()!),
            _ => throw new ArgumentException($"Unsupported parameter type: {parameter.ParameterType}")
        };
    }
}
