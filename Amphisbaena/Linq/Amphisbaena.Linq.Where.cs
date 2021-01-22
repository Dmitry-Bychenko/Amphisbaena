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

    /// <summary>
    /// Where
    /// </summary>
    public static ChannelReader<T> Where<T>(this ChannelReader<T> reader,
                                                 Func<T, int, bool> condition,
                                                 ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));
      if (condition is null)
        throw new ArgumentNullException(nameof(condition));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      Channel<T> result = op.CreateChannel<T>();

      Task.Run(async () => {
        int index = -1;

        await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false))
          if (condition(item, ++index))
            await result.Writer.WriteAsync(item).ConfigureAwait(false);

        result.Writer.TryComplete();
      }, op.CancellationToken);

      return result.Reader;

    }

    /// <summary>
    /// Where
    /// </summary>
    public static ChannelReader<T> Where<T>(this ChannelReader<T> reader, Func<T, int, bool> condition) =>
      Where(reader, condition, null);

    /// <summary>
    /// Where
    /// </summary>
    public static ChannelReader<T> Where<T>(this ChannelReader<T> reader,
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

      Channel<T> result = op.CreateChannel<T>();

      Task.Run(async () => {
        await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false))
          if (condition(item))
            await result.Writer.WriteAsync(item).ConfigureAwait(false);

        result.Writer.TryComplete();
      }, op.CancellationToken);

      return result.Reader;
    }

    /// <summary>
    /// Where
    /// </summary>
    public static ChannelReader<T> Where<T>(this ChannelReader<T> reader, Func<T, bool> condition) =>
      Where(reader, condition, null);

    #endregion Public
  }

}
