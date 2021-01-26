using Amphisbaena.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace Amphisbaena.Tests.Linq {

  [TestCategory("Linq.SkipAndTake")]
  [TestClass]
  public class SkipAndTakeTest {
    [TestMethod("Skip And Take")]
    public async Task SimpleStatistics() {
      int[] data = Enumerable
        .Range(0, 100)
        .ToArray();

      int expected = data
        .SkipWhile(x => x < 10)
        .TakeWhile(x => x < 50)
        .Skip(3)
        .Take(10)
        .Sum();

      int actual = await data
        .ToChannelReader()
        .SkipWhile(x => x < 10)
        .TakeWhile(x => x < 50)
        .Skip(3)
        .Take(10)
        .Aggregate((s, a) => s + a);

      Assert.AreEqual(expected, actual);
    }

  }
}
