using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Amphisbaena {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Channel Reader Factory
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static class ChannelReaderFactory {
    #region Public

    /// <summary>
    /// Empty
    /// </summary>
    public static ChannelReader<T> Empty<T>() {
      Channel<T> result = Channel.CreateBounded<T>(1);

      result.Writer.Complete();

      return result.Reader;
    }

    /// <summary>
    /// Range
    /// </summary>
    public static ChannelReader<long> Range(long start, long count, ChannelParallelOptions options) {
      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      Channel<long> result = op.CreateChannel<long>();

      Task.Run(async () => {
        for (long i = 0; i < count; ++i)
          await result.Writer.WriteAsync(i + start, op.CancellationToken).ConfigureAwait(false);

        result.Writer.TryComplete();
      }, op.CancellationToken);

      return result.Reader;
    }

    /// <summary>
    /// Range
    /// </summary>
    public static ChannelReader<long> Range(long start, long count) =>
      Range(start, count, default);

    /// <summary>
    /// Range
    /// </summary>
    public static ChannelReader<long> Range(long count, ChannelParallelOptions options) =>
      Range(0, count, options);

    /// <summary>
    /// Range
    /// </summary>
    public static ChannelReader<long> Range(long count) =>
      Range(0, count, default);

    /// <summary>
    /// To Channel Reader
    /// </summary>
    public static ChannelReader<T> ToChannelReader<T>(this IEnumerable<T> source,
                                                           ChannelParallelOptions options) {
      if (source is null)
        throw new ArgumentNullException(nameof(source));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      Channel<T> result = op.CreateChannel<T>();

      Task.Run(async () => {
        try {
          foreach (T item in source)
            await result.Writer.WriteAsync(item, op.CancellationToken).ConfigureAwait(false);
        }
        finally {
          result.Writer.TryComplete();
        }
      },
      op.CancellationToken);

      return result.Reader;
    }

    /// <summary>
    /// To Channel Reader
    /// </summary>
    public static ChannelReader<T> ToChannelReader<T>(this IEnumerable<T> source) =>
      ToChannelReader(source, new ChannelParallelOptions());

    /// <summary>
    /// To Channel Reader
    /// </summary>
    public static ChannelReader<T> ToChannelReader<T>(this IAsyncEnumerable<T> source,
                                                           ChannelParallelOptions options) {
      if (source is null)
        throw new ArgumentNullException(nameof(source));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      Channel<T> result = op.CreateChannel<T>();

      Task.Run(async () => {
        try {
          await foreach (T item in source.ConfigureAwait(false)) {
            await result.Writer.WriteAsync(item, op.CancellationToken).ConfigureAwait(false);
          }
        }
        finally {
          result.Writer.TryComplete();
        }
      },
      op.CancellationToken);

      return result.Reader;
    }

    /// <summary>
    /// To Channel Reader
    /// </summary>
    public static ChannelReader<T> ToChannelReader<T>(this IAsyncEnumerable<T> source) =>
      ToChannelReader(source, new ChannelParallelOptions());

    #endregion Public
  }

}
