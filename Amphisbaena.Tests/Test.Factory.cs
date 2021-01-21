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
  public sealed class FactoryTest {
    #region Tests

    /// <summary>
    /// Create Channel from an array and materialize it back to array
    /// </summary>
    /// <returns></returns>
    [TestMethod]
    public async Task SimpleCreation() {
      int[] data = Enumerable
        .Range(1, 10)
        .Select(index => index)
        .ToArray();

      int[] result = await data.ToChannelReader().ToArrayAsync();

      Assert.IsTrue(data.SequenceEqual(result));
    }

    /// <summary>
    /// Run and cancel in the middle
    /// </summary>
    [TestMethod]
    public async Task SimpleCreationCancelled() {
      int[] data = Enumerable
        .Range(1, 100)
        .Select(index => index)
        .ToArray();

      List<int> result = new List<int>();

      int bufferSize = 7;

      using CancellationTokenSource cts = new CancellationTokenSource();

      var token = cts.Token;

      try {
        var channel = data.ToChannelReader(new ChannelParallelOptions() {
          CancellationToken = token,
          Capacity = bufferSize
        }); //  (bufferSize, token);

        await foreach (var item in channel.ReadAllAsync(token)) {
          result.Add(item);

          if (result.Count >= bufferSize)
            cts.Cancel();
        }

        Assert.IsTrue(false, "Hasn't been cancelled/");
      }
      catch (TaskCanceledException) {
        Assert.IsTrue(data.Take(result.Count).SequenceEqual(result) && result.Count < data.Length);
      }

    }

    #endregion Tests
  }
}
