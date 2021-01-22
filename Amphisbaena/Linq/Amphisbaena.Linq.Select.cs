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
    /// Select 
    /// </summary>
    public static ChannelReader<T> Select<T, S>(this ChannelReader<S> reader, 
                                                     Func<S, int, T> map,
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
        int index = -1;

        await foreach (S item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false))
          await result.Writer.WriteAsync(map(item, ++index)).ConfigureAwait(false);

        result.Writer.TryComplete();
      }, op.CancellationToken);

      return result.Reader;
    }

    /// <summary>
    /// Select 
    /// </summary>
    public static ChannelReader<T> Select<T, S>(this ChannelReader<S> reader, Func<S, int, T> map) =>
      Select(reader, map, null);

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
          await result.Writer.WriteAsync(map(item)).ConfigureAwait(false);

        result.Writer.TryComplete();
      }, op.CancellationToken);

      return result.Reader;
    }

    /// <summary>
    /// Select 
    /// </summary>
    public static ChannelReader<T> Select<T, S>(this ChannelReader<S> reader, Func<S, T> map) =>
      Select(reader, map, null);

    #endregion Public
  }

}
