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

    #region First Or Default

    /// <summary>
    /// First Or Default
    /// </summary>
    public static async Task<T> FirstOrDefault<T>(this ChannelReader<T> reader,
                                                       Func<T, bool> condition,
                                                       T defaultValue,
                                                       ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));

      condition ??= x => true;

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false))
        if (condition(item))
          return item;

      return defaultValue;
    }

    /// <summary>
    /// First Or default
    /// </summary>
    public static async Task<T> FirstOrDefault<T>(this ChannelReader<T> reader,
                                                       Func<T, bool> condition,
                                                       T defaultValue) =>
      await FirstOrDefault(reader, condition, defaultValue, default);

    /// <summary>
    /// First Or Default
    /// </summary>
    public static async Task<T> FirstOrDefault<T>(this ChannelReader<T> reader,
                                                       Func<T, bool> condition,
                                                       ChannelParallelOptions options) =>
      await FirstOrDefault(reader, condition, default, options);

    /// <summary>
    /// First Or Default
    /// </summary>
    public static async Task<T> FirstOrDefault<T>(this ChannelReader<T> reader,
                                                       Func<T, bool> condition) =>
      await FirstOrDefault(reader, condition, default, default);

    /// <summary>
    /// First Or Default
    /// </summary>
    public static async Task<T> FirstOrDefault<T>(this ChannelReader<T> reader,
                                                       T defaultValue,
                                                       ChannelParallelOptions options) =>
      await FirstOrDefault(reader, default, defaultValue, options);

    /// <summary>
    /// First Or Default
    /// </summary>
    public static async Task<T> FirstOrDefault<T>(this ChannelReader<T> reader, T defaultValue) =>
      await FirstOrDefault(reader, default, defaultValue, default);

    /// <summary>
    /// First Or Default
    /// </summary>
    public static async Task<T> FirstOrDefault<T>(this ChannelReader<T> reader, ChannelParallelOptions options) =>
      await FirstOrDefault(reader, default, default, options);

    /// <summary>
    /// First Or Default
    /// </summary>
    public static async Task<T> FirstOrDefault<T>(this ChannelReader<T> reader) =>
      await FirstOrDefault(reader, default, default, default);

    #endregion First Or Default

    #region First

    /// <summary>
    /// First Or Default
    /// </summary>
    public static async Task<T> First<T>(this ChannelReader<T> reader,
                                              Func<T, bool> condition,
                                              ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));

      condition ??= x => true;

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false))
        if (condition(item))
          return item;

      throw new InvalidOperationException("Channel Reader is Empty.");
    }

    /// <summary>
    /// First
    /// </summary>
    public static async Task<T> First<T>(this ChannelReader<T> reader,
                                                       Func<T, bool> condition) =>
      await First(reader, condition, default);

    /// <summary>
    /// First
    /// </summary>
    public static async Task<T> First<T>(this ChannelReader<T> reader, ChannelParallelOptions options) =>
      await First(reader, default, options);

    /// <summary>
    /// First
    /// </summary>
    public static async Task<T> First<T>(this ChannelReader<T> reader) =>
      await First(reader, default, default);

    #endregion First

    #region Last Or Default

    /// <summary>
    /// Last Or Default
    /// </summary>
    public static async Task<T> LastOrDefault<T>(this ChannelReader<T> reader,
                                                      Func<T, bool> condition,
                                                      T defaultValue,
                                                      ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));

      condition ??= x => true;

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      T result = defaultValue;

      await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false))
        if (condition(item))
          result = item;

      return result;
    }

    /// <summary>
    /// Last Or Default
    /// </summary>
    public static async Task<T> LastOrDefault<T>(this ChannelReader<T> reader,
                                                      Func<T, bool> condition,
                                                      T defaultValue) =>
      await LastOrDefault(reader, condition, defaultValue, default);

    /// <summary>
    /// Last Or Default
    /// </summary>
    public static async Task<T> LastOrDefault<T>(this ChannelReader<T> reader,
                                                      Func<T, bool> condition,
                                                      ChannelParallelOptions options) =>
      await LastOrDefault(reader, condition, default, options);

    /// <summary>
    /// Last Or Default
    /// </summary>
    public static async Task<T> LastOrDefault<T>(this ChannelReader<T> reader,
                                                      Func<T, bool> condition) =>
      await LastOrDefault(reader, condition, default, default);

    /// <summary>
    /// Last Or Default
    /// </summary>
    public static async Task<T> LastOrDefault<T>(this ChannelReader<T> reader,
                                                      T defaultValue,
                                                      ChannelParallelOptions options) =>
      await LastOrDefault(reader, default, defaultValue, options);

    /// <summary>
    /// Last Or Default
    /// </summary>
    public static async Task<T> LastOrDefault<T>(this ChannelReader<T> reader, T defaultValue) =>
      await LastOrDefault(reader, default, defaultValue, default);

    /// <summary>
    /// Last Or Default
    /// </summary>
    public static async Task<T> LastOrDefault<T>(this ChannelReader<T> reader, ChannelParallelOptions options) =>
      await LastOrDefault(reader, default, default, options);

    /// <summary>
    /// Last Or Default
    /// </summary>
    public static async Task<T> LastOrDefault<T>(this ChannelReader<T> reader) =>
      await LastOrDefault(reader, default, default, default);

    #endregion Last Or Default

    #region Last

    /// <summary>
    /// Last
    /// </summary>
    public static async Task<T> Last<T>(this ChannelReader<T> reader,
                                             Func<T, bool> condition,
                                             ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));

      condition ??= x => true;

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      T result = default;
      bool empty = true;

      await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false))
        if (condition(item)) {
          empty = false;
          result = item;
        }

      return !empty
        ? result
        : throw new InvalidOperationException("Channel Reader is Empty.");
    }

    /// <summary>
    /// Last
    /// </summary>
    public static async Task<T> Last<T>(this ChannelReader<T> reader,
                                             Func<T, bool> condition) =>
      await Last(reader, condition, default);

    /// <summary>
    /// Last
    /// </summary>
    public static async Task<T> Last<T>(this ChannelReader<T> reader, ChannelParallelOptions options) =>
      await Last(reader, default, options);

    /// <summary>
    /// Last
    /// </summary>
    public static async Task<T> Last<T>(this ChannelReader<T> reader) =>
      await Last(reader, default, default);

    #endregion Last

    #region Single Or Default

    /// <summary>
    /// Single Or Default
    /// </summary>
    public static async Task<T> SingleOrDefault<T>(this ChannelReader<T> reader,
                                                        Func<T, bool> condition,
                                                        T defaultValue,
                                                        ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));

      condition ??= x => true;

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      bool first = true;

      T result = defaultValue;

      await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false)) {
        if (!condition(item))
          continue;

        if (first) {
          result = item;
          first = false;
        }
        else
          throw new InvalidOperationException("Channel Reader has more than single item.");
      }

      return result;
    }

    /// <summary>
    /// Single Or Default
    /// </summary>
    public static async Task<T> SingleOrDefault<T>(this ChannelReader<T> reader,
                                                        Func<T, bool> condition,
                                                        T defaultValue) =>
      await SingleOrDefault(reader, condition, defaultValue, default);

    /// <summary>
    /// Single Or Default
    /// </summary>
    public static async Task<T> SingleOrDefault<T>(this ChannelReader<T> reader,
                                                        Func<T, bool> condition,
                                                        ChannelParallelOptions options) =>
      await SingleOrDefault(reader, condition, default, options);

    /// <summary>
    /// Single Or Default
    /// </summary>
    public static async Task<T> SingleOrDefault<T>(this ChannelReader<T> reader,
                                                        Func<T, bool> condition) =>
      await SingleOrDefault(reader, condition, default, default);

    /// <summary>
    /// Single Or Default
    /// </summary>
    public static async Task<T> SingleOrDefault<T>(this ChannelReader<T> reader,
                                                        T defaultValue,
                                                        ChannelParallelOptions options) =>
      await SingleOrDefault(reader, default, defaultValue, options);

    /// <summary>
    /// Single Or Default
    /// </summary>
    public static async Task<T> SingleOrDefault<T>(this ChannelReader<T> reader, T defaultValue) =>
      await SingleOrDefault(reader, default, defaultValue, default);

    /// <summary>
    /// Single Or Default
    /// </summary>
    public static async Task<T> SingleOrDefault<T>(this ChannelReader<T> reader, ChannelParallelOptions options) =>
      await SingleOrDefault(reader, default, default, options);

    /// <summary>
    /// Single Or Default
    /// </summary>
    public static async Task<T> SingleOrDefault<T>(this ChannelReader<T> reader) =>
      await SingleOrDefault(reader, default, default, default);

    #endregion Single Or Default

    #region Single

    /// <summary>
    /// Single
    /// </summary>
    public static async Task<T> Single<T>(this ChannelReader<T> reader,
                                               Func<T, bool> condition,
                                               ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));

      condition ??= x => true;

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      bool first = true;

      T result = default;

      await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false)) {
        if (!condition(item))
          continue;

        if (first) {
          result = item;
          first = false;
        }
        else
          throw new InvalidOperationException("Channel Reader has more than single item.");
      }

      return !first
        ? result
        : throw new InvalidOperationException("Channel Reader is Empty.");
    }

    /// <summary>
    /// Single
    /// </summary>
    public static async Task<T> Single<T>(this ChannelReader<T> reader,
                                               Func<T, bool> condition) =>
      await Single(reader, condition, default);

    /// <summary>
    /// Single
    /// </summary>
    public static async Task<T> Single<T>(this ChannelReader<T> reader, ChannelParallelOptions options) =>
      await Single(reader, default, options);

    /// <summary>
    /// Single
    /// </summary>
    public static async Task<T> Single<T>(this ChannelReader<T> reader) =>
      await Single(reader, default, default);

    #endregion Single

    #region Element At Or Default

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

    #endregion Element At Or Default

    #region Element At

    /// <summary>
    /// Element At 
    /// </summary>
    public static async Task<T> ElementAt<T>(this ChannelReader<T> reader,
                                                  long index,
                                                  ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));

      if (index < 0)
        throw new ArgumentOutOfRangeException(nameof(index));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false))
        if (index == 0)
          return item;
        else
          index -= 1;

      throw new InvalidOperationException("Channel Reader is too short.");
    }

    /// <summary>
    /// Element At
    /// </summary>
    public static async Task<T> ElementAt<T>(this ChannelReader<T> reader,
                                                  long index) =>
      await ElementAt(reader, index, default);

    #endregion Element At

    #endregion Public
  }

}
