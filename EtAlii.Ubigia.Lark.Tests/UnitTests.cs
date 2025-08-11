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
        var tree = LarkParser.Parse(content);

        // Assert.
        Assert.NotEmpty(tree);
        var sb = new StringBuilder();
        foreach (var item in tree)
        {
            sb.AppendLine(item.ToString());
        }
        var actualDump = sb.ToString();

        var dmpFile = Path.ChangeExtension(fileName, "dmp");
        if (File.Exists(dmpFile))
        {
            var expectedDump = File.ReadAllText(dmpFile);
            Assert.Equal(expectedDump, actualDump);
        }
    }
    
    [Fact]
    public void Test2()
    {
        // Arrange.
        var fileName = @"_Examples/Antlr4/common.lark";
        var content = File.OpenRead(fileName);
        
        // Act.
        var tree = LarkParser.Parse(content);

        // Assert.
        Assert.NotEmpty(tree);
    }
}