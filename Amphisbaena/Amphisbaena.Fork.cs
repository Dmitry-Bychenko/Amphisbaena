using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Amphisbaena {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Fork
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static class ChannelReaderFork {
    #region Public

    /// <summary>
    /// Fork according to conditions
    /// </summary>
    /// <param name="reader">Initial reader to fork</param>
    /// <param name="conditions">Conditions for i-th fork; null stands for always true</param>
    /// <param name="options">Parallel options</param>
    public static ChannelReader<T>[] Fork<T>(this ChannelReader<T> reader,
                                                  IEnumerable<Func<T, bool>> conditions,
                                                  ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      conditions ??= Enumerable
        .Range(0, op.DegreeOfParallelism)
        .Select(_x => new Func<T, bool>(x => true))
        .ToArray();

      Func<T, bool>[] funcs = conditions
        .Select(f => f ?? (x => true))
        .ToArray();

      if (0 == funcs.Length)
        return Array.Empty<ChannelReader<T>>();
      if (1 == funcs.Length)
        return new ChannelReader<T>[] { reader };

      var result = Enumerable
        .Range(0, funcs.Length)
        .Select(_x => op.CreateChannel<T>())
        .ToArray();

      Task.Run(async () => {
        await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false)) {
          for (int i = 0; i < funcs.Length; ++i)
            if (funcs[i](item))
              await result[i].Writer.WriteAsync(item, op.CancellationToken).ConfigureAwait(false);
        }

        for (int i = 0; i < result.Length; ++i)
          result[i].Writer.TryComplete();
      }, op.CancellationToken);

      return result
        .Select(channel => channel.Reader)
        .ToArray();
    }

    /// <summary>
    /// Fork according to conditions
    /// </summary>
    /// <param name="reader">Initial reader to fork</param>
    /// <param name="conditions">Conditions for i-th fork; null stands for always true</param>
    /// <param name="options">Parallel options</param>
    public static ChannelReader<T>[] Fork<T>(this ChannelReader<T> reader,
                                                  ChannelParallelOptions options,
                                           params Func<T, bool>[] conditions) =>
      Fork(reader, conditions, options);

    /// <summary>
    /// Fork according to conditions
    /// </summary>
    /// <param name="reader">Initial reader to fork</param>
    /// <param name="conditions">Conditions for i-th fork; null stands for always true</param>
    public static ChannelReader<T>[] Fork<T>(this ChannelReader<T> reader,
                                           params Func<T, bool>[] conditions) =>
      Fork(reader, conditions, default);

    /// <summary>
    /// Fork according to conditions
    /// </summary>
    /// <param name="reader">Initial reader to fork</param>
    /// <param name="conditions">Conditions for i-th fork; null stands for always true</param>
    public static ChannelReader<T>[] Fork<T>(this ChannelReader<T> reader,
                                                  IEnumerable<Func<T, bool>> conditions) =>
      Fork(reader, conditions, default);

    /// <summary>
    /// Fork according to conditions
    /// </summary>
    /// <param name="reader">Initial reader to fork</param>
    /// <param name="options">Parallel options</param>
    public static ChannelReader<T>[] Fork<T>(this ChannelReader<T> reader,
                                                  ChannelParallelOptions options) =>
      Fork(reader, default, options);

    /// <summary>
    /// Fork according to conditions
    /// </summary>
    /// <param name="reader">Initial reader to fork</param>
    /// <param name="numberOfForks">Number Of Forks</param>
    public static ChannelReader<T>[] Fork<T>(this ChannelReader<T> reader,
                                                  int numberOfForks) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));
      if (numberOfForks < 0)
        throw new ArgumentOutOfRangeException(nameof(numberOfForks));

      return Fork(reader, new Func<T, bool>[numberOfForks], default);
    }

    /// <summary>
    /// Detach
    /// </summary>
    /// <param name="reader">Initial reader to detach from</param>
    /// <param name="detached">Detached channel</param>
    /// <param name="condition">Condition on detached channel</param>
    /// <param name="options">Options</param>
    public static ChannelReader<T> Detach<T>(this ChannelReader<T> reader,
                                              out ChannelReader<T> detached,
                                                  Func<T, bool> condition,
                                                  ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));

      condition ??= x => true;

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      Channel<T> result = op.CreateChannel<T>();
      Channel<T> detachedChannel = op.CreateChannel<T>();

      detached = detachedChannel.Reader;

      Task.Run(async () => {
        await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false)) {
          await result.Writer.WriteAsync(item, op.CancellationToken).ConfigureAwait(false);

          if (condition(item))
            await detachedChannel.Writer.WriteAsync(item, op.CancellationToken).ConfigureAwait(false);
        }

        result.Writer.TryComplete();
        detachedChannel.Writer.TryComplete();
      });

      return result.Reader;
    }

    /// <summary>
    /// Detach
    /// </summary>
    /// <param name="reader">Initial reader to detach from</param>
    /// <param name="detached">Detached channel</param>
    /// <param name="condition">Condition on detached channel</param>
    public static ChannelReader<T> Detach<T>(this ChannelReader<T> reader,
                                              out ChannelReader<T> detached,
                                                  Func<T, bool> condition) =>
      Detach(reader, out detached, condition, default);

    /// <summary>
    /// Detach
    /// </summary>
    /// <param name="reader">Initial reader to detach from</param>
    /// <param name="detached">Detached channel</param>
    /// <param name="options">Options</param>
    public static ChannelReader<T> Detach<T>(this ChannelReader<T> reader,
                                              out ChannelReader<T> detached,
                                                  ChannelParallelOptions options) =>
      Detach(reader, out detached, default, options);

    /// <summary>
    /// Detach
    /// </summary>
    /// <param name="reader">Initial reader to detach from</param>
    /// <param name="detached">Detached channel</param>
    public static ChannelReader<T> Detach<T>(this ChannelReader<T> reader,
                                              out ChannelReader<T> detached) =>
      Detach(reader, out detached, default, default);

    #endregion Public
  }

}
