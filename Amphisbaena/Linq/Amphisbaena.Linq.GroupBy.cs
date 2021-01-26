using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Amphisbaena.Linq {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Channel Group
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public sealed class ChannelGroup<K, V> {
    #region Algorithm

    internal Channel<V> Channel { get; }

    #endregion Algorithm

    #region Create

    internal ChannelGroup(K key, ChannelParallelOptions options) {
      Key = key;

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      Channel = op.CreateChannel<V>();
    }

    #endregion Create

    #region Public

    /// <summary>
    /// Key
    /// </summary>
    public K Key { get; }

    /// <summary>
    /// Reader for Values
    /// </summary>
    public ChannelReader<V> Reader => Channel.Reader;

    #endregion Public

    #region Operators

    /// <summary>
    /// Reader from group
    /// </summary>
    public static implicit operator ChannelReader<V>(ChannelGroup<K, V> value) =>
      value?.Reader;

    #endregion Operators
  }

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
    /// Group By 
    /// </summary>
    /// <param name="reader">reader which items to group</param>
    /// <param name="keySelector">Key selector</param>
    /// <param name="keyComparer">Key Comparer</param>
    /// <param name="valueSelector">Value Selector</param>
    /// <param name="options">Parallel Options</param>
    /// <exception cref="ArgumentNullException">When reader, keySelector or ValueSelector is null</exception>
    public static ChannelReader<ChannelGroup<K, V>> GroupBy<K, V, S>(this ChannelReader<S> reader,
                                                                          Func<S, K> keySelector,
                                                                          Func<S, V> valueSelector,
                                                                          IEqualityComparer<K> keyComparer,
                                                                          ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));
      if (keySelector is null)
        throw new ArgumentNullException(nameof(keySelector));
      if (valueSelector is null)
        throw new ArgumentNullException(nameof(valueSelector));

      keyComparer ??= EqualityComparer<K>.Default;

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      Channel<ChannelGroup<K, V>> result = op.CreateChannel<ChannelGroup<K, V>>();

      Task.Run(async () => {
        Dictionary<K, ChannelGroup<K, V>> dict = new Dictionary<K, ChannelGroup<K, V>>(keyComparer);

        await foreach (S item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false)) {
          K key = keySelector(item);
          V value = valueSelector(item);

          if (dict.TryGetValue(key, out var channel))
            await channel.Channel.Writer.WriteAsync(value, op.CancellationToken).ConfigureAwait(false);
          else {
            ChannelGroup<K, V> group = new ChannelGroup<K, V>(key, op);

            dict.Add(key, group);

            await result.Writer.WriteAsync(group, op.CancellationToken).ConfigureAwait(false);

            await group.Channel.Writer.WriteAsync(value, op.CancellationToken).ConfigureAwait(false);
          }
        }

        foreach (var g in dict.Values)
          g.Channel.Writer.TryComplete();

        result.Writer.TryComplete();
      }, op.CancellationToken);

      return result.Reader;
    }

    /// <summary>
    /// Group By 
    /// </summary>
    /// <param name="reader">reader which items to group</param>
    /// <param name="keySelector">Key selector</param>
    /// <param name="keyComparer">Key Comparer</param>
    /// <param name="valueSelector">Value Selector</param>
    /// <exception cref="ArgumentNullException">When reader, keySelector or ValueSelector is null</exception>
    public static ChannelReader<ChannelGroup<K, V>> GroupBy<K, V, S>(this ChannelReader<S> reader,
                                                                           Func<S, K> keySelector,
                                                                           Func<S, V> valueSelector,
                                                                           IEqualityComparer<K> keyComparer) =>
      GroupBy(reader, keySelector, valueSelector, keyComparer, default);

    /// <summary>
    /// Group By 
    /// </summary>
    /// <param name="reader">reader which items to group</param>
    /// <param name="keySelector">Key selector</param>
    /// <param name="valueSelector">Value Selector</param>
    /// <exception cref="ArgumentNullException">When reader, keySelector or ValueSelector is null</exception> 
    public static ChannelReader<ChannelGroup<K, V>> GroupBy<K, V, S>(this ChannelReader<S> reader,
                                                                           Func<S, K> keySelector,
                                                                           Func<S, V> valueSelector,
                                                                           ChannelParallelOptions options) =>
      GroupBy(reader, keySelector, valueSelector, default, options);

    /// <summary>
    /// Group By 
    /// </summary>
    /// <param name="reader">reader which items to group</param>
    /// <param name="keySelector">Key selector</param>
    /// <param name="valueSelector">Value Selector</param>
    /// <exception cref="ArgumentNullException">When reader, keySelector or ValueSelector is null</exception>
    public static ChannelReader<ChannelGroup<K, V>> GroupBy<K, V, S>(this ChannelReader<S> reader,
                                                                          Func<S, K> keySelector,
                                                                          Func<S, V> valueSelector) =>
      GroupBy(reader, keySelector, valueSelector, default, default);

    /// <summary>
    /// Group By 
    /// </summary>
    /// <param name="reader">reader which items to group</param>
    /// <param name="keySelector">Key selector</param>
    /// <param name="keyComparer">Key Comparer</param>
    /// <param name="options">Parallel Options</param>
    /// <exception cref="ArgumentNullException">When reader or keySelector is null</exception>
    public static ChannelReader<ChannelGroup<K, V>> GroupBy<K, V>(this ChannelReader<V> reader,
                                                                       Func<V, K> keySelector,
                                                                       IEqualityComparer<K> keyComparer,
                                                                       ChannelParallelOptions options) =>
      GroupBy<K, V, V>(reader, keySelector, x => x, keyComparer, options);

    /// <summary>
    /// Group By 
    /// </summary>
    /// <param name="reader">reader which items to group</param>
    /// <param name="keySelector">Key selector</param>
    /// <param name="keyComparer">Key Comparer</param>
    /// <exception cref="ArgumentNullException">When reader or keySelector is null</exception>
    public static ChannelReader<ChannelGroup<K, V>> GroupBy<K, V>(this ChannelReader<V> reader,
                                                                       Func<V, K> keySelector,
                                                                       IEqualityComparer<K> keyComparer) =>
      GroupBy<K, V, V>(reader, keySelector, x => x, keyComparer, default);

    /// <summary>
    /// Group By 
    /// </summary>
    /// <param name="reader">reader which items to group</param>
    /// <param name="keySelector">Key selector</param>
    /// <param name="options">Parallel Options</param>
    /// <exception cref="ArgumentNullException">When reader or keySelector is null</exception>
    public static ChannelReader<ChannelGroup<K, V>> GroupBy<K, V>(this ChannelReader<V> reader,
                                                                       Func<V, K> keySelector,
                                                                       ChannelParallelOptions options) =>
      GroupBy<K, V, V>(reader, keySelector, x => x, default, options);

    /// <summary>
    /// Group By 
    /// </summary>
    /// <param name="reader">reader which items to group</param>
    /// <param name="keySelector">Key selector</param>
    /// <exception cref="ArgumentNullException">When reader or keySelector is null</exception>
    public static ChannelReader<ChannelGroup<K, V>> GroupBy<K, V>(this ChannelReader<V> reader,
                                                                       Func<V, K> keySelector) =>
      GroupBy<K, V, V>(reader, keySelector, x => x, default, default);

    #endregion Public
  }

}
