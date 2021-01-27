using Amphisbaena.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace Amphisbaena.Tests.Linq {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Factory Tests
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  [TestCategory("Linq.CastAndOfType")]
  [TestClass()]
  public sealed class CastAndOfTypeTest {
    [TestMethod("Cast int to long")]
    public async Task CastIntToLong() {
      int[] data = new int[] { 1, 2, 3 };

      long actual = await data
        .ToChannelReader()
        .Cast<long, int>()
        .Aggregate((s, a) => s + a);

      Assert.AreEqual(6, actual);
    }

    [TestMethod("OfType int to long")]
    public async Task OfTypeIntToLong() {
      int[] data = new int[] { 1, 2, 3 };

      long actual = await data
        .ToChannelReader()
        .OfType<long, int>()
        .Aggregate((s, a) => s + a);

      Assert.AreEqual(6, actual);
    }
  }

}
