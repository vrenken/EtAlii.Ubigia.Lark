namespace EtAlii.Text.Lark.Tests;

public class LarkEbnfParserTests
{
    [Theory]
    [ClassData(typeof(LarkTestFileNames))]
    public void EbnfSyntax_Parse_File(string fileName)
    {
        // Arrange.
        var expectedFile = Path.ChangeExtension(fileName, "expected");
        var ebnf = File.ReadAllText(fileName);
        //var commonDirectory = "_Examples/Common";
        //var directory = Path.GetDirectoryName(fileName)!;
        //var importSource = new FileSystemImportSource(directory, commonDirectory);
        
        // Act.
        var ebnfGrammar = new LarkGrammar();// | EbnfStyle.UseCommentRuleWithSeparator | EbnfStyle.SquareBracketAsOptional | EbnfStyle.WhitespaceSeparator);
        var grammar = ebnfGrammar.Build(ebnf);
        var actual = grammar?.ToString() ?? string.Empty;
        
        // Assert.
        Assert.NotNull(ebnfGrammar);
        Assert.NotNull(grammar);
        Assert.NotEmpty(actual);
        // Assert.Empty(syntax.Errors);
        // Assert.NotEmpty(syntax.Items);

        if (File.Exists(expectedFile))
        {
            // var actual = syntax.Text;
            var expected = File.ReadAllText(expectedFile).TrimEnd();
            Assert.Equal(expected, actual);
        }
    }
}