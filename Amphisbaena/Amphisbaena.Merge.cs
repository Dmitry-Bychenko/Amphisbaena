using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Amphisbaena {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Merge (Attach) Channel Readers
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static class ChannelReaderMerge {
    #region Public

    /// <summary>
    /// Merge Readers
    /// </summary>
    public static ChannelReader<T> Merge<T>(this IEnumerable<ChannelReader<T>> readers,
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
    /// Merge Readers
    /// </summary>
    public static ChannelReader<T> Merge<T>(this IEnumerable<ChannelReader<T>> readers) =>
      Merge(readers, default);

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

      return Merge(new ChannelReader<T>[] { source }.Concat(readers), options);
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

      return Merge(source.Concat(readers), options);
    }

    /// <summary>
    /// Attach 
    /// </summary>
    public static ChannelReader<T> Attach<T>(this IEnumerable<ChannelReader<T>> source,
                                                  IEnumerable<ChannelReader<T>> readers) =>
      Attach(source, readers, default);

    /// <summary>
    /// Attach 
    /// </summary>
    public static ChannelReader<T> Attach<T>(this ChannelReader<T> source,
                                                  ChannelReader<T> other,
                                                  ChannelParallelOptions options) {
      if (source is null)
        throw new ArgumentNullException(nameof(source));
      if (other is null)
        throw new ArgumentNullException(nameof(other));
      if (ReferenceEquals(source, other))
        throw new ArgumentException("Attach to itself is not allowed", nameof(other));

      return Attach(source, new ChannelReader<T>[] { other }, options);
    }

    /// <summary>
    /// Attach 
    /// </summary>
    public static ChannelReader<T> Attach<T>(this ChannelReader<T> source, ChannelReader<T> other) =>
      Attach(source, other, default);

    /// <summary>
    /// Detach, process, Attach back 
    /// </summary>
    public static ChannelReader<T> DetachAttach<T>(this ChannelReader<T> source,
                                                        Func<T, bool> condition,
                                                        Func<ChannelReader<T>, ChannelReader<T>> process,
                                                        ChannelParallelOptions options) {
      if (source is null)
        throw new ArgumentNullException(nameof(source));
      if (condition is null)
        throw new ArgumentNullException(nameof(condition));
      if (process is null)
        throw new ArgumentNullException(nameof(process));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      Channel<T> result = op.CreateChannel<T>();
      Channel<T> detached = op.CreateChannel<T>();

      ChannelReader<T> detachedReader = process(detached.Reader);

      Task detachedTask = Task.Run(async () => {
        await foreach (T item in detachedReader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false)) {
          await result.Writer.WriteAsync(item, op.CancellationToken).ConfigureAwait(false);
        }
      }, op.CancellationToken);

      Task.Run(async () => {
        await foreach (T item in source.ReadAllAsync(op.CancellationToken).ConfigureAwait(false)) {
          if (!condition(item))
            await result.Writer.WriteAsync(item, op.CancellationToken).ConfigureAwait(false);
          else
            await detached.Writer.WriteAsync(item, op.CancellationToken).ConfigureAwait(false);
        }

        detached.Writer.TryComplete();

        await detachedTask.ConfigureAwait(false);

        result.Writer.TryComplete();
      }, op.CancellationToken);

      return result.Reader;
    }

    /// <summary>
    /// Detach, process, Attach back 
    /// </summary>
    public static ChannelReader<T> DetachAttach<T>(this ChannelReader<T> source,
                                                        Func<T, bool> condition,
                                                        Func<ChannelReader<T>, ChannelReader<T>> process) =>
      DetachAttach(source, condition, process, default);

    #endregion Public
  }

}
