using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Amphisbaena.Linq {

  // Wrapper over IDisposable 
  internal class WrappedEnumerable<T> {
    #region Private Data

    private BlockingCollection<T> m_Collection;

    #endregion Private Data

    #region Create

    internal WrappedEnumerable(BlockingCollection<T> collection) {
      m_Collection = collection ?? throw new ArgumentNullException(nameof(collection));
    }

    #endregion Public

    #region Public

    /// <summary>
    /// Enumerate
    /// </summary>
    public IEnumerable<T> Enumerate() {
      if (m_Collection is null)
        throw new InvalidOperationException("Enumerator has been disposed.");

      try {
        foreach (T item in m_Collection.GetConsumingEnumerable())
          yield return item;
      }
      finally {
        m_Collection.Dispose();
        m_Collection = null;
      }
    }

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
    /// To AsyncEnumerable 
    /// </summary>
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
    public static IAsyncEnumerable<T> ToAsyncEnumerable<T>(this ChannelReader<T> reader) =>
      ToAsyncEnumerable(reader, default);

    /// <summary>
    /// To Enumerable 
    /// </summary>
    public static IEnumerable<T> ToEnumerable<T>(this ChannelReader<T> reader,
                                                      ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      BlockingCollection<T> result = new BlockingCollection<T>();

      Task.Run(async () => {
        await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false))
          result.Add(item);

        result.CompleteAdding();
      }, op.CancellationToken);

      WrappedEnumerable<T> wrapper = new WrappedEnumerable<T>(result);

      return wrapper.Enumerate();
    }

    /// <summary>
    /// To Enumerable 
    /// </summary>
    public static IEnumerable<T> ToEnumerable<T>(this ChannelReader<T> reader) =>
      ToEnumerable(reader, default);

    #endregion Public
  }

}
