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

  [TestCategory("Linq.Aggregate")]
  [TestClass]
  public sealed class AggregateTest {
    #region Private Data

    private static readonly long[] data = Enumerable
      .Range(0, 100)
      .Select(x => (long)x)
      .OrderBy(x => Guid.NewGuid())
      .ToArray();

    #endregion Private Data

    #region Tests

    [TestMethod("Sum 1 + 2 + ... + n")]
    public async Task TestSimpleSum() {
      long expected = data.Sum();

      long actual = await data
        .ToChannelReader()
        .Aggregate((s, a) => s + a);

      Assert.AreEqual(expected, actual);
    }

    [TestMethod("Sum (5 + 1 + 2 + ... + n) * 7")]
    public async Task TestElaboratedSum() {
      long expected = (data.Sum() + 5) * 7;

      long actual = await data
        .ToChannelReader()
        .Aggregate(5L, (s, a) => s + a, a => a * 7, new ChannelParallelOptions() { Capacity = 15 });

      Assert.AreEqual(expected, actual);
    }


    #endregion Tests
  }

}
