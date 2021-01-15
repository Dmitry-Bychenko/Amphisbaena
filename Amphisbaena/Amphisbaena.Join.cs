using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Amphisbaena {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Joining several Channel Readers into one 
  /// </summary>
  // 
  //-------------------------------------------------------------------------------------------------------------------

  public static class ChannelReaderJoin {
    #region Public

    /// <summary>
    /// Join Several Channel Readers into one 
    /// </summary>
    /// <param name="readers">Readers to join</param>
    /// <param name="token">Cancellation Token</param>
    /// <returns></returns>
    public static ChannelReader<T> Join<T>(IEnumerable<ChannelReader<T>> readers, CancellationToken token) {
      if (readers is null)
        throw new ArgumentNullException(nameof(readers));

      token.ThrowIfCancellationRequested();

      ChannelReader<T>[] source = readers
        .Where(reader => reader is not null)
        .Distinct()
        .ToArray();

      if (source.Length == 0)
        return ChannelReaderFactory.Empty<T>();
      if (source.Length == 1)
        return source[0];

      Channel<T> result = Channel.CreateUnbounded<T>();

      Task.Run(async () => {
        async Task WriterTask(ChannelReader<T> reader) {
          await foreach (T item in reader.ReadAllAsync().WithCancellation(token).ConfigureAwait(false))
            await result.Writer.WriteAsync(item, token);
        }

        await Task.WhenAll(source.Select(reader => WriterTask(reader)).ToArray());

        result.Writer.Complete();
      }, token);

      return result;
    }

    /// <summary>
    /// Join Several Channel Readers into one 
    /// </summary>
    /// <param name="readers">Readers to join</param>
    /// <returns></returns>
    public static ChannelReader<T> Join<T>(IEnumerable<ChannelReader<T>> readers) =>
      Join(readers, default);

    #endregion Public
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
    /// Merge Channel Reader with others
    /// </summary>
    /// <param name="reader">Reader</param>
    /// <param name="others">Readers to Merge</param>
    /// <param name="token">Cancellation token</param>
    /// <returns></returns>
    public static ChannelReader<T> Merge<T>(this ChannelReader<T> reader, 
                                                 IEnumerable<ChannelReader<T>> others,
                                                 CancellationToken token) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));
      if (others is null)
        throw new ArgumentNullException(nameof(others));

      return ChannelReaderJoin.Join(new ChannelReader<T>[] { reader }.Concat(others), token);
    }

    /// <summary>
    /// Merge Channel Reader with others
    /// </summary>
    /// <param name="reader">Reader</param>
    /// <param name="others">Readers to Merge</param>
    /// <returns></returns>
    public static ChannelReader<T> Merge<T>(this ChannelReader<T> reader, IEnumerable<ChannelReader<T>> others) =>
      Merge(reader, others, default);

    /// <summary>
    /// Merge Channel Reader with others
    /// </summary>
    /// <param name="reader">Reader</param>
    /// <param name="others">Readers to Merge</param>
    /// <param name="token">Cancellation token</param>
    /// <returns></returns>
    public static ChannelReader<T> Merge<T>(this ChannelReader<T> reader,
                                                 CancellationToken token,
                                          params ChannelReader<T>[] others) =>
      Merge(reader, others, token);

    /// <summary>
    /// Merge Channel Reader with others
    /// </summary>
    /// <param name="reader">Reader</param>
    /// <param name="others">Readers to Merge</param>
    /// <returns></returns>
    public static ChannelReader<T> Merge<T>(this ChannelReader<T> reader,
                                         params ChannelReader<T>[] others) =>
      Merge(reader, others, default);

    #endregion Public
  }

}
