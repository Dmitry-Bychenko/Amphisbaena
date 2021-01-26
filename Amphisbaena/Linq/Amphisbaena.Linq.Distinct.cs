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
    /// Distinct
    /// </summary>
    /// <param name="reader">reader to get values from</param>
    /// <param name="comparer">comparer to compare two values to equality</param>
    /// <param name="options">parallel options</param>
    /// <exception cref="ArgumentNullException">when reader is null</exception>
    public static ChannelReader<T> Distinct<T>(this ChannelReader<T> reader,
                                                    IEqualityComparer<T> comparer,
                                                    ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));

      comparer ??= EqualityComparer<T>.Default;

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      Channel<T> result = op.CreateChannel<T>();

      Task.Run(async () => {
        HashSet<T> unique = new HashSet<T>(comparer);

        await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false)) {
          if (unique.Add(item))
            await result.Writer.WriteAsync(item, op.CancellationToken).ConfigureAwait(false);
        }

        result.Writer.TryComplete();
      }, op.CancellationToken);

      return result.Reader;
    }

    /// <summary>
    /// Distinct
    /// </summary>
    /// <param name="reader">reader to get values from</param>
    /// <param name="options">parallel options</param>
    /// <exception cref="ArgumentNullException">when reader is null</exception>
    public static ChannelReader<T> Distinct<T>(this ChannelReader<T> reader, IEqualityComparer<T> comparer) =>
      Distinct(reader, comparer, default);

    /// <summary>
    /// Distinct
    /// </summary>
    /// <param name="reader">reader to get values from</param>
    /// <param name="comparer">comparer to compare two values to equality</param>
    /// <exception cref="ArgumentNullException">when reader is null</exception>
    public static ChannelReader<T> Distinct<T>(this ChannelReader<T> reader, ChannelParallelOptions options) =>
      Distinct(reader, default, options);

    /// <summary>
    /// Distinct
    /// </summary>
    /// <param name="reader">reader to get values from</param>
    /// <exception cref="ArgumentNullException">when reader is null</exception> 
    public static ChannelReader<T> Distinct<T>(this ChannelReader<T> reader) =>
      Distinct(reader, default, default);

    #endregion Public
  }

}
