using System;
using System.Linq;

using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Amphisbaena {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Channel Reader Split Strategy
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public enum ChannelReaderSplitStrategy {
    /// <summary>
    /// Default (Round Robin)
    /// </summary>
    Default = 0,

    /// <summary>
    /// Round Robin
    /// </summary>
    RoundRobin = Default,

    /// <summary>
    /// Balanced
    /// </summary>
    Balanced = 1,
  }

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
          await result[index].Writer.WriteAsync(item).ConfigureAwait(false);

          index = (index + 1) % count;
        }

        foreach (var channel in result)
          channel.Writer.Complete();
      }, token);

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

    /// <summary>
    /// Balanced Split
    /// </summary>
    /// <param name="reader">Reader To Split</param>
    /// <param name="count">Number of Chunks</param>
    public static ChannelReader<T>[] Balanced<T>(ChannelReader<T> reader,
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
        await foreach (T item in reader.ReadAllAsync(token).WithCancellation(token).ConfigureAwait(false)) {
          int minIndex = -1;
          int min = 0;

          for (int i = 0; i < result.Length; ++i) {
            int actualCount = result[i].Reader.Count;

            if (i == 0 || min > actualCount) {
              min = actualCount;
              minIndex = i;
            }
          }

          await result[minIndex].Writer.WriteAsync(item).ConfigureAwait(false);
        }

        foreach (var channel in result)
          channel.Writer.Complete();
      }, token);

      return result
        .Select(ch => ch.Reader)
        .ToArray();
    }

    /// <summary>
    /// Balanced Split
    /// </summary>
    /// <param name="reader">Reader To Split</param>
    /// <param name="count">Number of Chunks</param>
    public static ChannelReader<T>[] Balanced<T>(ChannelReader<T> reader, int count) =>
      Balanced(reader, count, default);

    #endregion Public
  }

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Channel Reader Extensions
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static partial class ChannelReaderExtensions {
    #region Public

    /// <summary>
    /// Split ChannelReader into several readers
    /// </summary>
    /// <param name="source">Initial Channel to Split</param>
    /// <param name="count">Number of readers to create</param>
    /// <param name="splitStrategy">Split Strategy</param>
    /// <param name="token">Cancellation token</param>
    public static ChannelReader<T>[] Split<T>(this ChannelReader<T> source,
                                                   int count,
                                                   ChannelReaderSplitStrategy splitStrategy,
                                                   CancellationToken token) {
      if (source is null)
        throw new ArgumentNullException(nameof(source));

      return splitStrategy switch {
        ChannelReaderSplitStrategy.Balanced => ChannelReaderSplit.Balanced(source, count, token),
        ChannelReaderSplitStrategy.RoundRobin => ChannelReaderSplit.RoundRobin(source, count, token),
        _ => ChannelReaderSplit.RoundRobin(source, count, token),
      };
    }

    /// <summary>
    /// Split ChannelReader into several readers
    /// </summary>
    /// <param name="source">Initial Channel to Split</param>
    /// <param name="count">Number of readers to create</param>
    /// <param name="splitStrategy">Split Strategy</param>
    public static ChannelReader<T>[] Split<T>(this ChannelReader<T> source,
                                                   int count,
                                                   ChannelReaderSplitStrategy splitStrategy) =>
      Split(source, count, splitStrategy, default);

    /// <summary>
    /// Split ChannelReader into several readers
    /// </summary>
    /// <param name="source">Initial Channel to Split</param>
    /// <param name="count">Number of readers to create</param>
    /// <param name="token">Cancellation token</param>
    public static ChannelReader<T>[] Split<T>(this ChannelReader<T> source,
                                                   int count,
                                                   CancellationToken token) =>
      Split(source, count, ChannelReaderSplitStrategy.Default, token);

    /// <summary>
    /// Split ChannelReader into several readers
    /// </summary>
    /// <param name="source">Initial Channel to Split</param>
    public static ChannelReader<T>[] Split<T>(this ChannelReader<T> source, int count) =>
      Split(source, count, ChannelReaderSplitStrategy.Default, default);

    #endregion Public
  }

}
