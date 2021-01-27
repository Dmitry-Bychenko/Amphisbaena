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

  [TestCategory("Linq.Distinct")]
  [TestClass()]
  public sealed class DistinctTest {
    [TestMethod("Distinct remove")]
    public async Task DistinctRemove() {
      int[] data = new int[] { 1, 2, 3, 4, 2, 2, 3 };

      bool result = await data
        .ToChannelReader()
        .Distinct()
        .SequenceEquals(new int[] { 1, 2, 3, 4 });

      Assert.IsTrue(result);
    }

    [TestMethod("Distinct keep")]
    public async Task DistinctKeep() {
      int[] data = new int[] { 1, 3, 2, 4 };

      bool result = await data
        .ToChannelReader()
        .Distinct()
        .SequenceEquals(data);

      Assert.IsTrue(result);
    }
  }
}
