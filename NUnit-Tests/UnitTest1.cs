namespace NUnit_Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        // Arrange
        var minhaClasse = new GerarHorario();

        // Act
        bool resultado = minhaClasse.ReturnVolta();

        // Assert
        Assert.That(resultado, Is.False);
        Assert.Pass();
    }
}
