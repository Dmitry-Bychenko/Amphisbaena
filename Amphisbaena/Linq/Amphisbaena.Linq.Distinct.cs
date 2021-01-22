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
            await result.Writer.WriteAsync(item).ConfigureAwait(false);
        }

        result.Writer.TryComplete();
      }, op.CancellationToken);

      return result.Reader;
    }

    /// <summary>
    /// Distinct
    /// </summary>
    public static ChannelReader<T> Distinct<T>(this ChannelReader<T> reader, IEqualityComparer<T> comparer) =>
      Distinct(reader, comparer, null);

    /// <summary>
    /// Distinct
    /// </summary>
    public static ChannelReader<T> Distinct<T>(this ChannelReader<T> reader, ChannelParallelOptions options) =>
      Distinct(reader, null, options);

    /// <summary>
    /// Distinct
    /// </summary>
    public static ChannelReader<T> Distinct<T>(this ChannelReader<T> reader) =>
      Distinct(reader, null, null);

    #endregion Public
  }

}
