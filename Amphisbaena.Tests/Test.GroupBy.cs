using Amphisbaena.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Amphisbaena.Tests {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// 
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  [TestClass]
  public class TestGroupBy {
    #region Public

    [TestMethod]
    public async Task GroupEvenAndOdd() {
      int[] data = Enumerable
        .Range(0, 1000)
        .ToArray();

      int[] expected = data
        .GroupBy(item => item % 2)
        .Select(group => group.Sum())
        .ToArray();

      int[] result = await data
        .ToChannelReader()
        .GroupBy(item => item % 2)
        .Select(group => group.Reader.Aggregate((s, a) => s + a))
        .WhenAll()
        .ToArrayAsync();

      Assert.IsTrue(expected.SequenceEqual(result));
    }

    #endregion Public
  }
}
