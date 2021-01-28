using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Amphisbaena {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Await task completions
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static partial class ChannelReaderExtensions {
    #region Public

    /// <summary>
    /// When All: turn Task<T> into T
    /// </summary>
    /// <param name="reader">reader to convert</param>
    /// <param name="options">parallel options</param>
    public static ChannelReader<T> WhenAll<T>(this ChannelReader<Task<T>> reader,
                                                   ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      Channel<T> result = op.CreateChannel<T>();

      Task.Run(async () => {
        await foreach (Task<T> task in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false)) {
          await result.Writer.WriteAsync(await task, op.CancellationToken).ConfigureAwait(false);
        }

        result.Writer.TryComplete();
      }, op.CancellationToken);

      return result.Reader;
    }

    /// <summary>
    /// When All: turn Task<T> into T
    /// </summary>
    /// <param name="reader">reader to convert</param>
    public static ChannelReader<T> WhenAll<T>(this ChannelReader<Task<T>> reader) =>
      WhenAll(reader, default);

    /// <summary>
    /// When All turn ValueTask<T> to T
    /// </summary>
    /// <param name="reader">reader to convert</param>
    /// <param name="options">parallel options</param>
    public static ChannelReader<T> WhenAll<T>(this ChannelReader<ValueTask<T>> reader,
                                                   ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      Channel<T> result = op.CreateChannel<T>();

      Task.Run(async () => {
        await foreach (ValueTask<T> task in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false)) {
          await result.Writer.WriteAsync(await task, op.CancellationToken).ConfigureAwait(false);
        }

        result.Writer.TryComplete();
      }, op.CancellationToken);

      return result.Reader;
    }

    /// <summary>
    /// When All turn Task<T> into T
    /// </summary>
    /// <param name="reader">reader to convert</param>
    public static ChannelReader<T> WhenAll<T>(this ChannelReader<ValueTask<T>> reader) =>
      WhenAll(reader, default);

    /// <summary>
    /// To Task (Stub for awaiting)
    /// </summary>
    /// <param name="reader">reader to convert</param>
    /// <param name="options">parallel options</param>
    public static async Task ToTask<T>(this ChannelReader<T> reader,
                                            ChannelParallelOptions options) {
      if (reader is null)
        return;

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      await foreach (T _ in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false))
        ;
    }

    /// <summary>
    /// To Task (Stub for awaiting)
    /// </summary>
    /// <param name="reader">reader to convert</param>
    public static async Task ToTask<T>(this ChannelReader<T> reader) =>
      await ToTask(reader, default).ConfigureAwait(false);

    #endregion Public
  }

}
