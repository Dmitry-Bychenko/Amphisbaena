using Amphisbaena.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
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


    [TestMethod]
    public async Task MinAndMaxDetach() {
      int[] data = Enumerable
        .Range(1, 10000)
        .Select(x => x * x % 12345)
        .ToArray();

      int expectedMin = data.Min();
      int expectedMax = data.Max();

      var minTask = data
        .ToChannelReader()
        .Detach(out var detached)
        .Min();

      var maxTask = detached.Max();

      await Task.WhenAll(minTask, maxTask);

      var actualMin = await minTask;
      var actualMax = await maxTask;

      Assert.AreEqual(expectedMax, actualMax);
      Assert.AreEqual(expectedMin, actualMin);
    }


    #endregion Public
  }

}
