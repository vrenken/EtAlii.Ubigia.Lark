using EtAlii.Text.Lark._Old;

namespace EtAlii.Text.Lark.Tests._Old;

public class RuntimeParserTests
{
    [Fact]
    public void Parse_Literals_Sequence()
    {
        // Arrange
        var grammarText = """
            start: "hello" " " "world"
            """;
        var syntax = EbnfSyntax.Parse(new StringReader(grammarText));

        var parser = new LarkRuntimeParser(syntax);
        var input = "hello world";

        // Act
        var result = parser.Parse(input);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Root);
        Assert.Equal("start", result.Root!.Name);
        Assert.Equal(input, result.Root.Text);
    }

    [Fact]
    public void Parse_Regex_Token_With_Ignore()
    {
        // Arrange
        var grammarText = """
            WORD: /[A-Za-z]+/
            WS: /[ \t]+/
            %ignore WS
            start: WORD+
            """;
        var syntax = EbnfSyntax.Parse(new StringReader(grammarText));

        var parser = new LarkRuntimeParser(syntax);
        var input = "alpha   beta\tgamma";

        // Act
        var result = parser.Parse(input);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Root);
        Assert.Equal("start", result.Root!.Name);
        Assert.Equal("alpha   beta\tgamma", result.Root.Text);
    }

    [Theory]
    [InlineData("abcc", true)]
    [InlineData("ac", true)]
    [InlineData("ab", false)]
    [InlineData("a", false)]
    public void Parse_Quantifiers_QMark_Star_Plus(string input, bool expectedSuccess)
    {
        // Arrange
        var grammarText = """
            start: "a" "b"? "c"+
            """;
        var syntax = EbnfSyntax.Parse(new StringReader(grammarText));
        var parser = new LarkRuntimeParser(syntax);

        // Act
        var result = parser.Parse(input);

        // Assert
        Assert.Equal(expectedSuccess, result.Success);
    }

    [Fact]
    public void Parse_Range_Repetition()
    {
        // Arrange
        var grammarText = """
            LETTER: "a".."z"
            start: LETTER~2
            """;
        var syntax = EbnfSyntax.Parse(new StringReader(grammarText));
        var parser = new LarkRuntimeParser(syntax);
        var input = "az";

        // Act
        var result = parser.Parse(input);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(input, result.Root!.Text);
    }

    [Theory]
    [InlineData("abc", true)]
    [InlineData("ab", false)]
    [InlineData("abcd", false)]
    public void Parse_Tilde_Range(string input, bool expectedSuccess)
    {
        // Arrange
        var grammarText = """
            X: /[a-z]/
            start: X~3..3
            """;
        var syntax = EbnfSyntax.Parse(new StringReader(grammarText));
        var parser = new LarkRuntimeParser(syntax);

        // Act
        var result = parser.Parse(input);

        // Assert
        Assert.Equal(expectedSuccess, result.Success);
    }
}
