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

  [TestCategory("Linq.Select")]
  [TestClass]
  public sealed class SelectTest {
    
    [TestMethod("SelectMany")]
    public async Task SelectManyTest() {
      int[][] data = new int[][] {
        new int[] { 1, 2, 3 },
        new int[] { 4 },
        new int[] { 5, 6, 7, 8 },
        new int[] { 9, 10},
      };

      int expected = data
        .Select(line => line.Sum())
        .Sum();

      int actual = await data
        .ToChannelReader()
        .SelectMany(line => line.ToChannelReader())
        .Aggregate((s, a) => s + a);

      Assert.AreEqual(expected, actual);
    }
  }
}
