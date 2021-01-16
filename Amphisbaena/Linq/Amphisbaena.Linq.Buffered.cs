using System;
using System.Linq;

using System.Threading;
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
    /// Buffered Reader. When buffer is full, the reader waits 
    /// </summary>
    /// <param name="reader">Reader</param>
    /// <param name="bufferSize">Buffer Size</param>
    /// <param name="token">Cancellation Token</param>
    public static ChannelReader<T> Buffered<T>(this ChannelReader<T> reader,
                                                    int bufferSize,
                                                    CancellationToken token) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));
      if (bufferSize <= 0)
        throw new ArgumentOutOfRangeException(nameof(bufferSize));

      token.ThrowIfCancellationRequested();

      BoundedChannelOptions options = new BoundedChannelOptions(bufferSize) {
        FullMode = BoundedChannelFullMode.Wait
      };

      Channel<T> result = Channel.CreateBounded<T>(options);

      Task.Run(async () => {
        await foreach (T item in reader.ReadAllAsync(token).WithCancellation(token).ConfigureAwait(false)) {
          await result.Writer.WriteAsync(item, token).ConfigureAwait(false);
        }

        result.Writer.Complete();

      }, token);

      return result;
    }

    /// <summary>
    /// Buffered Reader. When buffer is full, the reader waits 
    /// </summary>
    /// <param name="reader">Reader</param>
    /// <param name="bufferSize">Buffer Size</param>
    public static ChannelReader<T> Buffered<T>(this ChannelReader<T> reader, int bufferSize) =>
      Buffered(reader, bufferSize, default);

    #endregion Public
  }

}
