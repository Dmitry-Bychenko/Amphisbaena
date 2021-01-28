using Amphisbaena.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Linq;
using System.Threading.Tasks;

namespace Amphisbaena.Tests.Core {

  [TestClass]
  public class MergeTest {
    [TestMethod("Simple Merge")]
    public async Task SimpleMerge() {
      int[] data = Enumerable
       .Range(1, 120)
       .ToArray();

      var channels = data
        .ToChannelReader()
        .Split(new ChannelParallelOptions());

      var next = channels
        .Select(channel => channel.Select(item => item * 2))
        .ToArray();

      int result = await next
        .Merge()
        .Select(x => x / 2)
        .Aggregate((s, a) => s + a);

      Assert.IsTrue(data.Sum() == result);

    }

  }
}
