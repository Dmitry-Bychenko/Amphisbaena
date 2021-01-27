using System;
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

    #region Count

    /// <summary>
    /// Count 
    /// </summary>
    public static async Task<long> Count<T>(this ChannelReader<T> reader,
                                                 Func<T, bool> condition,
                                                 ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      condition ??= x => true;

      long result = 0;

      await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false))
        if (condition(item))
          result += 1;

      return result;
    }

    /// <summary>
    /// Count 
    /// </summary>
    public static async Task<long> Count<T>(this ChannelReader<T> reader, Func<T, bool> condition) =>
      await Count(reader, condition, default).ConfigureAwait(false);

    /// <summary>
    /// Count 
    /// </summary>
    public static async Task<long> Count<T>(this ChannelReader<T> reader, ChannelParallelOptions options) =>
      await Count(reader, default, options).ConfigureAwait(false);

    /// <summary>
    /// Count 
    /// </summary>
    public static async Task<long> Count<T>(this ChannelReader<T> reader) =>
      await Count(reader, default, default).ConfigureAwait(false);

    #endregion Count

    #region Count Fraction

    /// <summary>
    /// Count Fraction 
    /// </summary>
    public static async Task<double> CountFraction<T>(this ChannelReader<T> reader,
                                                           Func<T, bool> condition,
                                                           ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));
      if (condition is null)
        throw new ArgumentNullException(nameof(condition));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      long right = 0;
      long all = 0;

      await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false)) {
        all += 1;

        if (condition(item))
          right += 1;
      }

      return ((double)right) / all;
    }

    /// <summary>
    /// Count Fraction 
    /// </summary>
    public static async Task<double> CountFraction<T>(this ChannelReader<T> reader,
                                                           Func<T, bool> condition) =>
      await CountFraction(reader, condition, default).ConfigureAwait(false);

    #endregion Count Fraction

    #endregion Public
  }

}
