namespace EtAlii.Ubigia.Lark.Tests;

public class ImportsTests
{
    [Fact]
    public void Import_Test_Simple()
    {
        // Arrange.
        var fileName = @"_Examples/Imports/simple.lark";
        
        // Act.
        var syntax = EbnfSyntax.Parse(fileName);

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
}