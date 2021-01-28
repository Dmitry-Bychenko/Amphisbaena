using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Amphisbaena {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Channel Reader Perform
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static class ChannelReaderPerformer {
    #region Public

    /// <summary>
    /// For All
    /// </summary>
    /// <param name="reader">reader to get items from</param>
    /// <param name="action">action to perform on each item</param>
    /// <param name="options">parallel options</param>
    /// <exception cref="ArgumentNullException">When reader or action is null</exception>
    public static async Task ForAll<T>(this ChannelReader<T> reader,
                                            Action<T> action,
                                            ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));
      if (action is null)
        throw new ArgumentNullException(nameof(action));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      async Task Executor(ChannelReader<T> source) {
        await foreach (T item in source.ReadAllAsync(op.CancellationToken).ConfigureAwait(false)) {
          action(item);
        }
      };

      Channel<T>[] readers = Enumerable
        .Range(0, op.DegreeOfParallelism)
        .Select(i => op.CreateChannel<T>())
        .ToArray();

      Task[] executors = Enumerable
        .Range(0, op.DegreeOfParallelism)
        .Select(i => Executor(readers[i].Reader))
        .ToArray();

      var balancer = op.CreateBalancer(readers);

      await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false)) {
        var actor = balancer.NextActor();

        await actor.Writer.WriteAsync(item, op.CancellationToken).ConfigureAwait(false);
      }

      foreach (var item in readers)
        item.Writer.TryComplete();

      await Task.WhenAll(executors).ConfigureAwait(false);
    }

    /// <summary>
    /// For All
    /// </summary>
    /// <param name="reader">reader to get items from</param>
    /// <param name="action">action to perform on each item</param>
    /// <exception cref="ArgumentNullException">When reader or action is null</exception>
    public static async Task ForAll<T>(this ChannelReader<T> reader, Action<T> action) =>
      await ForAll(reader, action, default).ConfigureAwait(false);

    /// <summary>
    /// For Each (Parallelized Select) 
    /// </summary>
    /// <param name="reader">reader to get items from</param>
    /// <param name="selector">action to perform on each item</param>
    /// <param name="options">parallel options</param>
    /// <exception cref="ArgumentNullException">When reader or selector is null</exception>
    public static ChannelReader<T> ForEach<S, T>(this ChannelReader<S> reader,
                                                      Func<S, T> selector,
                                                      ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));
      if (selector is null)
        throw new ArgumentNullException(nameof(selector));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      Channel<T> result = op.CreateChannel<T>();

      Task.Run(async () => {
        HashSet<Task> actors = new HashSet<Task>();

        await foreach (S item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false)) {
          while (actors.Count >= op.DegreeOfParallelism)
            actors.Remove(await Task.WhenAny(actors));

          actors.Add(Task.Run(async () =>
            await result.Writer.WriteAsync(selector(item), op.CancellationToken).ConfigureAwait(false)
          ));
        }

        await Task.WhenAll(actors).ConfigureAwait(false);

        result.Writer.TryComplete();
      }, op.CancellationToken);

      return result.Reader;
    }

    /// <summary>
    /// For Each (Parallelized Select) 
    /// </summary>
    /// <param name="reader">reader to get items from</param>
    /// <param name="selector">action to perform on each item</param>
    /// <exception cref="ArgumentNullException">When reader or selector is null</exception>
    public static ChannelReader<T> ForEach<S, T>(this ChannelReader<S> reader, Func<S, T> selector) =>
      ForEach(reader, selector, default);

    #endregion Public
  }

}
