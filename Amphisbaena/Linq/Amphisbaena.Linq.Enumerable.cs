using System;
using System.Collections.Concurrent;
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
    /// To AsyncEnumerable 
    /// </summary>
    /// <param name="reader">reader to be converted into IAsyncEnumerable</param>
    /// <param name="options">Parallel options</param>
    /// <exception cref="ArgumentNullException">When reader is null</exception>
    public static IAsyncEnumerable<T> ToAsyncEnumerable<T>(this ChannelReader<T> reader,
                                                                ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      return reader.ReadAllAsync(op.CancellationToken);
    }

    /// <summary>
    /// To AsyncEnumerable 
    /// </summary>
    /// <param name="reader">reader to be converted into IAsyncEnumerable</param>
    /// <exception cref="ArgumentNullException">When reader is null</exception>
    public static IAsyncEnumerable<T> ToAsyncEnumerable<T>(this ChannelReader<T> reader) =>
      ToAsyncEnumerable(reader, default);

    /// <summary>
    /// To Enumerable 
    /// </summary>
    /// <param name="reader">reader to be converted into IAsyncEnumerable</param>
    /// <param name="options">Parallel options</param>
    /// <exception cref="ArgumentNullException">When reader is null</exception>
    public static IEnumerable<T> ToEnumerable<T>(this ChannelReader<T> reader,
                                                      ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      BlockingCollection<T> result = op.Capacity > 0
        ? new BlockingCollection<T>(op.Capacity)
        : new BlockingCollection<T>();

      Task.Run(async () => {
        await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false))
          result.Add(item);

        result.CompleteAdding();
      }, op.CancellationToken);

      IEnumerable<T> RollUp() {
        if (result is null)
          throw new InvalidOperationException("Enumeration has been consumed and disposed.");

        try {
          foreach (T item in result.GetConsumingEnumerable())
            yield return item;
        }
        finally {
          result.Dispose();

          result = null;
        }
      }

      return RollUp();
    }

    /// <summary>
    /// To Enumerable 
    /// </summary>
    /// <param name="reader">reader to be converted into IAsyncEnumerable</param>
    /// <exception cref="ArgumentNullException">When reader is null</exception>
    public static IEnumerable<T> ToEnumerable<T>(this ChannelReader<T> reader) =>
      ToEnumerable(reader, default);

    #endregion Public
  }

}
