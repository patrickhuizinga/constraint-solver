using Solver.Lib;

namespace Solver.Test;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        var answer = new Class1().Answer();
        Assert.That(answer, Is.EqualTo(42));
    }
}