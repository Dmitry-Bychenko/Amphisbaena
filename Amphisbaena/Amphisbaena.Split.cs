using System;
using System.Linq;
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
    /// <param name="reader">reader to split</param>
    /// <param name="options">parallel options</param>
    /// <exception cref="ArgumentNullException">When reader is null</exception>
    public static ChannelReader<T>[] Split<T>(this ChannelReader<T> reader,
                                                   ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions() { DegreeOfParallelism = Environment.ProcessorCount }
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      if (op.DegreeOfParallelism == 1)
        return new ChannelReader<T>[] { reader };

      Channel<T>[] result = Enumerable
        .Range(0, op.DegreeOfParallelism)
        .Select(_x => op.CreateChannel<T>())
        .ToArray();

      Task.Run(async () => {
        var balancer = op.CreateBalancer(result);

        await foreach (var item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false)) {
          Channel<T> channel = balancer.NextActor();

          await channel.Writer.WriteAsync(item, op.CancellationToken).ConfigureAwait(false);
        }

        foreach (var channel in result)
          channel.Writer.TryComplete();
      }, op.CancellationToken);

      return result
        .Select(ch => ch.Reader)
        .ToArray();
    }

    /// <summary>
    /// Split into several readers
    /// </summary>
    /// <param name="reader">reader to split</param>
    /// <exception cref="ArgumentNullException">When reader is null</exception>
    public static ChannelReader<T>[] Split<T>(this ChannelReader<T> reader) =>
      Split(reader, new ChannelParallelOptions() {
        DegreeOfParallelism = Environment.ProcessorCount
      });

    #endregion Public
  }

}
