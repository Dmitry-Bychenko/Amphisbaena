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

  public static partial class ChannelReaderSpread {
    #region Public

    /// <summary>
    /// Spread into several readers
    /// </summary>
    /// <param name="reader">reader to spread</param>
    /// <param name="options">parallel options</param>
    /// <exception cref="ArgumentNullException">When reader is null</exception>
    public static ChannelReader<T>[] Spread<T>(this ChannelReader<T> reader,
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
        await foreach (var item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false)) {
          foreach (var channel in result)
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
    /// Spread into several readers
    /// </summary>
    /// <param name="reader">reader to spread</param>
    /// <param name="count">number of channels to create</param>
    /// <exception cref="ArgumentNullException">When reader is null</exception>
    public static ChannelReader<T>[] Spread<T>(this ChannelReader<T> reader, int count) =>
      Spread(reader, new ChannelParallelOptions() {
        DegreeOfParallelism = count
      });

    #endregion Public
  }

}
