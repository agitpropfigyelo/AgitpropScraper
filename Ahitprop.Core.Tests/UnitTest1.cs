using Agitprop.Core;

namespace Ahitprop.Core.Tests;

public class SpiderTest
{
    private Spider spider;

    [SetUp]
    [TestCase(false)]
    [TestCase(true)]
    public void Setup(bool withProxy)
    {
    }

    [Test]
    public void Test1()
    {
        Assert.Pass();
    }
}