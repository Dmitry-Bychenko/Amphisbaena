using Amphisbaena.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
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
  public sealed class EnumerableTest {
    [TestMethod("To IAsyncEnumerable")]
    public async Task AsyncEnumerable() {
      int[] data = new [] { 1, 2, 3, 4 };

      List<int> result = new ();

      var q = data
        .ToChannelReader()
        .ToAsyncEnumerable();

      await foreach (int v in q.ConfigureAwait(false)) {
        result.Add(v);
      }

      Assert.IsTrue(data.SequenceEqual(result));
    }

    [TestMethod("To IEnumerable")]
    public void SyncEnumerable() {
      int[] data = new int[] { 1, 2, 3, 4 };

      var q = data
        .ToChannelReader()
        .ToEnumerable();

      Assert.IsTrue(data.SequenceEqual(q));
    }

  }
}
