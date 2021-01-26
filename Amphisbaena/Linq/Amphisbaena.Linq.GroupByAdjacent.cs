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
    /// Group By adjacent items only
    /// </summary>
    /// <typeparam name="S">Source</typeparam>
    /// <typeparam name="K">Key</typeparam>
    /// <typeparam name="V">Value</typeparam>
    /// <param name="reader">reader to group</param>
    /// <param name="keySelector">Key from source</param>
    /// <param name="valueSelector">Value from source</param>
    /// <param name="comparer">Keys comparer</param>
    /// <param name="options">Parallel options</param>
    /// <returns>Groups of adjacent items</returns>
    public static ChannelReader<ChannelGroup<K, V>> GroupByAdjacent<S, K, V>(this ChannelReader<S> reader,
                                                                                  Func<S, K> keySelector,
                                                                                  Func<S, V> valueSelector,
                                                                                  IEqualityComparer<K> comparer,
                                                                                  ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));
      if (keySelector is null)
        throw new ArgumentNullException(nameof(keySelector));
      if (valueSelector is null)
        throw new ArgumentNullException(nameof(valueSelector));

      comparer ??= EqualityComparer<K>.Default;

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      Channel<ChannelGroup<K, V>> result = op.CreateChannel<ChannelGroup<K, V>>();

      Task.Run(async () => {
        ChannelGroup<K, V> group = null;

        await foreach (S item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false)) {
          K key = keySelector(item);
          V value = valueSelector(item);

          if ((group is null) || !comparer.Equals(key, group.Key)) {
            if (group is object) {
              group.Channel.Writer.TryComplete();

              await result.Writer.WriteAsync(group, op.CancellationToken).ConfigureAwait(false);
            }
            
            group = new ChannelGroup<K, V>(key, op);
          }
            
          await group.Channel.Writer.WriteAsync(value, op.CancellationToken).ConfigureAwait(false);
        }

        if (group is object) {
          group.Channel.Writer.TryComplete();

          await result.Writer.WriteAsync(group, op.CancellationToken).ConfigureAwait(false);
        }

        result.Writer.TryComplete();
      });

      return result.Reader;
    }

    /// <summary>
    /// Group By adjacent items only
    /// </summary>
    /// <typeparam name="S">Source</typeparam>
    /// <typeparam name="K">Key</typeparam>
    /// <typeparam name="V">Value</typeparam>
    /// <param name="reader">reader to group</param>
    /// <param name="keySelector">Key from source</param>
    /// <param name="valueSelector">Value from source</param>
    /// <param name="comparer">Keys comparer</param>
    /// <returns>Groups of adjacent items</returns>
    public static ChannelReader<ChannelGroup<K, V>> GroupByAdjacent<S, K, V>(this ChannelReader<S> reader,
                                                                                  Func<S, K> keySelector,
                                                                                  Func<S, V> valueSelector,
                                                                                  IEqualityComparer<K> comparer) =>
      GroupByAdjacent(reader, keySelector, valueSelector, comparer, default);

    /// <summary>
    /// Group By adjacent items only
    /// </summary>
    /// <typeparam name="S">Source</typeparam>
    /// <typeparam name="K">Key</typeparam>
    /// <typeparam name="V">Value</typeparam>
    /// <param name="reader">reader to group</param>
    /// <param name="keySelector">Key from source</param>
    /// <param name="valueSelector">Value from source</param>
    /// <param name="options">Parallel options</param>
    /// <returns>Groups of adjacent items</returns>
    public static ChannelReader<ChannelGroup<K, V>> GroupByAdjacent<S, K, V>(this ChannelReader<S> reader,
                                                                                  Func<S, K> keySelector,
                                                                                  Func<S, V> valueSelector,
                                                                                  ChannelParallelOptions options) =>
      GroupByAdjacent(reader, keySelector, valueSelector, default, options);

    /// <summary>
    /// Group By adjacent items only
    /// </summary>
    /// <typeparam name="S">Source</typeparam>
    /// <typeparam name="K">Key</typeparam>
    /// <typeparam name="V">Value</typeparam>
    /// <param name="reader">reader to group</param>
    /// <param name="keySelector">Key from source</param>
    /// <param name="valueSelector">Value from source</param>
    /// <returns>Groups of adjacent items</returns>
    public static ChannelReader<ChannelGroup<K, V>> GroupByAdjacent<S, K, V>(this ChannelReader<S> reader,
                                                                                  Func<S, K> keySelector,
                                                                                  Func<S, V> valueSelector) =>
      GroupByAdjacent(reader, keySelector, valueSelector, default, default);

    /// <summary>
    /// Group By adjacent items only
    /// </summary>
    /// <typeparam name="S">Source</typeparam>
    /// <typeparam name="K">Key</typeparam>
    /// <typeparam name="V">Value</typeparam>
    /// <param name="reader">reader to group</param>
    /// <param name="keySelector">Key from source</param>
    /// <param name="comparer">Keys comparer</param>
    /// <param name="options">Parallel options</param>
    /// <returns>Groups of adjacent items</returns>
    public static ChannelReader<ChannelGroup<K, S>> GroupByAdjacent<S, K>(this ChannelReader<S> reader,
                                                                               Func<S, K> keySelector,
                                                                               IEqualityComparer<K> comparer,
                                                                               ChannelParallelOptions options) =>
      GroupByAdjacent(reader, keySelector, x => x, comparer, options);

    /// <summary>
    /// Group By adjacent items only
    /// </summary>
    /// <typeparam name="S">Source</typeparam>
    /// <typeparam name="K">Key</typeparam>
    /// <typeparam name="V">Value</typeparam>
    /// <param name="reader">reader to group</param>
    /// <param name="keySelector">Key from source</param>
    /// <param name="comparer">Keys comparer</param>
    /// <returns>Groups of adjacent items</returns>
    public static ChannelReader<ChannelGroup<K, S>> GroupByAdjacent<S, K>(this ChannelReader<S> reader,
                                                                               Func<S, K> keySelector,
                                                                               IEqualityComparer<K> comparer) =>
      GroupByAdjacent(reader, keySelector, x => x, comparer, default);

    /// <summary>
    /// Group By adjacent items only
    /// </summary>
    /// <typeparam name="S">Source</typeparam>
    /// <typeparam name="K">Key</typeparam>
    /// <typeparam name="V">Value</typeparam>
    /// <param name="reader">reader to group</param>
    /// <param name="keySelector">Key from source</param>
    /// <param name="options">Parallel options</param>
    /// <returns>Groups of adjacent items</returns>
    public static ChannelReader<ChannelGroup<K, S>> GroupByAdjacent<S, K>(this ChannelReader<S> reader,
                                                                               Func<S, K> keySelector,
                                                                               ChannelParallelOptions options) =>
      GroupByAdjacent(reader, keySelector, x => x, default, options);

    /// <summary>
    /// Group By adjacent items only
    /// </summary>
    /// <typeparam name="S">Source</typeparam>
    /// <typeparam name="K">Key</typeparam>
    /// <typeparam name="V">Value</typeparam>
    /// <param name="reader">reader to group</param>
    /// <param name="keySelector">Key from source</param>
    /// <returns>Groups of adjacent items</returns>
    public static ChannelReader<ChannelGroup<K, S>> GroupByAdjacent<S, K>(this ChannelReader<S> reader,
                                                                               Func<S, K> keySelector) =>
      GroupByAdjacent(reader, keySelector, x => x, default, default);

    #endregion Public
  }

}
