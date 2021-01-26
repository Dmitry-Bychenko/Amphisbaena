using Amphisbaena.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace Amphisbaena.Tests.Linq {
  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// 
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  [TestCategory("Linq.GroupByAdjacent")]
  [TestClass]
  public class TestGroupByAdjacent {
    #region Public

    [TestMethod("Group Adjacent Even And Odd")]
    public async Task GroupEvenAndOdd() {
      int[] data = new int[] { 1, 3, 5, 2, 4, 7 };

      int[] expected = new int[] { 9, 6, 7 };

      int[] result = await data
        .ToChannelReader()
        .GroupByAdjacent(item => item % 2)
        .Select(group => group.Reader.Aggregate((s, a) => s + a))
        .WhenAll()
        .ToArrayAsync();

      Assert.IsTrue(expected.SequenceEqual(result));
    }

    #endregion Public
  }
}
