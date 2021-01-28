using Amphisbaena.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Linq;
using System.Threading.Tasks;

namespace Amphisbaena.Tests.Core {

  [TestClass]
  public class SpreadTest {
    [TestMethod("Min and Max")]
    public async Task MinMax() {
      int[] data = Enumerable
        .Range(1, 100)
        .ToArray();

      var channels = data
        .ToChannelReader()
        .Spread(2);

      Task<int>[] next = new Task<int>[] {
        channels[0].Min(),
        channels[1].Max()
      };

      await Task.WhenAll(next);

      int min = await next[0];
      int max = await next[1];

      Assert.AreEqual(1, min);
      Assert.AreEqual(100, max);
    }
  }
}
