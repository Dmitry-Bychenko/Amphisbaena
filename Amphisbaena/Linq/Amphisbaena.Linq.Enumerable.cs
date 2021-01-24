using System;
using System.Collections.Generic;
using System.Threading.Channels;

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
    /// To AsyncEnumerable 
    /// </summary>
    public static IAsyncEnumerable<T> ToAsyncEnumerable<T>(this ChannelReader<T> reader,
                                                                ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      return reader.ReadAllAsync(op.CancellationToken);
    }

    /// <summary>
    /// To AsyncEnumerable 
    /// </summary>
    public static IAsyncEnumerable<T> ToAsyncEnumerable<T>(this ChannelReader<T> reader) =>
      ToAsyncEnumerable(reader, default);

    #endregion Public
  }
}
