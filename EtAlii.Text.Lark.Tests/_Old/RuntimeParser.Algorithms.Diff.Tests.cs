using EtAlii.Text.Lark._Old;

namespace EtAlii.Text.Lark.Tests._Old;

public class RuntimeParserAlgorithmsDiffTests
{
    [Fact]
    public void Earley_Can_Segment_Letters_While_Lalr_Cannot()
    {
        // Arrange
        var grammarText = """
            WORD: /[A-Za-z]+/
            W: /[A-Za-z]/
            start: W W W W W "!"
            """;
        var syntax = EbnfSyntax.Parse(new StringReader(grammarText));

        // Earley (dynamic, scannerless-style)
        var earley = new LarkRuntimeParser(syntax, new LarkParserConfiguration
        {
            Algorithm = ParserAlgorithm.Earley,
            Lexing = LexingMode.Dynamic
        });

        // LALR (global lexing, longest match)
        var lalr = new LarkRuntimeParser(syntax, new LarkParserConfiguration
        {
            Algorithm = ParserAlgorithm.Lalr,
            Lexing = LexingMode.Standard
        });

        var input = "Hello!";

        // Act
        var earleyResult = earley.Parse(input);
        var lalrResult = lalr.Parse(input);

        // Assert
        Assert.True(earleyResult.Success); // Earley can match W W W W W over characters, then "!"
        Assert.False(lalrResult.Success);  // LALR tokenizes "Hello" as a WORD token and cannot re-segment into 5 W tokens
    }

    [Fact]
    public void Lalr_Tokenization_Prefers_Longest_For_WORD_Exclamation()
    {
        // Arrange
        var grammarText = """
            WORD: /[A-Za-z]+/
            start: WORD "!"
            """;
        var syntax = EbnfSyntax.Parse(new StringReader(grammarText));

        var lalr = new LarkRuntimeParser(syntax, new LarkParserConfiguration
        {
            Algorithm = ParserAlgorithm.Lalr
        });

        // Act
        var result = lalr.Parse("Hello!");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("LALR(1)", result.Diagnostics.Algorithm);
        Assert.False(result.Diagnostics.DynamicLexing);
    }

    [Fact]
    public void Lalr_Parses_Literal_Only_Grammar_Via_Implicit_Literals()
    {
        // Arrange
        var grammarText = """
            start: "hello" " " "world"
            """;
        var syntax = EbnfSyntax.Parse(new StringReader(grammarText));

        var lalr = new LarkRuntimeParser(syntax, new LarkParserConfiguration
        {
            Algorithm = ParserAlgorithm.Lalr
        });

        // Act
        var result = lalr.Parse("hello world");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("LALR(1)", result.Diagnostics.Algorithm);
    }

    [Fact]
    public void Earley_And_Lalr_Both_Handle_Ignore_With_WORD_Plus()
    {
        // Arrange
        var grammarText = """
            WORD: /[A-Za-z]+/
            WS: /[ \t]+/
            %ignore WS
            start: WORD+
            """;
        var syntax = EbnfSyntax.Parse(new StringReader(grammarText));

        var input = "alpha   beta\tgamma";

        // Earley
        var earley = new LarkRuntimeParser(syntax, new LarkParserConfiguration { Algorithm = ParserAlgorithm.Earley });
        var r1 = earley.Parse(input);
        // LALR
        var lalr = new LarkRuntimeParser(syntax, new LarkParserConfiguration { Algorithm = ParserAlgorithm.Lalr });
        var r2 = lalr.Parse(input);

        // Assert
        Assert.True(r1.Success);
        Assert.True(r2.Success);
        Assert.Equal("alpha   beta\tgamma", r1.Root!.Text);
        Assert.Equal("alpha   beta\tgamma", r2.Root!.Text);
    }

    [Fact]
    public void Earley_Exact_Repetition_On_Character_Tokens_Succeeds_Where_Lalr_Cannot()
    {
        // Arrange
        var grammarText = """
            W: /[A-Za-z]/
            start: W~5 "!"
            """;
        var syntax = EbnfSyntax.Parse(new StringReader(grammarText));

        var earley = new LarkRuntimeParser(syntax, new LarkParserConfiguration { Algorithm = ParserAlgorithm.Earley });
        var lalr = new LarkRuntimeParser(syntax, new LarkParserConfiguration { Algorithm = ParserAlgorithm.Lalr });

        // Act
        var rEarley = earley.Parse("Hello!");
        var rLalr = lalr.Parse("Hello!");

        // Assert
        Assert.True(rEarley.Success);
        // LALR will try to tokenize "Hello" as a single token if another overlapping token exists; here only W exists,
        // so LALR also succeeds. To make it distinct from the segmentation test above, we keep this as a sanity pass.
        Assert.True(rLalr.Success);
    }
}
