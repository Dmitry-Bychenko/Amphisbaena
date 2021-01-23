using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Amphisbaena {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Join (Attach) Channel Readers
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static class ChannelReaderJoin {
    #region Public

    /// <summary>
    /// Join Readers
    /// </summary>
    public static ChannelReader<T> Join<T>(this IEnumerable<ChannelReader<T>> readers,
                                                ChannelParallelOptions options) {
      if (readers is null)
        throw new ArgumentNullException(nameof(readers));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      ChannelReader<T>[] source = readers
        .Where(reader => reader is not null)
        .Distinct()
        .ToArray();

      if (source.Length == 0)
        return ChannelReaderFactory.Empty<T>();
      if (source.Length == 1)
        return source[0];

      Channel<T> result = op.CreateChannel<T>();

      Task.Run(async () => {
        async Task WriterTask(ChannelReader<T> reader) {
          await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false))
            await result.Writer.WriteAsync(item, op.CancellationToken).ConfigureAwait(false);
        }

        await Task.WhenAll(source.Select(reader => WriterTask(reader)).ToArray()).ConfigureAwait(false);

        result.Writer.TryComplete();
      }, op.CancellationToken);

      return result.Reader;
    }

    /// <summary>
    /// Join Readers
    /// </summary>
    public static ChannelReader<T> Join<T>(this IEnumerable<ChannelReader<T>> readers) =>
      Join(readers, default);

    /// <summary>
    /// Attach 
    /// </summary>
    public static ChannelReader<T> Attach<T>(this ChannelReader<T> source,
                                                  IEnumerable<ChannelReader<T>> readers,
                                                  ChannelParallelOptions options) {
      if (source is null)
        throw new ArgumentNullException(nameof(source));
      if (readers is null)
        throw new ArgumentNullException(nameof(readers));

      return Join(new ChannelReader<T>[] { source }.Concat(readers), options);
    }

    /// <summary>
    /// Attach 
    /// </summary>
    public static ChannelReader<T> Attach<T>(this ChannelReader<T> source,
                                                  IEnumerable<ChannelReader<T>> readers) =>
      Attach(source, readers, default);

    /// <summary>
    /// Attach 
    /// </summary>
    public static ChannelReader<T> Attach<T>(this IEnumerable<ChannelReader<T>> source,
                                                  IEnumerable<ChannelReader<T>> readers,
                                                  ChannelParallelOptions options) {
      if (source is null)
        throw new ArgumentNullException(nameof(source));
      if (readers is null)
        throw new ArgumentNullException(nameof(readers));

      return Join(source.Concat(readers), options);
    }

    /// <summary>
    /// Attach 
    /// </summary>
    public static ChannelReader<T> Attach<T>(this IEnumerable<ChannelReader<T>> source,
                                                  IEnumerable<ChannelReader<T>> readers) =>
      Attach(source, readers, default);

    #endregion Public
  }

}
