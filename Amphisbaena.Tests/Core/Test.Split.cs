using Amphisbaena.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;
using System.Collections.Concurrent;

using System.Diagnostics;

using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Amphisbaena.Tests.Core {
  
  [TestClass]
  public class SplitTest {
    [TestMethod("Simple Split")]
    public async Task SplitAggregate() {
      int[] data = Enumerable
        .Range(1, 120)
        .ToArray();

      int result = 0;

      var channels = data
        .ToChannelReader()
        .Split(new ChannelParallelOptions());

      Task<int>[] sums = channels
        .Select(channel => channel.Aggregate((s, a) => s + a))
        .ToArray();

      await Task.WhenAll(sums);

      foreach (var t in sums)
        result += await t;

      Assert.IsTrue(data.Sum() == result);
    }
  }
}
