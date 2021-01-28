using Amphisbaena.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
  public class TestFork {
    #region Public

    [TestMethod]
    public async Task MinAndMaxFork() {
      int[] data = Enumerable
        .Range(0, 10000)
        .Select(x => x)
        .ToArray();

      var forks = data
        .ToChannelReader()
        .Fork(x => x % 2 == 0,
              x => x % 2 != 0);

      var evenTask = forks[0].Max();
      var oddTask = forks[1].Max();

      await Task.WhenAll(evenTask, oddTask);

      int even = await evenTask;
      int odd = await oddTask;

      Assert.AreEqual(9998, even);
      Assert.AreEqual(9999, odd);
    }


    #endregion Public
  }

}
