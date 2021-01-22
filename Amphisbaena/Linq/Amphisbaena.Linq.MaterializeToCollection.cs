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
    /// To List (async)
    /// </summary>
    public static async Task<List<T>> ToListAsync<T>(this ChannelReader<T> source, ChannelParallelOptions options) {
      if (source is null)
        throw new ArgumentNullException(nameof(source));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      List<T> result = new List<T>();

      await foreach (T item in source.ReadAllAsync(op.CancellationToken).ConfigureAwait(false))
        result.Add(item);

      return result;
    }

    /// <summary>
    /// To List (async)
    /// </summary>
    public static async Task<List<T>> ToListAsync<T>(this ChannelReader<T> source) =>
      await ToListAsync(source, null);

    /// <summary>
    /// To Array (async)
    /// </summary>
    public static async Task<T[]> ToArrayAsync<T>(this ChannelReader<T> source, ChannelParallelOptions options) {
      if (source is null)
        throw new ArgumentNullException(nameof(source));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      List<T> result = new List<T>();

      await foreach (T item in source.ReadAllAsync(op.CancellationToken).ConfigureAwait(false))
        result.Add(item);

      return result.ToArray();
    }

    /// <summary>
    /// To Array (async)
    /// </summary>
    public static async Task<T[]> ToArrayAsync<T>(this ChannelReader<T> source) =>
      await ToArrayAsync(source, null);

    /// <summary>
    /// To Dictionary
    /// </summary>
    public static async Task<Dictionary<K, V>> ToDictionaryAsync<T, K, V>(this ChannelReader<T> source,
                                                                               Func<T, K> key,
                                                                               Func<T, V> value,
                                                                               IEqualityComparer<K> comparer,
                                                                               ChannelParallelOptions options) {
      if (source is null)
        throw new ArgumentNullException(nameof(source));
      if (key is null)
        throw new ArgumentNullException(nameof(key));
      if (value is null)
        throw new ArgumentNullException(nameof(value));

      if (comparer is null)
        comparer = EqualityComparer<K>.Default;

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      Dictionary<K, V> result = new Dictionary<K, V>(comparer);

      await foreach (T item in source.ReadAllAsync(op.CancellationToken).ConfigureAwait(false))
        result.Add(key(item), value(item));

      return result;
    }
    /// <summary>
    /// To Dictionary
    /// </summary>
    public static async Task<Dictionary<K, V>> ToDictionaryAsync<T, K, V>(this ChannelReader<T> source,
                                                                               Func<T, K> key,
                                                                               Func<T, V> value,
                                                                               IEqualityComparer<K> comparer) =>
      await ToDictionaryAsync(source, key, value, comparer, null);

    /// <summary>
    /// To Dictionary
    /// </summary>
    public static async Task<Dictionary<K, V>> ToDictionaryAsync<T, K, V>(this ChannelReader<T> source,
                                                                               Func<T, K> key,
                                                                               Func<T, V> value) =>
      await ToDictionaryAsync(source, key, value, null, null);

    /// <summary>
    /// To Dictionary
    /// </summary>
    public static async Task<Dictionary<K, T>> ToDictionaryAsync<T, K>(this ChannelReader<T> source,
                                                                            Func<T, K> key,
                                                                            IEqualityComparer<K> comparer,
                                                                            ChannelParallelOptions options) =>
      await ToDictionaryAsync(source, key, item => item, comparer, options);

    /// <summary>
    /// To Dictionary
    /// </summary>
    public static async Task<Dictionary<K, T>> ToDictionaryAsync<T, K>(this ChannelReader<T> source,
                                                                            Func<T, K> key,
                                                                            IEqualityComparer<K> comparer) =>
      await ToDictionaryAsync(source, key, item => item, comparer, null);

    /// <summary>
    /// To Dictionary
    /// </summary>
    public static async Task<Dictionary<K, T>> ToDictionaryAsync<T, K>(this ChannelReader<T> source,
                                                                            Func<T, K> key) =>
      await ToDictionaryAsync(source, key, item => item, null, null);

    #endregion Public
  }

}
