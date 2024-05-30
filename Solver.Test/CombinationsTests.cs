using Solver.Lib;

namespace Solver.Test;

[TestFixture]
public class CombinationsTests
{
    [Test]
    [TestCase(5, 0, ExpectedResult = 1)]
    [TestCase(5, 1, ExpectedResult = 5)]
    [TestCase(5, 2, ExpectedResult = 10)]
    [TestCase(5, 3, ExpectedResult = 10)]
    [TestCase(5, 4, ExpectedResult = 5)]
    [TestCase(5, 5, ExpectedResult = 1)]
    [TestCase(10, 5, ExpectedResult = 252)]
    public int Test(int n, int k)
    {
        return Combinations.Get(n, k);
    }
}