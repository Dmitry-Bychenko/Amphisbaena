using Amphisbaena.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Amphisbaena.Tests {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Factory Tests
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  [TestClass]
  public sealed class AggregateTest {
    #region Tests

    /// <summary>
    /// Sum of 1 + 2 + 3 + ... + 100
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task SumOfNumber() {
      var data = Enumerable
        .Range(1, 100)
        .Select(index => (long)index)
        .ToArray();

      long expected = data.Sum();

      long actual = await data
        .ToChannelReader()
        .Aggregate((s, a) => s + a);
      
      Assert.IsTrue(expected == actual);
    }



    #endregion Tests
  }

}
