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

      var maxTask = forks[0].Max();
      var minTask = forks[1].Min();

      await Task.WhenAll(maxTask, minTask);

      int max = await maxTask;
      int min = await minTask;

      Assert.AreEqual(9998, max);
      Assert.AreEqual(1, min);
    }


    #endregion Public
  }

}
