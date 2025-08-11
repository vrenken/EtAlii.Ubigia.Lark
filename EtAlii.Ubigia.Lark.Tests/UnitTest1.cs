namespace EtAlii.Ubigia.Lark.Tests;

public class UnitTest1
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