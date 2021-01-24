using Amphisbaena;
using Amphisbaena.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Amphisbaena.Tests {

  [TestClass]
  public class ActForEachTest {
    #region Public

    [TestMethod]
    public async Task ActForEach() {
      int[] data = Enumerable
        .Range(0, 10)
        .Select(i => i)
        .ToArray();

      ConcurrentQueue<int> result = new ConcurrentQueue<int>();

      await data
        .ToChannelReader()
        .ActForEach(item => result.Enqueue(item))
        .ToTask();

      int sum = result.Sum();

      Assert.AreEqual(45, sum);
    }

    #endregion Public
  }
}
