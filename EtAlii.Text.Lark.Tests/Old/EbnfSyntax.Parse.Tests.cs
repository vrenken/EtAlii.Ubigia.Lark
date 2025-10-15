namespace EtAlii.Ubigia.Lark.Tests;

public class EbnfSyntaxTests
{
    [Theory]
    [ClassData(typeof(LarkTestFileNames))]
    public void EbnfSyntax_Parse_File(string fileName)
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
    }
    
    [Fact]
    public void EbnfSyntax_Parse_File_Common()
    {
        // Arrange.
        var fileName = @"_Examples/Antlr4/common.lark";
        
        // Act.
        var syntax = EbnfSyntax.Parse(fileName);

        // Assert.
        Assert.NotEmpty(syntax.Items);
    }
}