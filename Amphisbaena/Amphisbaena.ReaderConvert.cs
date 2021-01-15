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
  /// Conversions and Materializations
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static class ChannelReaderConvertor {
    #region Public

    /// <summary>
    /// To Async Enumerable
    /// </summary>
    /// <param name="reader">Reader to Convert</param>
    /// <param name="token">Cancellation token</param>
    public static IAsyncEnumerable<T> ToAsyncEnumerable<T>(this ChannelReader<T> reader,
                                                                CancellationToken token) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));

      token.ThrowIfCancellationRequested();

      return reader.ReadAllAsync(token);
    }

    /// <summary>
    /// To Async Enumerable
    /// </summary>
    /// <param name="reader">Reader to Convert</param>
    public static IAsyncEnumerable<T> ToAsyncEnumerable<T>(this ChannelReader<T> reader) =>
      ToAsyncEnumerable(reader, default);

    /// <summary>
    /// To List
    /// </summary>
    /// <param name="reader">Reader to materialize</param>
    /// <param name="token">Cancellation token</param>
    /// <returns></returns>
    public static async Task<List<T>> ToListAsync<T>(this ChannelReader<T> reader, CancellationToken token) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));

      token.ThrowIfCancellationRequested();

      List<T> result = new List<T>();

      await foreach (T item in reader.ReadAllAsync(token).WithCancellation(token).ConfigureAwait(false))
        result.Add(item);

      return result;
    }

    /// <summary>
    /// To List
    /// </summary>
    /// <param name="reader">Reader to materialize</param>
    /// <returns></returns>
    public static async Task<List<T>> ToListAsync<T>(this ChannelReader<T> reader) =>
      await ToListAsync(reader, default);

    /// <summary>
    /// To Array
    /// </summary>
    /// <param name="reader">Reader to materialize</param>
    /// <param name="token">Cancellation token</param>
    /// <returns></returns>
    public static async Task<T[]> ToArrayAsync<T>(this ChannelReader<T> reader, CancellationToken token) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));

      token.ThrowIfCancellationRequested();

      List<T> result = new List<T>();

      await foreach (T item in reader.ReadAllAsync(token).WithCancellation(token).ConfigureAwait(false))
        result.Add(item);

      return result.ToArray();
    }

    /// <summary>
    /// To Array
    /// </summary>
    /// <param name="reader">Reader to materialize</param>
    /// <returns></returns>
    public static async Task<T[]> ToArrayAsync<T>(this ChannelReader<T> reader) =>
      await ToArrayAsync(reader, default);

    #endregion Public
  }

}
