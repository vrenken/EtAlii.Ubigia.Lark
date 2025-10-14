namespace EtAlii.Ubigia.Lark.Tests;

public class RuntimeParserConfigTests
{
    [Fact]
    public void Parse_Algorithm_Earley_Default()
    {
        // Arrange
        var grammarText = """
            start: "ok"
            """;
        var syntax = EbnfSyntax.Parse(new StringReader(grammarText));
        var parser = new LarkRuntimeParser(syntax);

        // Act
        var result = parser.Parse("ok");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Earley", result.Diagnostics.Algorithm);
    }

    [Fact]
    public void Parse_Algorithm_Lalr_Configured()
    {
        // Arrange
        var grammarText = """
            start: "ok"
            """;
        var syntax = EbnfSyntax.Parse(new StringReader(grammarText));
        var config = new LarkParserConfiguration { Algorithm = ParserAlgorithm.Lalr };
        var parser = new LarkRuntimeParser(syntax, config);

        // Act
        var result = parser.Parse("ok");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("LALR(1)", result.Diagnostics.Algorithm);
    }

    [Fact]
    public void Parse_ErrorReporting_Farthest_Expected()
    {
        // Arrange
        var grammarText = """
            start: "a" "b"
            """;
        var syntax = EbnfSyntax.Parse(new StringReader(grammarText));
        var parser = new LarkRuntimeParser(syntax);

        // Act
        var result = parser.Parse("aX");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Error at line", result.Errors[0]);
        Assert.Contains("expected", result.Errors[0]);
        Assert.Contains("b", result.Errors[0]);
        Assert.True(result.Diagnostics.FarthestPosition > 0);
        Assert.Contains("b", result.Diagnostics.Expected);
    }

    [Fact]
    public void Parse_Algorithm_Earley_Uses_DynamicLexing_When_Configured()
    {
        // Arrange
        var grammarText = """
            WORD: /[A-Za-z]+/
            start: WORD
            """;
        var syntax = EbnfSyntax.Parse(new StringReader(grammarText));
        var config = new LarkParserConfiguration
        {
            Algorithm = ParserAlgorithm.Earley,
            Lexing = LexingMode.Dynamic,
            Ambiguity = "resolve",
            CollectAllParses = true
        };
        var parser = new LarkRuntimeParser(syntax, config);

        // Act
        var result = parser.Parse("Hello");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Earley", result.Diagnostics.Algorithm);
        Assert.True(result.Diagnostics.DynamicLexing);
        Assert.Equal("resolve", result.Diagnostics.AmbiguityHandling);
    }

    [Fact]
    public void Parse_Algorithm_Lalr_Uses_StandardLexing_When_Configured()
    {
        // Arrange
        var grammarText = """
            WORD: /[A-Za-z]+/
            start: WORD
            """;
        var syntax = EbnfSyntax.Parse(new StringReader(grammarText));
        var config = new LarkParserConfiguration
        {
            Algorithm = ParserAlgorithm.Lalr,
            Lexing = LexingMode.Standard,
            Ambiguity = "resolve",
            CollectAllParses = false
        };
        var parser = new LarkRuntimeParser(syntax, config);

        // Act
        var result = parser.Parse("Hello");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("LALR(1)", result.Diagnostics.Algorithm);
        Assert.False(result.Diagnostics.DynamicLexing);
        Assert.Equal("resolve", result.Diagnostics.AmbiguityHandling);
    }
}
