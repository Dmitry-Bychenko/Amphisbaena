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
    /// Cast 
    /// </summary>
    /// <param name="reader">initial reader</param>
    /// <param name="options">parallel options</param>
    /// <exception cref="ArgumentNullException">When reader is null</exception>
    /// <exception cref="InvalidCastException">When an item can't be cast</exception>
    public static ChannelReader<T> Cast<T, S>(this ChannelReader<S> reader, ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      Channel<T> result = op.CreateChannel<T>();

      Task.Run(async () => {
        await foreach (S item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false)) {
          try {
            T rec = (T)(Convert.ChangeType(item, typeof(T)));

            await result.Writer.WriteAsync(rec, op.CancellationToken).ConfigureAwait(false);
          }
          catch (Exception e) {
            result.Writer.TryComplete();

            throw new InvalidCastException("Cast failed to cast an item", e);
          }
        }

        result.Writer.TryComplete();
      }, op.CancellationToken);

      return result.Reader;
    }

    /// <summary>
    /// Cast 
    /// </summary>
    /// <param name="reader">initial reader</param>
    /// <exception cref="ArgumentNullException">When reader is null</exception>
    /// <exception cref="InvalidCastException">When an item can't be cast</exception>
    public static ChannelReader<T> Cast<T, S>(this ChannelReader<S> reader) =>
      Cast<T, S>(reader, default);

    /// <summary>
    /// Of Type 
    /// </summary>
    /// <param name="reader">initial reader</param>
    /// <param name="options">parallel options</param>
    /// <exception cref="ArgumentNullException">When reader is null</exception>
    public static ChannelReader<T> OfType<T, S>(this ChannelReader<S> reader, ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      Channel<T> result = op.CreateChannel<T>();

      Task.Run(async () => {
        await foreach (S item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false)) {
          try {
            T rec = (T)(Convert.ChangeType(item, typeof(T)));

            await result.Writer.WriteAsync(rec, op.CancellationToken).ConfigureAwait(false);
          }
          catch (InvalidCastException) {
            ;
          }
        }

        result.Writer.TryComplete();
      }, op.CancellationToken);

      return result.Reader;
    }

    /// <summary>
    /// OfType 
    /// </summary>
    /// <param name="reader">initial reader</param>
    /// <exception cref="ArgumentNullException">When reader is null</exception>
    public static ChannelReader<T> OfType<T, S>(this ChannelReader<S> reader) =>
      OfType<T, S>(reader, default);

    #endregion Public
  }

}
