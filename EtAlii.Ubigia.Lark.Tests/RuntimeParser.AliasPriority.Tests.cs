namespace EtAlii.Ubigia.Lark.Tests;

public class RuntimeParserAliasPriorityTests
{
    [Fact]
    public void Parse_Alias_NodeName()
    {
        // Arrange
        var grammarText = """
            start: item
            item: ["x"] -> myalias
            """;
        var syntax = EbnfSyntax.Parse(new StringReader(grammarText));
        var parser = new LarkRuntimeParser(syntax);

        // Act
        var result = parser.Parse("x");

        // Assert
        Assert.True(result.Success);
        Assert.Contains(result.Root!.Children, c => c.AliasName == "myalias" || c.Name == "myalias");
    }

    [Fact]
    public void Parse_Token_Priority_LongestMatch()
    {
        // Arrange: overlapping tokens, priority should prefer the one that consumes more (or higher priority if equal)
        var grammarText = """
                          WORD: /[A-Za-z]+/
                          W: /[A-Za-z]/
                          start: WORD
                          """;
        var syntax = EbnfSyntax.Parse(new StringReader(grammarText));
        var parser = new LarkRuntimeParser(syntax);

        // Act
        var result = parser.Parse("Hello");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("start", result.Root!.Name);
        // The full token should be consumed, not just one character.
        Assert.Equal("Hello", result.Root.Text);
    }

    [Fact]
    public void Parse_Token_Priority_LongestMatch_2()
    {
        // Arrange: overlapping tokens, priority should prefer the one that consumes more (or higher priority if equal)
        var grammarText = """
                          WORD: /[A-Za-z]+/
                          W: /[A-Za-z]/
                          WS: /[ \t]+/
                          %ignore WS
                          start: WORD*
                          """;
        var syntax = EbnfSyntax.Parse(new StringReader(grammarText));
        var parser = new LarkRuntimeParser(syntax);

        // Act
        var result = parser.Parse("Hello world");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("start", result.Root!.Name);
        // The full token should be consumed, not just one character.
        Assert.Equal("Hello world", result.Root.Text);
    }
}
