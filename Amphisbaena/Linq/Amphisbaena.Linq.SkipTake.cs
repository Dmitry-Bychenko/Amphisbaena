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
    /// Skip
    /// </summary>
    public static ChannelReader<T> Skip<T>(this ChannelReader<T> reader,
                                                int count,
                                                ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      Channel<T> result = op.CreateChannel<T>();

      Task.Run(async () => {
        await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false)) {
          if (count == 0)
            await result.Writer.WriteAsync(item).ConfigureAwait(false);
          else
            count -= 1;
        }

        result.Writer.TryComplete();

      }, op.CancellationToken);

      return result;
    }

    /// <summary>
    /// Skip
    /// </summary>
    public static ChannelReader<T> Skip<T>(this ChannelReader<T> reader, int count) =>
      Skip(reader, count, null);

    /// <summary>
    /// SkipWhile
    /// </summary>
    public static ChannelReader<T> SkipWhile<T>(this ChannelReader<T> reader,
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
        bool skipped = false;

        await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false)) {
          if (skipped || !condition(item)) {
            skipped = true;

            await result.Writer.WriteAsync(item).ConfigureAwait(false);
          }
        }

        result.Writer.TryComplete();

      }, op.CancellationToken);

      return result;
    }

    /// <summary>
    /// SkipWhile
    /// </summary>
    public static ChannelReader<T> SkipWhile<T>(this ChannelReader<T> reader, Func<T, bool> condition) =>
      SkipWhile(reader, condition, null);

    /// <summary>
    /// Take
    /// </summary>
    public static ChannelReader<T> Take<T>(this ChannelReader<T> reader,
                                                int count,
                                                ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      Channel<T> result = op.CreateChannel<T>();

      Task.Run(async () => {
        await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false)) {
          if (count > 0) {
            count -= 1;

            await result.Writer.WriteAsync(item).ConfigureAwait(false);
          }
          else
            break;
        }

        result.Writer.TryComplete();
      }, op.CancellationToken);

      return result;
    }

    /// <summary>
    /// Take
    /// </summary>
    public static ChannelReader<T> Take<T>(this ChannelReader<T> reader, int count) =>
      Take(reader, count, null);

    /// <summary>
    /// TakeWhile
    /// </summary>
    public static ChannelReader<T> TakeWhile<T>(this ChannelReader<T> reader,
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
        await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false)) {
          if (condition(item))
            await result.Writer.WriteAsync(item).ConfigureAwait(false);
          else
            break;
        }

        result.Writer.TryComplete();

      }, op.CancellationToken);

      return result;
    }

    /// <summary>
    /// TakeWhile
    /// </summary>
    public static ChannelReader<T> TakeWhile<T>(this ChannelReader<T> reader, Func<T, bool> condition) =>
      TakeWhile(reader, condition, null);

    #endregion Public
  }

}
