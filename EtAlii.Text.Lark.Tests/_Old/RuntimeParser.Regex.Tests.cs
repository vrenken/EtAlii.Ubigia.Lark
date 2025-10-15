using EtAlii.Text.Lark._Old;

namespace EtAlii.Text.Lark.Tests._Old;

public class RuntimeParserRegexTests
{
    [Fact]
    public void Parse_Regex_IgnoreCase_Flag()
    {
        // Arrange
        var grammarText = """
            WORD: /[a-z]+/i
            start: WORD
            """;
        var syntax = EbnfSyntax.Parse(new StringReader(grammarText));
        var parser = new LarkRuntimeParser(syntax);

        // Act
        var result = parser.Parse("ABCdef");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("start", result.Root!.Name);
    }

    [Fact]
    public void Parse_Regex_Verbose_Spaces()
    {
        // Arrange
        var grammarText = """
            WS: /[ \t]+/
            %ignore WS
            WORD: /(?x)  [a-z]  +/
            start: WORD
            """;
        var syntax = EbnfSyntax.Parse(new StringReader(grammarText));
        var parser = new LarkRuntimeParser(syntax);

        // Act
        var result = parser.Parse("hello");

        // Assert
        Assert.True(result.Success);
    }
}
