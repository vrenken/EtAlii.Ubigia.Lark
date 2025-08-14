using System.Text;

namespace EtAlii.Ubigia.Lark.Tests;

public class UnitTests
{
    [Theory]
    [ClassData(typeof(LarkTestFileNames))]
    public void Test1(string fileName)
    {
        // Arrange.
        var content = File.OpenRead(fileName);
        
        // Act.
        var syntax = LarkParseSyntax.Parse(content);

        // Assert.
        Assert.True(syntax.IsValid);
        Assert.Empty(syntax.Errors);
        Assert.NotEmpty(syntax.Items);

        var dmpFile = Path.ChangeExtension(fileName, "dmp");
        if (File.Exists(dmpFile))
        {
            var actual = syntax.Text;
            var expected = File.ReadAllText(dmpFile).TrimEnd();
            Assert.Equal(expected, actual);
        }
    }
    
    [Fact]
    public void Test2()
    {
        // Arrange.
        var fileName = @"_Examples/Antlr4/common.lark";
        var content = File.OpenRead(fileName);
        
        // Act.
        var syntax = LarkParseSyntax.Parse(content);

        // Assert.
        Assert.NotEmpty(syntax.Items);
    }
}