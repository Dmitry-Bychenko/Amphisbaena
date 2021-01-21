using System;
using System.Linq;

using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Amphisbaena {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Split Channel Reader into several ones
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static class ChannelReaderSplit {
    #region Public

    /// <summary>
    /// Split into several readers
    /// </summary>
    public static ChannelReader<T>[] Split<T>(this ChannelReader<T> source, 
                                                   ChannelParallelOptions options) {
      if (source is null)
        throw new ArgumentNullException(nameof(source));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      if (op.Capacity == 1)
        return new ChannelReader<T>[] { source };

      Channel<T>[] result = Enumerable
        .Range(0, op.Capacity)
        .Select(_x => op.CreateChannel<T>())
        .ToArray();

      Task.Run(async () => {
        var balancer = op.BalancingStrategy.Create(result);

        await foreach (var item in source.ReadAllAsync(op.CancellationToken).ConfigureAwait(false)) {
          Channel<T> channel = balancer.Next();

          await channel.Writer.WriteAsync(item).ConfigureAwait(false);
        }

        foreach (var channel in result)
          channel.Writer.TryComplete();
      }, op.CancellationToken);

      return result
        .Select(ch => ch.Reader)
        .ToArray();
    }

    #endregion Public
  }

}
