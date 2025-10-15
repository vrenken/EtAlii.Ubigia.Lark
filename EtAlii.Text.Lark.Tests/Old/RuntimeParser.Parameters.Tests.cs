namespace EtAlii.Ubigia.Lark.Tests;

public class RuntimeParserParametersTests
{
    [Fact]
    public void Parameterized_Brackets_Invocation_Default()
    {
        // Arrange
        var grammarText = """
            A: /a+/
            B: /b+/
            pair[x, y]: x y
            start: pair[A, B]
            """;
        var syntax = EbnfSyntax.Parse(new StringReader(grammarText));
        var config = new LarkParserConfiguration
        {
            EnableParameterizedRules = true,
            ParamBrackets = ParameterBracketStyle.Both
        };
        var parser = new LarkRuntimeParser(syntax, config);

        // Act
        var result = parser.Parse("aaabbb");

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Root);
        Assert.Equal("start", result.Root!.Name);
    }

    [Fact]
    public void Parameterized_Braces_Invocation_Configured()
    {
        // Arrange
        var grammarText = """
            A: /a+/
            B: /b+/
            pair[x, y]: x y
            start: pair{A, B}
            """;
        var syntax = EbnfSyntax.Parse(new StringReader(grammarText));
        var config = new LarkParserConfiguration
        {
            EnableParameterizedRules = true,
            ParamBrackets = ParameterBracketStyle.Braces
        };
        var parser = new LarkRuntimeParser(syntax, config);

        // Act
        var result = parser.Parse("aaabbb");

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Root);
        Assert.Equal("start", result.Root!.Name);
    }

    [Fact]
    public void Parameterized_Argument_Count_Mismatch_Shows_Error()
    {
        // Arrange
        var grammarText = """
            A: /a+/
            pair[x, y]: x y
            start: pair{A}
            """;
        var syntax = EbnfSyntax.Parse(new StringReader(grammarText));
        var config = new LarkParserConfiguration
        {
            EnableParameterizedRules = true,
            ParamBrackets = ParameterBracketStyle.Both
        };
        var parser = new LarkRuntimeParser(syntax, config);

        // Act
        var result = parser.Parse("aaa");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("expected", result.Errors[0]);
    }
}
