using System;
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

      await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false)) {
        int minCount = 0;
        int minIndex = -1;

        for (int i = 0; i < readers.Length; ++i) {
          int actualCount = readers[i].Reader.Count;

          if (minIndex < 0 || minCount > actualCount) {
            minCount = actualCount;
            minIndex = i;
          }
        }

        await readers[minIndex].Writer.WriteAsync(item, op.CancellationToken).ConfigureAwait(false);
      }

      foreach (var item in readers)
        item.Writer.TryComplete();

      await Task.WhenAll(executors).ConfigureAwait(false);
    }

    /// <summary>
    /// For All
    /// </summary>
    public static async Task ForAll<T>(this ChannelReader<T> reader, Action<T> action) =>
      await ForAll(reader, action, null);

    #endregion Public
  }

}
