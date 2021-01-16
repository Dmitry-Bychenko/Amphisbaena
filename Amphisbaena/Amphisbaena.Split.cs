using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Amphisbaena {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Split (parallelize) Reader into several
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static class ChannelReaderSplit {
    #region Public

    /// <summary>
    /// Round Robin Split
    /// </summary>
    /// <param name="reader">Reader To Split</param>
    /// <param name="count">Number of Chunks</param>
    /// <param name="token">Cancellation Token</param>
    public static ChannelReader<T>[] RoundRobin<T>(ChannelReader<T> reader, 
                                                   int count,
                                                   CancellationToken token) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));
      if (count <= 0)
        throw new ArgumentOutOfRangeException(nameof(count));

      token.ThrowIfCancellationRequested();

      if (1 == count)
        return new ChannelReader<T>[] { reader };

      var result = new Channel<T>[count];

      for (int i = 0; i < count; i++)
        result[i] = Channel.CreateUnbounded<T>();

      Task.Run(async () => {
        var index = 0;

        await foreach (var item in reader.ReadAllAsync(token).WithCancellation(token).ConfigureAwait(false)) {
          await result[index].Writer.WriteAsync(item);

          index = (index + 1) % count;
        }

        foreach (var channel in result)
          channel.Writer.Complete();
      });

      return result
        .Select(ch => ch.Reader)
        .ToArray();
    }

    /// <summary>
    /// Round Robin Split
    /// </summary>
    /// <param name="reader">Reader To Split</param>
    /// <param name="count">Number of Chunks</param>
    public static ChannelReader<T>[] RoundRobin<T>(ChannelReader<T> reader, int count) =>
      RoundRobin(reader, count, default);

    #endregion Public
  }
}
