using Amphisbaena.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace Amphisbaena.Tests.Linq {

  [TestCategory("Linq.Statistics")]
  [TestClass]
  public class ToBatchStatistics {
    [TestMethod("Simple Batch")]
    public async Task DoBatch() {
      int[] data = Enumerable
        .Range(0, 100)
        .ToArray();

      int[] actual = await data
        .ToChannelReader()
        .ToBatch((batch, item, index) => batch.Count < 4)
        .Skip(2)
        .FirstOrDefault();

      Assert.IsTrue(new int[] { 8, 9, 10, 11}.SequenceEqual(actual));
    }
  }
}
