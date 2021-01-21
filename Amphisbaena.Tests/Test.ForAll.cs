using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Amphisbaena.Tests {
  
  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// ForAll tests
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  [TestClass]
  public class TestForAll {
    #region Public

    /// <summary>
    /// Sum Of Squares with a help of ForAll 
    /// </summary>
    [TestMethod]
    public async Task ForAllSumOfSquares() {
      long[] data = Enumerable
        .Range(1, 1000)
        .Select(i => (long) i)
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

    #endregion Public
  }

}
