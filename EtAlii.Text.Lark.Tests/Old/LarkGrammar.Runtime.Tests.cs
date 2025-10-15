namespace EtAlii.Ubigia.Lark.Tests;

public class LarkGrammarTests
{
    [Theory]
    [ClassData(typeof(LarkTestFileNames))]
    public void LarkGrammar_Runtime_Parse_File(string fileName)
    {
        // Arrange.
        var commonDirectory = "_Examples/Common";
        var directory = Path.GetDirectoryName(fileName)!;
        var importSource = new FileSystemImportSource(directory, commonDirectory);
        
        // Act.
        var syntax = EbnfSyntax.Parse(fileName, importSource);

        // Assert.
        Assert.True(syntax.IsValid);
        Assert.Empty(syntax.Errors);
        Assert.NotEmpty(syntax.Items);

        var expectedFile = Path.ChangeExtension(fileName, "expected");
        if (File.Exists(expectedFile))
        {
            var actual = syntax.Text;
            var expected = File.ReadAllText(expectedFile).TrimEnd();
            Assert.Equal(expected, actual);
        }
        var txtFile = Path.ChangeExtension(fileName, "txt");
        if (File.Exists(txtFile))
        {
            var parser = new LarkRuntimeParser(syntax, new LarkParserConfiguration
            {
                Lexing = LexingMode.Dynamic,
                Algorithm = ParserAlgorithm.Earley, 
                CollectAllParses = true
            });
            var input = File.ReadAllText(txtFile);
            var result = parser.Parse(input);
            Assert.True(result.Success);
        }
    }
}