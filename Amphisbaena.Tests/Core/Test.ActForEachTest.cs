using Amphisbaena.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Amphisbaena.Tests.Core {

  [TestClass]
  public class ActForEachTest {
    #region Public

    [TestMethod]
    public async Task ActForEach() {
      int[] data = Enumerable
        .Range(0, 10)
        .Select(i => i)
        .ToArray();

      ConcurrentQueue<int> result = new ();

      await data
        .ToChannelReader()
        .ActForEach(item => {
          result.Enqueue(item);
          Console.WriteLine(item);
        })
        .ToTask();

      int sum = result.Sum();

      Assert.AreEqual(45, sum);
    }

    #endregion Public
  }
}
