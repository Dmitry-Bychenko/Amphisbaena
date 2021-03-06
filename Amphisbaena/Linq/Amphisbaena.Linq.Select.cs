﻿using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Amphisbaena.Linq {

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
    /// Select 
    /// </summary>
    public static ChannelReader<T> Select<T, S>(this ChannelReader<S> reader,
                                                     Func<S, long, T> map,
                                                     ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));
      if (map is null)
        throw new ArgumentNullException(nameof(map));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      Channel<T> result = op.CreateChannel<T>();

      Task.Run(async () => {
        long index = -1;

        await foreach (S item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false))
          await result.Writer.WriteAsync(map(item, ++index), op.CancellationToken).ConfigureAwait(false);

        result.Writer.TryComplete();
      }, op.CancellationToken);

      return result.Reader;
    }

    /// <summary>
    /// Select 
    /// </summary>
    public static ChannelReader<T> Select<T, S>(this ChannelReader<S> reader, Func<S, long, T> map) =>
      Select(reader, map, default);

    /// <summary>
    /// Select 
    /// </summary>
    public static ChannelReader<T> Select<T, S>(this ChannelReader<S> reader,
                                                     Func<S, T> map,
                                                     ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));
      if (map is null)
        throw new ArgumentNullException(nameof(map));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      Channel<T> result = op.CreateChannel<T>();

      Task.Run(async () => {
        await foreach (S item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false))
          await result.Writer.WriteAsync(map(item), op.CancellationToken).ConfigureAwait(false);

        result.Writer.TryComplete();
      }, op.CancellationToken);

      return result.Reader;
    }

    /// <summary>
    /// Select 
    /// </summary>
    public static ChannelReader<T> Select<T, S>(this ChannelReader<S> reader, Func<S, T> map) =>
      Select(reader, map, default);

    /// <summary>
    /// Select Many
    /// </summary>
    public static ChannelReader<T> SelectMany<T, S>(this ChannelReader<S> reader,
                                                         Func<S, long, ChannelReader<T>> selector,
                                                         ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));
      if (selector is null)
        throw new ArgumentNullException(nameof(selector));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      Channel<T> result = op.CreateChannel<T>();

      Task.Run(async () => {
        long index = -1;

        await foreach (S item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false)) {
          index += 1;

          ChannelReader<T> inner = selector(item, index);

          if (inner is null)
            continue;

          await foreach (T rec in inner.ReadAllAsync(op.CancellationToken).ConfigureAwait(false)) {
            await result.Writer.WriteAsync(rec, op.CancellationToken).ConfigureAwait(false);
          }
        }

        result.Writer.TryComplete();
      }, op.CancellationToken);

      return result.Reader;
    }

    /// <summary>
    /// Select Many
    /// </summary>
    public static ChannelReader<T> SelectMany<T, S>(this ChannelReader<S> reader,
                                                         Func<S, long, ChannelReader<T>> selector) =>
      SelectMany(reader, selector, default);

    /// <summary>
    /// Select Many
    /// </summary>
    public static ChannelReader<T> SelectMany<T, S>(this ChannelReader<S> reader,
                                                         Func<S, ChannelReader<T>> selector,
                                                         ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));
      if (selector is null)
        throw new ArgumentNullException(nameof(selector));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      Channel<T> result = op.CreateChannel<T>();

      Task.Run(async () => {
        await foreach (S item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false)) {
          ChannelReader<T> inner = selector(item);

          if (inner is null)
            continue;

          await foreach (T rec in inner.ReadAllAsync(op.CancellationToken).ConfigureAwait(false)) {
            await result.Writer.WriteAsync(rec, op.CancellationToken).ConfigureAwait(false);
          }
        }

        result.Writer.TryComplete();
      }, op.CancellationToken);

      return result.Reader;
    }

    /// <summary>
    /// Select Many
    /// </summary>
    public static ChannelReader<T> SelectMany<T, S>(this ChannelReader<S> reader,
                                                         Func<S, ChannelReader<T>> selector) =>
      SelectMany(reader, selector, default);

    #endregion Public
  }

}
