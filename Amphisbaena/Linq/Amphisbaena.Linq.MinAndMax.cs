using System;
using System.Collections.Generic;
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
    /// Min
    /// </summary>
    /// <param name="reader">Initial ChannelReader to aggregate</param>
    /// <param name="selector">Selector to find maximum</param>
    /// <param name="comparer">Comparer to use</param>
    /// <param name="options">Options</param>
    /// <exception cref="ArgumentNullException">When reader is null; when comparer is null and T is not comparable</exception>
    /// <exception cref="InvalidOperationException">When reader is empty</exception>
    public static async Task<R> Min<T, R>(this ChannelReader<T> reader,
                                               Func<T, R> selector,
                                               IComparer<R> comparer,
                                               ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));
      if (selector is null)
        throw new ArgumentNullException(nameof(selector));

      comparer ??= Comparer<R>.Default;

      if (comparer is null)
        throw new ArgumentNullException(nameof(comparer), $"{typeof(T).Name} doesn't provide default comparer");

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      R result = default;
      bool first = true;

      await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false)) {
        R next = selector(item);

        if (first || comparer.Compare(next, result) < 0) {
          first = false;

          result = next;
        }
      }

      if (first)
        throw new InvalidOperationException("Empty reader doesn't have min item.");

      return result;
    }

    /// <summary>
    /// Min
    /// </summary>
    /// <param name="reader">Initial ChannelReader to aggregate</param>
    /// <param name="selector">Selector to find maximum</param>
    /// <param name="options">Options</param>
    /// <exception cref="ArgumentNullException">When reader is null; when comparer is null and T is not comparable</exception>
    /// <exception cref="InvalidOperationException">When reader is empty</exception>
    public static async Task<R> Min<T, R>(this ChannelReader<T> reader,
                                               Func<T, R> selector,
                                               ChannelParallelOptions options) =>
      await Min(reader, selector, default, options);

    /// <summary>
    /// Min
    /// </summary>
    /// <param name="reader">Initial ChannelReader to aggregate</param>
    /// <param name="selector">Selector to find maximum</param>
    /// <param name="comparer">Comparer to use</param>
    /// <exception cref="ArgumentNullException">When reader is null; when comparer is null and T is not comparable</exception>
    /// <exception cref="InvalidOperationException">When reader is empty</exception>
    public static async Task<R> Min<T, R>(this ChannelReader<T> reader,
                                               Func<T, R> selector,
                                               IComparer<R> comparer) =>
      await Min(reader, selector, comparer, default);

    /// <summary>
    /// Min
    /// </summary>
    /// <param name="reader">Initial ChannelReader to aggregate</param>
    /// <param name="selector">Selector to find maximum</param>
    /// <exception cref="ArgumentNullException">When reader is null; when comparer is null and T is not comparable</exception>
    /// <exception cref="InvalidOperationException">When reader is empty</exception>
    public static async Task<R> Min<T, R>(this ChannelReader<T> reader,
                                               Func<T, R> selector) =>
      await Min(reader, selector, default, default);

    /// <summary>
    /// Min
    /// </summary>
    /// <param name="reader">Initial ChannelReader to aggregate</param>
    /// <param name="comparer">Comparer to use</param>
    /// <param name="options">Options</param>
    /// <exception cref="ArgumentNullException">When reader is null; when comparer is null and T is not comparable</exception>
    /// <exception cref="InvalidOperationException">When reader is empty</exception>
    public static async Task<T> Min<T>(this ChannelReader<T> reader,
                                            IComparer<T> comparer,
                                            ChannelParallelOptions options) =>
      await Min<T, T>(reader, x => x, comparer, options);


    /// <summary>
    /// Min
    /// </summary>
    /// <param name="reader">Initial ChannelReader to aggregate</param>
    /// <param name="comparer">Comparer to use</param>
    /// <exception cref="ArgumentNullException">When reader is null; when comparer is null and T is not comparable</exception>
    /// <exception cref="InvalidOperationException">When reader is empty</exception>
    public static async Task<T> Min<T>(this ChannelReader<T> reader, IComparer<T> comparer) =>
      await Min<T, T>(reader, x => x, comparer, default);

    /// <summary>
    /// Min
    /// </summary>
    /// <param name="reader">Initial ChannelReader to aggregate</param>
    /// <param name="options">Options</param>
    /// <exception cref="ArgumentNullException">When reader is null</exception>
    /// <exception cref="InvalidOperationException">When reader is empty</exception>
    public static async Task<T> Min<T>(this ChannelReader<T> reader, ChannelParallelOptions options)
      where T : IComparable<T> => await Min<T, T>(reader, x => x, default, options);

    /// <summary>
    /// Min
    /// </summary>
    /// <param name="reader">Initial ChannelReader to aggregate</param>
    /// <param name="options">Options</param>
    /// <exception cref="ArgumentNullException">When reader is null</exception>
    /// <exception cref="InvalidOperationException">When reader is empty</exception>
    public static async Task<T> Min<T>(this ChannelReader<T> reader)
      where T : IComparable<T> => await Min<T, T>(reader, x => x, default, default);

    /// <summary>
    /// Max
    /// </summary>
    /// <param name="reader">Initial ChannelReader to aggregate</param>
    /// <param name="selector">Selector to find maximum</param>
    /// <param name="comparer">Comparer to use</param>
    /// <param name="options">Options</param>
    /// <exception cref="ArgumentNullException">When reader is null; when comparer is null and T is not comparable</exception>
    /// <exception cref="InvalidOperationException">When reader is empty</exception>
    public static async Task<R> Max<T, R>(this ChannelReader<T> reader,
                                               Func<T, R> selector,
                                               IComparer<R> comparer,
                                               ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));
      if (selector is null)
        throw new ArgumentNullException(nameof(selector));

      comparer ??= Comparer<R>.Default;

      if (comparer is null)
        throw new ArgumentNullException(nameof(comparer), $"{typeof(T).Name} doesn't provide default comparer");

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      R result = default;
      bool first = true;

      await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false)) {
        R next = selector(item);

        if (first || comparer.Compare(next, result) > 0) {
          first = false;

          result = next;
        }
      }

      if (first)
        throw new InvalidOperationException("Empty reader doesn't have max item.");

      return result;
    }

    /// <summary>
    /// Max
    /// </summary>
    /// <param name="reader">Initial ChannelReader to aggregate</param>
    /// <param name="selector">Selector to find maximum</param>
    /// <param name="options">Options</param>
    /// <exception cref="ArgumentNullException">When reader is null; when comparer is null and T is not comparable</exception>
    /// <exception cref="InvalidOperationException">When reader is empty</exception>
    public static async Task<R> Max<T, R>(this ChannelReader<T> reader,
                                               Func<T, R> selector,
                                               ChannelParallelOptions options) =>
      await Max(reader, selector, default, options);

    /// <summary>
    /// Max
    /// </summary>
    /// <param name="reader">Initial ChannelReader to aggregate</param>
    /// <param name="selector">Selector to find maximum</param>
    /// <param name="comparer">Comparer to use</param>
    /// <exception cref="ArgumentNullException">When reader is null; when comparer is null and T is not comparable</exception>
    /// <exception cref="InvalidOperationException">When reader is empty</exception>
    public static async Task<R> Max<T, R>(this ChannelReader<T> reader,
                                               Func<T, R> selector,
                                               IComparer<R> comparer) =>
      await Max(reader, selector, comparer, default);

    /// <summary>
    /// Max
    /// </summary>
    /// <param name="reader">Initial ChannelReader to aggregate</param>
    /// <param name="selector">Selector to find maximum</param>
    /// <exception cref="ArgumentNullException">When reader is null; when comparer is null and T is not comparable</exception>
    /// <exception cref="InvalidOperationException">When reader is empty</exception>
    public static async Task<R> Max<T, R>(this ChannelReader<T> reader,
                                               Func<T, R> selector) =>
      await Max(reader, selector, default, default);

    /// <summary>
    /// Max
    /// </summary>
    /// <param name="reader">Initial ChannelReader to aggregate</param>
    /// <param name="comparer">Comparer to use</param>
    /// <param name="options">Options</param>
    /// <exception cref="ArgumentNullException">When reader is null; when comparer is null and T is not comparable</exception>
    /// <exception cref="InvalidOperationException">When reader is empty</exception>
    public static async Task<T> Max<T>(this ChannelReader<T> reader,
                                            IComparer<T> comparer,
                                            ChannelParallelOptions options) =>
      await Max<T, T>(reader, x => x, comparer, options);

    /// <summary>
    /// Max
    /// </summary>
    /// <param name="reader">Initial ChannelReader to aggregate</param>
    /// <param name="comparer">Comparer to use</param>
    /// <exception cref="ArgumentNullException">When reader is null; when comparer is null and T is not comparable</exception>
    /// <exception cref="InvalidOperationException">When reader is empty</exception>
    public static async Task<T> Max<T>(this ChannelReader<T> reader, IComparer<T> comparer) =>
      await Max<T, T>(reader, x => x, comparer, default);

    /// <summary>
    /// Max
    /// </summary>
    /// <param name="reader">Initial ChannelReader to aggregate</param>
    /// <param name="options">Options</param>
    /// <exception cref="ArgumentNullException">When reader is null</exception>
    /// <exception cref="InvalidOperationException">When reader is empty</exception>
    public static async Task<T> Max<T>(this ChannelReader<T> reader, ChannelParallelOptions options)
      where T : IComparable<T> => await Max<T, T>(reader, x => x, default, options);

    /// <summary>
    /// Max
    /// </summary>
    /// <param name="reader">Initial ChannelReader to aggregate</param>
    /// <exception cref="ArgumentNullException">When reader is null</exception>
    /// <exception cref="InvalidOperationException">When reader is empty</exception>
    public static async Task<T> Max<T>(this ChannelReader<T> reader)
      where T : IComparable<T> => await Max<T, T>(reader, x => x, default, default);

    #endregion Public
  }

}
