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

    #endregion Public
  }

}
