using Amphisbaena;
using Amphisbaena.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Amphisbaena.Tests.Complex {
  [TestClass]
  public sealed class FactoryTest {
    #region Public

    [TestMethod]
    public async Task ComplexAggregation() {
      int[] data = Enumerable
        .Range(0, 1000)
        .ToArray();

      int expected = data.Sum();

      int actual = await data
        .ToChannelReader()
        .ToBatch((batch, item, index) => batch.Count < 100)
        .ForEach(batch => batch.Sum(), new ChannelParallelOptions() { DegreeOfParallelism = 4 })
        .Aggregate((s, a) => s + a);

      Assert.AreEqual(expected, actual);
    }

    [TestMethod("Detach even")]
    public async Task DetachTest() {
      int[] data = Enumerable
        .Range(0, 10)
        .ToArray();

      int expected = data
        .Select(x => x % 2 == 0 ? -x : x)
        .Sum();

      int sumOdd = await data
        .ToChannelReader()
        .Detach(out var even, item => item % 2 == 0)
        .Aggregate((s, a) => s + a);

      int sumEven = await even
        .Aggregate(0, (s, a) => s - a);

      Assert.AreEqual(expected, sumOdd + sumEven);
    }

    #endregion Public
  }

}
