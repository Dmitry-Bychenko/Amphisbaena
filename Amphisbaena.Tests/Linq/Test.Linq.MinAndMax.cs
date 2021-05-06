using Amphisbaena.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Amphisbaena.Tests.Linq {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Min And Max
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  [TestCategory("Linq.MinAndMax")]
  [TestClass]
  public sealed class MinAnMaxTest {
    #region Private Data

    private static readonly long[] data = Enumerable
      .Range(0, 100)
      .Select(x => (long)x)
      .OrderBy(x => Guid.NewGuid())
      .ToArray();

    #endregion Private Data

    [TestMethod("Min({0 .. 99})")]
    public async Task TestMin() {
      long actual = await data
        .ToChannelReader()
        .Min();

      Assert.AreEqual(data.Min(x => x), actual);
    }

    [TestMethod("Max({0 .. 99})")]
    public async Task TestMax() {
      long actual = await data
        .ToChannelReader()
        .Max();

      Assert.AreEqual(data.Max(), actual);
    }

    [TestMethod("Extremum")]
    public async Task TestExtremum() {
      var (_, min, _, _, max, _) = await data
        .ToChannelReader()
        .Extremum(item => item);

      Assert.AreEqual(min, data.Min());
      Assert.AreEqual(max, data.Max());
    }
  }
}
