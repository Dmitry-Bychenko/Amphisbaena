using Amphisbaena.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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

  [TestCategory("Linq.Concat")]
  [TestClass()]
  public sealed class ConcatTest {
    [TestMethod("Simple Concat")]
    public async Task DistinctRemove() {
      int[] data = new int[] { 1, 2, 3 };

      bool result = await data
        .ToChannelReader()
        .Concat(new int[] { 4, 5},
                Array.Empty<int>(),
                new int[] { 6})
        .Distinct()
        .SequenceEquals(new int[] { 1, 2, 3, 4, 5, 6 });

      Assert.IsTrue(result);
    }
  }

}
