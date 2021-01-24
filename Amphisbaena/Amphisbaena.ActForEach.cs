using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Amphisbaena {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// 
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static class ChannelReaderActor {
    #region Public

    /// <summary>
    /// Act for each item in the channel
    /// </summary>
    public static ChannelReader<T> ActForEach<T>(this ChannelReader<T> reader,
                                                      Action<T, long> action,
                                                      Func<T, long, bool> condition,
                                                      ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));
      if (action is null)
        throw new ArgumentNullException(nameof(action));

      condition ??= (value, index) => true;

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      Channel<T> result = op.CreateChannel<T>();

      Task.Run(async () => {
        long index = -1;

        await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false)) {
          index += 1;

          if (condition(item, index))
            action(item, index);

          await result.Writer.WriteAsync(item, op.CancellationToken).ConfigureAwait(false);
        }

        result.Writer.TryComplete();
      }, op.CancellationToken);

      return result.Reader;
    }

    /// <summary>
    /// Act for each item in the channel
    /// </summary>
    public static ChannelReader<T> ActForEach<T>(this ChannelReader<T> reader,
                                                      Action<T, long> action,
                                                      Func<T, long, bool> condition) =>
      ActForEach(reader, action, condition, default);

    /// <summary>
    /// Act for each item in the channel
    /// </summary>
    public static ChannelReader<T> ActForEach<T>(this ChannelReader<T> reader,
                                                      Action<T, long> action,
                                                      ChannelParallelOptions options) =>
      ActForEach(reader, action, default, options);

    /// <summary>
    /// Act for each item in the channel
    /// </summary>
    public static ChannelReader<T> ActForEach<T>(this ChannelReader<T> reader,
                                                      Action<T, long> action) =>
      ActForEach(reader, action, default, default);

    /// <summary>
    /// Act for each item in the channel
    /// </summary>
    public static ChannelReader<T> ActForEach<T>(this ChannelReader<T> reader,
                                                      Action<T> action,
                                                      Func<T, bool> condition,
                                                      ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));
      if (action is null)
        throw new ArgumentNullException(nameof(action));

      condition ??= value => true;

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      Channel<T> result = op.CreateChannel<T>();

      Task.Run(async () => {
        await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false)) {
          if (condition(item))
            action(item);

          await result.Writer.WriteAsync(item, op.CancellationToken).ConfigureAwait(false);
        }

        result.Writer.TryComplete();
      }, op.CancellationToken);

      return result.Reader;
    }

    /// <summary>
    /// Act for each item in the channel
    /// </summary>
    public static ChannelReader<T> ActForEach<T>(this ChannelReader<T> reader,
                                                      Action<T> action,
                                                      Func<T, bool> condition) =>
      ActForEach(reader, action, condition, default);

    /// <summary>
    /// Act for each item in the channel
    /// </summary>
    public static ChannelReader<T> ActForEach<T>(this ChannelReader<T> reader,
                                                      Action<T> action,
                                                      ChannelParallelOptions options) =>
      ActForEach(reader, action, default, options);

    /// <summary>
    /// Act for each item in the channel
    /// </summary>
    public static ChannelReader<T> ActForEach<T>(this ChannelReader<T> reader,
                                                      Action<T> action) =>
      ActForEach(reader, action, default, default);

    #endregion Public
  }

}
