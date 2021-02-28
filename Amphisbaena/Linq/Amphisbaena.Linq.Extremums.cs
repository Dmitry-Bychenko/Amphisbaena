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
    /// Extremal Values
    /// </summary>
    public static async Task<(T ArgMin, R Min, long MinIndex, T ArgMax, R Max, long MaxIndex)> Extremum<T, R>
      (this ChannelReader<T> reader,
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

      R min = default;
      R max = default;
      T argMin = default;
      T argMax = default;
      long minIndex = 0;
      long maxIndex = 0;

      long index = -1;

      await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false)) {
        R next = selector(item);
        index += 1;

        if (0 == index) {
          argMin = item;
          argMax = item;

          min = next;
          max = next;
        }
        else {
          if (comparer.Compare(next, min) < 0) {
            argMin = item;
            min = next;
            minIndex = index;
          }
          else if (comparer.Compare(next, max) > 0) {
            argMax = item;
            max = next;
            maxIndex = index;
          }
        }
      }

      if (index <= 0)
        throw new InvalidOperationException("Empty reader doesn't have extremum items.");

      return (argMin, min, minIndex, argMax, max, maxIndex);
    }

    /// <summary>
    /// Extremal Values
    /// </summary>
    public static async Task<(T ArgMin, R Min, long MinIndex, T ArgMax, R Max, long MaxIndex)> Extremum<T, R>
      (this ChannelReader<T> reader,
            Func<T, R> selector,
            IComparer<R> comparer) => await Extremum(reader, selector, comparer, default).ConfigureAwait(false);

    /// <summary>
    /// Extremal Values
    /// </summary>
    public static async Task<(T ArgMin, R Min, long MinIndex, T ArgMax, R Max, long MaxIndex)> Extremum<T, R>
      (this ChannelReader<T> reader,
             Func<T, R> selector,
             ChannelParallelOptions options)
      where R : IComparable<R> => await Extremum(reader, selector, default, options).ConfigureAwait(false);

    /// <summary>
    /// Extremal Values
    /// </summary>
    public static async Task<(T ArgMin, R Min, long MinIndex, T ArgMax, R Max, long MaxIndex)> Extremum<T, R>
      (this ChannelReader<T> reader,
             Func<T, R> selector)
      where R : IComparable<R> => await Extremum(reader, selector, default, default).ConfigureAwait(false);

    #endregion Public
  }

}
