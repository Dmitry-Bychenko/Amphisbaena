using Amphisbaena.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Amphisbaena.Tests.Core {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// ForAll tests
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  [TestClass]
  public class TestPerform {
    #region Public

    /// <summary>
    /// Sum Of Squares with a help of ForAll 
    /// </summary>
    [TestMethod]
    public async Task ForAllSumOfSquares() {
      long[] data = Enumerable
        .Range(1, 1000)
        .Select(i => (long)i)
        .ToArray();

      long expected = data
        .Sum(item => item * item);

      ConcurrentDictionary<long, long> dict = new ConcurrentDictionary<long, long>();

      await data
        .ToChannelReader()
        .ForAll((item) => dict.TryAdd(item, item * item));

      long actual = dict.Values.Sum(item => item);

      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Sum of squares via ForEach 
    /// </summary>
    [TestMethod]
    public async Task ForEachSumOfSquares() {
      long[] data = Enumerable
        .Range(1, 1000)
        .Select(i => (long)i)
        .ToArray();

      long expected = data
        .Sum(item => item * item);

      var array = await data
        .ToChannelReader()
        .ForEach(x => x * x)
        .ToArrayAsync();

      long actual = array.Sum();

      Assert.AreEqual(expected, actual);
    }

    #endregion Public
  }

}
