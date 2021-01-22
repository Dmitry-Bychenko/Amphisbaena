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
    /// First Or Default
    /// </summary>
    public static async Task<T> FirstOrDefault<T>(this ChannelReader<T> reader, 
                                                       T defaultValue, 
                                                       ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false))
        return item;

      return defaultValue;
    }

    /// <summary>
    /// First Or Default
    /// </summary>
    public static async Task<T> FirstOrDefault<T>(this ChannelReader<T> reader, T defaultValue) =>
      await FirstOrDefault(reader, defaultValue, default);

    /// <summary>
    /// First Or Default
    /// </summary>
    public static async Task<T> FirstOrDefault<T>(this ChannelReader<T> reader, ChannelParallelOptions options) =>
      await FirstOrDefault(reader, default, options);

    /// <summary>
    /// First Or Default
    /// </summary>
    public static async Task<T> FirstOrDefault<T>(this ChannelReader<T> reader) =>
      await FirstOrDefault(reader, default, default);

    /// <summary>
    /// Last Or Default
    /// </summary>
    public static async Task<T> LastOrDefault<T>(this ChannelReader<T> reader,
                                                      T defaultValue,
                                                      ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      T result = defaultValue;

      await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false))
        result = item;

      return result;
    }

    /// <summary>
    /// Last Or Default
    /// </summary>
    public static async Task<T> LastOrDefault<T>(this ChannelReader<T> reader, T defaultValue) =>
      await LastOrDefault(reader, defaultValue, default);

    /// <summary>
    /// Last Or Default
    /// </summary>
    public static async Task<T> LastOrDefault<T>(this ChannelReader<T> reader, ChannelParallelOptions options) =>
      await LastOrDefault(reader, default, options);

    /// <summary>
    /// Last Or Default
    /// </summary>
    public static async Task<T> LastOrDefault<T>(this ChannelReader<T> reader) =>
      await LastOrDefault(reader, default, default);

    /// <summary>
    /// Single Or Default
    /// </summary>
    public static async Task<T> SingleOrDefault<T>(this ChannelReader<T> reader,
                                                        T defaultValue,
                                                        ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      bool first = true;

      T result = defaultValue;

      await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false))
        if (first) {
          result = item;
          first = false;
        }
        else
          return defaultValue;

      return result;
    }

    /// <summary>
    /// Single Or Default
    /// </summary>
    public static async Task<T> SingleOrDefault<T>(this ChannelReader<T> reader, T defaultValue) =>
      await SingleOrDefault(reader, defaultValue, default);

    /// <summary>
    /// Single Or Default
    /// </summary>
    public static async Task<T> SingleOrDefault<T>(this ChannelReader<T> reader, ChannelParallelOptions options) =>
      await SingleOrDefault(reader, default, options);

    /// <summary>
    /// Single Or Default
    /// </summary>
    public static async Task<T> SingleOrDefault<T>(this ChannelReader<T> reader) =>
      await SingleOrDefault(reader, default, default);

    /// <summary>
    /// Element At Or Default
    /// </summary>
    public static async Task<T> ElementAtOrDefault<T>(this ChannelReader<T> reader,
                                                           long index,
                                                           T defaultValue,
                                                           ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      if (index < 0)
        return defaultValue;

      await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false))
        if (index == 0)
          return item;
        else
          index -= 1;

      return defaultValue;
    }

    /// <summary>
    /// Element At Or Default
    /// </summary>
    public static async Task<T> ElementAtOrDefault<T>(this ChannelReader<T> reader,
                                                           long index,
                                                           T defaultValue) =>
      await ElementAtOrDefault(reader, index, defaultValue, default);

    /// <summary>
    /// Element At Or Default
    /// </summary>
    public static async Task<T> ElementAtOrDefault<T>(this ChannelReader<T> reader,
                                                           long index,
                                                           ChannelParallelOptions options) =>
      await ElementAtOrDefault(reader, index, default, options);

    /// <summary>
    /// Element At Or Default
    /// </summary>
    public static async Task<T> ElementAtOrDefault<T>(this ChannelReader<T> reader, long index) =>
      await ElementAtOrDefault(reader, index, default, default);

    /// <summary>
    /// Any
    /// </summary>
    public static async Task<bool> Any<T>(this ChannelReader<T> reader, ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

#pragma warning disable IDE0059 // Unnecessary assignment of a value
      await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false))
#pragma warning restore IDE0059 // Unnecessary assignment of a value
        return true;

      return false;
    }

    /// <summary>
    /// Any
    /// </summary>
    public static async Task<bool> Any<T>(this ChannelReader<T> reader) =>
      await Any(reader, default);

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

    #endregion Public
  }

}
