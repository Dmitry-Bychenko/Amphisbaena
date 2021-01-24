using Amphisbaena.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace Amphisbaena.Tests {

  [TestClass]
  public class TestStatistics {
    [TestMethod]
    public async Task SimpleStatistics() {
      int[] data = Enumerable
        .Range(0, 10)
        .Select(x => x)
        .ToArray();

      var stat = await data
        .ToChannelReader()
        .Statistics();

      Assert.AreEqual(9, stat.ArgMax);
      Assert.AreEqual(4.5, stat.Average);
    }
  }

}
