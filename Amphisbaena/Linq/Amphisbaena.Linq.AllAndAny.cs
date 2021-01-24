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

    #region All

    /// <summary>
    /// All
    /// </summary>
    public static async Task<bool> All<T>(this ChannelReader<T> reader,
                                               Func<T, bool> condition,
                                               ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      condition ??= x => true;

      await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false))
        if (!condition(item))
          return false;

      return true;
    }

    /// <summary>
    /// All
    /// </summary>
    public static async Task<bool> All<T>(this ChannelReader<T> reader, Func<T, bool> condition) =>
      await All(reader, condition, default);

    /// <summary>
    /// All
    /// </summary>
    public static async Task<bool> All<T>(this ChannelReader<T> reader, ChannelParallelOptions options) =>
      await All(reader, default, options);

    /// <summary>
    /// All
    /// </summary>
    public static async Task<bool> All<T>(this ChannelReader<T> reader) =>
      await All(reader, default, default);

    #endregion All

    #region Any

    /// <summary>
    /// Any
    /// </summary>
    public static async Task<bool> Any<T>(this ChannelReader<T> reader,
                                               Func<T, bool> condition,
                                               ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      condition ??= x => true;

      await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false))
        if (condition(item))
          return true;

      return false;
    }

    /// <summary>
    /// Any
    /// </summary>
    public static async Task<bool> Any<T>(this ChannelReader<T> reader, Func<T, bool> condition) =>
      await Any(reader, condition, default);

    /// <summary>
    /// Any
    /// </summary>
    public static async Task<bool> Any<T>(this ChannelReader<T> reader, ChannelParallelOptions options) =>
      await Any(reader, default, options);

    /// <summary>
    /// Any
    /// </summary>
    public static async Task<bool> Any<T>(this ChannelReader<T> reader) =>
      await Any(reader, default, default);

    #endregion Any

    #region Default If Empty

    /// <summary>
    /// Default If Empty
    /// </summary>
    public static ChannelReader<T> DefaultIfEmpty<T>(this ChannelReader<T> reader,
                                                          T defaultValue,
                                                          ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      Channel<T> result = op.CreateChannel<T>();

      Task.Run(async () => {
        bool any = false;

        await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false)) {
          any = true;

          await result.Writer.WriteAsync(item, op.CancellationToken).ConfigureAwait(false);
        }

        if (!any)
          await result.Writer.WriteAsync(defaultValue, op.CancellationToken).ConfigureAwait(false);

        result.Writer.TryComplete();
      });

      return result.Reader;
    }

    /// <summary>
    /// Default If Empty
    /// </summary>
    public static ChannelReader<T> DefaultIfEmpty<T>(this ChannelReader<T> reader, T defaultValue) =>
      DefaultIfEmpty(reader, defaultValue, default);

    /// <summary>
    /// Default If Empty
    /// </summary>
    public static ChannelReader<T> DefaultIfEmpty<T>(this ChannelReader<T> reader, ChannelParallelOptions options) =>
      DefaultIfEmpty(reader, default, options);

    /// <summary>
    /// Default If Empty
    /// </summary>
    public static ChannelReader<T> DefaultIfEmpty<T>(this ChannelReader<T> reader) =>
      DefaultIfEmpty(reader, default, default);

    #endregion Default If Empty

    #endregion Public
  }

}
