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
    /// <param name="accumulate">Accumulate function</param>
    /// <param name="options">Options</param>
    /// <param name="reader">Initial ChannelReader to aggregate</param>
    /// <param name="seed">Initial value for accumulation</param>
    /// <param name="result">Final accumulated value conversion function</param>
    /// <exception cref="ArgumentNullException">When reader or accumulate is null</exception>
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
    /// <param name="accumulate">Accumulate function</param>
    /// <param name="reader">Initial ChannelReader to aggregate</param>
    /// <param name="seed">Initial value for accumulation</param>
    /// <param name="result">Final accumulated value conversion function</param>
    /// <exception cref="ArgumentNullException">When reader or accumulate is null</exception>
    public static async Task<T> Aggregate<T, A, S>(this ChannelReader<S> reader,
                                                        A seed,
                                                        Func<A, S, A> accumulate,
                                                        Func<A, T> result) =>
      await Aggregate(reader, seed, accumulate, result, default);

    /// <summary>
    /// Aggregate
    /// </summary>
    /// <param name="accumulate">Accumulate function</param>
    /// <param name="options">Options</param>
    /// <param name="reader">Initial ChannelReader to aggregate</param>
    /// <param name="seed">Initial value for accumulation</param>
    /// <exception cref="ArgumentNullException">When reader or accumulate is null</exception>
    public static async Task<T> Aggregate<T, S>(this ChannelReader<S> reader,
                                                     T seed,
                                                     Func<T, S, T> accumulate,
                                                     ChannelParallelOptions options) =>
      await Aggregate(reader, seed, accumulate, x => x, options);

    /// <summary>
    /// Aggregate
    /// </summary>
    /// <param name="accumulate">Accumulate function</param>
    /// <param name="reader">Initial ChannelReader to aggregate</param>
    /// <param name="seed">Initial value for accumulation</param>
    /// <exception cref="ArgumentNullException">When reader or accumulate is null</exception>
    public static async Task<T> Aggregate<T, S>(this ChannelReader<S> reader,
                                                     T seed,
                                                     Func<T, S, T> accumulate) =>
      await Aggregate(reader, seed, accumulate, x => x, default);

    /// <summary>
    /// Aggregate
    /// </summary>
    /// <param name="reader">Initial ChannelReader to aggregate</param>
    /// <param name="accumulate">Accumulate function</param>
    /// <param name="options">Options</param>
    /// <exception cref="ArgumentNullException">When reader or accumulate is null</exception>
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
    /// <param name="accumulate">Accumulate function</param>
    /// <param name="reader">Initial ChannelReader to aggregate</param>
    /// <exception cref="ArgumentNullException">When reader or accumulate is null</exception>
    public static async Task<T> Aggregate<T>(this ChannelReader<T> reader,
                                                  Func<T, T, T> accumulate) =>
      await Aggregate<T>(reader, accumulate, default);

    #endregion Public
  }

}
