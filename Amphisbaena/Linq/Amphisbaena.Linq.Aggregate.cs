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
    /// Aggregate
    /// </summary>
    public static async Task<T> Aggregate<T, A, S>(this ChannelReader<S> reader, 
                                                        A seed,
                                                        Func<A, S, A> accumulate,
                                                        Func<A, T> result,
                                                        ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));
      if (accumulate is null)
        throw new ArgumentNullException(nameof(accumulate));
      if (result is null)
        throw new ArgumentNullException(nameof(result));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      A outcome = seed;

      await foreach (S item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false))
        outcome = accumulate(outcome, item);

      return result(outcome);
    }

    /// <summary>
    /// Aggregate
    /// </summary>
    public static async Task<T> Aggregate<T, A, S>(this ChannelReader<S> reader,
                                                        A seed,
                                                        Func<A, S, A> accumulate,
                                                        Func<A, T> result) =>
      await Aggregate(reader, seed, accumulate, result, default);

    /// <summary>
    /// Aggregate
    /// </summary>
    public static async Task<T> Aggregate<T, S>(this ChannelReader<S> reader,
                                                     T seed,
                                                     Func<T, S, T> accumulate,
                                                     ChannelParallelOptions options) =>
      await Aggregate(reader, seed, accumulate, x => x, options);

    /// <summary>
    /// Aggregate
    /// </summary>
    public static async Task<T> Aggregate<T, S>(this ChannelReader<S> reader,
                                                     T seed,
                                                     Func<T, S, T> accumulate) =>
      await Aggregate(reader, seed, accumulate, x => x, default);

    /// <summary>
    /// Aggregate
    /// </summary>
    public static async Task<T> Aggregate<T>(this ChannelReader<T> reader,
                                                  Func<T, T, T> accumulate,
                                                  ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));
      if (accumulate is null)
        throw new ArgumentNullException(nameof(accumulate));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      T outcome = default;
      bool first = true;

      await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false)) {
        if (first) {
          first = false;
          outcome = item;
        }
        else  
          outcome = accumulate(outcome, item);
      }

      if (first)
        throw new InvalidOperationException("Empty reader can't be aggregated.");

      return outcome;
    }

    /// <summary>
    /// Aggregate
    /// </summary>
    public static async Task<T> Aggregate<T>(this ChannelReader<T> reader,
                                                  Func<T, T, T> accumulate) =>
      await Aggregate<T>(reader, accumulate, default);

    /// <summary>
    /// Count 
    /// </summary>
    public static async Task<long> Count<T> (this ChannelReader<T> reader, ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      long result = 0;

      await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false))
        result += 1;

      return result;
    }

    /// <summary>
    /// Count 
    /// </summary>
    public static async Task<long> Count<T>(this ChannelReader<T> reader) =>
      await Count(reader, default);

    /// <summary>
    /// Min
    /// </summary>
    public static async Task<T> Min<T>(this ChannelReader<T> reader, 
                                            IComparer<T> comparer,
                                            ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));

      comparer ??= Comparer<T>.Default;

      if (comparer is null)
        throw new ArgumentNullException(nameof(comparer), $"{typeof(T).Name} doesn't provide default comparer");

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      T result = default;
      bool first = true;

      await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false))
        if (first || comparer.Compare(item, result) < 0) {
          first = false;

          result = item;
        }

      if (first)
        throw new InvalidOperationException("Empty reader doesn't have min item.");

      return result;
    }

    /// <summary>
    /// Min
    /// </summary>
    public static async Task<T> Min<T>(this ChannelReader<T> reader, IComparer<T> compare) =>
      await Min(reader, compare, default);

    /// <summary>
    /// Min
    /// </summary>
    public static async Task<T> Min<T>(this ChannelReader<T> reader, ChannelParallelOptions options)
      where T : IComparable<T> => await Min<T>(reader, default, options);

    /// <summary>
    /// Min
    /// </summary>
    public static async Task<T> Min<T>(this ChannelReader<T> reader)
      where T : IComparable<T> => await Min<T>(reader, default, default);

    /// <summary>
    /// Max
    /// </summary>
    public static async Task<T> Max<T>(this ChannelReader<T> reader,
                                            IComparer<T> comparer,
                                            ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));

      comparer ??= Comparer<T>.Default;

      if (comparer is null)
        throw new ArgumentNullException(nameof(comparer), $"{typeof(T).Name} doesn't provide default comparer");

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      T result = default;
      bool first = true;

      await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false))
        if (first || comparer.Compare(item, result) > 0) {
          first = false;

          result = item;
        }

      if (first)
        throw new InvalidOperationException("Empty reader doesn't have max item.");

      return result;
    }

    /// <summary>
    /// Max
    /// </summary>
    public static async Task<T> Max<T>(this ChannelReader<T> reader, IComparer<T> compare) =>
      await Max(reader, compare, default);

    /// <summary>
    /// Max
    /// </summary>
    public static async Task<T> Max<T>(this ChannelReader<T> reader, ChannelParallelOptions options)
      where T : IComparable<T> => await Max<T>(reader, default, options);

    /// <summary>
    /// Max
    /// </summary>
    public static async Task<T> Max<T>(this ChannelReader<T> reader)
      where T : IComparable<T> => await Max<T>(reader, default, default);

    #endregion Public
  }

}
