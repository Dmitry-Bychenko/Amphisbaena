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
    /// If reader and other are equal
    /// </summary>
    /// <param name="reader">Reader to compare</param>
    /// <param name="other">(Other) reader to compare with</param>
    /// <param name="comparer">Comparer to use</param>
    /// <param name="options">Parallel Options</param>
    /// <returns>true, if readers have same items, order matters</returns>
    public static async Task<bool> SequenceEquals<T>(this ChannelReader<T> reader,
                                                          ChannelReader<T> other,
                                                          IEqualityComparer<T> comparer,
                                                          ChannelParallelOptions options) {
      if (ReferenceEquals(reader, other))
        return true;
      else if ((reader is null) || (other is null))
        return false;

      comparer ??= EqualityComparer<T>.Default;

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      var left = reader.ReadAllAsync(op.CancellationToken).GetAsyncEnumerator();
      var right = other.ReadAllAsync(op.CancellationToken).GetAsyncEnumerator();

      try {
        while (true) {
          bool hasLeft = await left.MoveNextAsync().ConfigureAwait(false);
          bool hasRight = await right.MoveNextAsync().ConfigureAwait(false);

          op.CancellationToken.ThrowIfCancellationRequested();

          if (!hasLeft && !hasRight)
            return true;
          else if (!hasLeft || !hasRight)
            return false;

          if (!comparer.Equals(left.Current, right.Current))
            return false;
        }
      }
      finally {
        await left.DisposeAsync().ConfigureAwait(false);
        await right.DisposeAsync().ConfigureAwait(false);
      }
    }

    /// <summary>
    /// If reader and other are equal
    /// </summary>
    /// <param name="reader">Reader to compare</param>
    /// <param name="other">(Other) reader to compare with</param>
    /// <param name="comparer">Comparer to use</param>
    /// <returns>true, if readers have same items, order matters</returns>
    public static async Task<bool> SequenceEquals<T>(this ChannelReader<T> reader,
                                                          ChannelReader<T> other,
                                                          IEqualityComparer<T> comparer) =>
      await SequenceEquals(reader, other, comparer, default).ConfigureAwait(false);

    /// <summary>
    /// If reader and other are equal
    /// </summary>
    /// <param name="reader">Reader to compare</param>
    /// <param name="other">(Other) reader to compare with</param>
    /// <param name="options">Parallel Options</param>
    /// <returns>true, if readers have same items, order matters</returns>
    public static async Task<bool> SequenceEquals<T>(this ChannelReader<T> reader,
                                                          ChannelReader<T> other,
                                                          ChannelParallelOptions options) =>
      await SequenceEquals(reader, other, default, options).ConfigureAwait(false);

    /// <summary>
    /// If reader and other are equal
    /// </summary>
    /// <param name="reader">Reader to compare</param>
    /// <param name="other">(Other) reader to compare with</param>
    /// <returns>true, if readers have same items, order matters</returns>
    public static async Task<bool> SequenceEquals<T>(this ChannelReader<T> reader,
                                                          ChannelReader<T> other) =>
      await SequenceEquals(reader, other, default, default).ConfigureAwait(false);

    /// <summary>
    /// If reader and other are equal
    /// </summary>
    /// <param name="reader">Reader to compare</param>
    /// <param name="other">Sequence to compare with</param>
    /// <param name="comparer">Comparer to use</param>
    /// <param name="options">Parallel Options</param>
    /// <returns>true, if readers have same items, order matters</returns>
    public static async Task<bool> SequenceEquals<T>(this ChannelReader<T> reader,
                                                          IAsyncEnumerable<T> other,
                                                          IEqualityComparer<T> comparer,
                                                          ChannelParallelOptions options) {
      if (ReferenceEquals(reader, other))
        return true;
      else if ((reader is null) || (other is null))
        return false;

      comparer ??= EqualityComparer<T>.Default;

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      var left = reader.ReadAllAsync(op.CancellationToken).GetAsyncEnumerator();
      var right = other.GetAsyncEnumerator();

      try {
        while (true) {
          bool hasLeft = await left.MoveNextAsync().ConfigureAwait(false);
          bool hasRight = await right.MoveNextAsync().ConfigureAwait(false);

          op.CancellationToken.ThrowIfCancellationRequested();

          if (!hasLeft && !hasRight)
            return true;
          else if (!hasLeft || !hasRight)
            return false;

          if (!comparer.Equals(left.Current, right.Current))
            return false;
        }
      }
      finally {
        await left.DisposeAsync().ConfigureAwait(false);
        await right.DisposeAsync().ConfigureAwait(false);
      }
    }

    /// <summary>
    /// If reader and other are equal
    /// </summary>
    /// <param name="reader">Reader to compare</param>
    /// <param name="other">Sequence to compare with</param>
    /// <param name="comparer">Comparer to use</param>
    /// <returns>true, if readers have same items, order matters</returns>
    public static async Task<bool> SequenceEquals<T>(this ChannelReader<T> reader,
                                                          IAsyncEnumerable<T> other,
                                                          IEqualityComparer<T> comparer) =>
      await SequenceEquals(reader, other, comparer, default).ConfigureAwait(false);

    /// <summary>
    /// If reader and other are equal
    /// </summary>
    /// <param name="reader">Reader to compare</param>
    /// <param name="other">Sequence to compare with</param>
    /// <param name="options">Parallel Options</param>
    /// <returns>true, if readers have same items, order matters</returns>
    public static async Task<bool> SequenceEquals<T>(this ChannelReader<T> reader,
                                                          IAsyncEnumerable<T> other,
                                                          ChannelParallelOptions options) =>
      await SequenceEquals(reader, other, default, options).ConfigureAwait(false);

    /// <summary>
    /// If reader and other are equal
    /// </summary>
    /// <param name="reader">Reader to compare</param>
    /// <param name="other">Sequence to compare with</param>
    /// <returns>true, if readers have same items, order matters</returns>
    public static async Task<bool> SequenceEquals<T>(this ChannelReader<T> reader,
                                                          IAsyncEnumerable<T> other) =>
      await SequenceEquals(reader, other, default, default).ConfigureAwait(false);

    /// <summary>
    /// If reader and other are equal
    /// </summary>
    /// <param name="reader">Reader to compare</param>
    /// <param name="other">Sequence to compare with</param>
    /// <param name="comparer">Comparer to use</param>
    /// <param name="options">Parallel Options</param>
    /// <returns>true, if readers have same items, order matters</returns>
    public static async Task<bool> SequenceEquals<T>(this ChannelReader<T> reader,
                                                          IEnumerable<T> other,
                                                          IEqualityComparer<T> comparer,
                                                          ChannelParallelOptions options) {
      if (ReferenceEquals(reader, other))
        return true;
      else if ((reader is null) || (other is null))
        return false;

      comparer ??= EqualityComparer<T>.Default;

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      var left = reader.ReadAllAsync(op.CancellationToken).GetAsyncEnumerator();
      var right = other.GetEnumerator();

      try {
        while (true) {
          bool hasLeft = await left.MoveNextAsync().ConfigureAwait(false);
          bool hasRight = right.MoveNext();

          op.CancellationToken.ThrowIfCancellationRequested();

          if (!hasLeft && !hasRight)
            return true;
          else if (!hasLeft || !hasRight)
            return false;

          if (!comparer.Equals(left.Current, right.Current))
            return false;
        }
      }
      finally {
        await left.DisposeAsync().ConfigureAwait(false);

        right.Dispose();
      }
    }

    /// <summary>
    /// If reader and other are equal
    /// </summary>
    /// <param name="reader">Reader to compare</param>
    /// <param name="other">Sequence to compare with</param>
    /// <param name="comparer">Comparer to use</param>
    /// <returns>true, if readers have same items, order matters</returns>
    public static async Task<bool> SequenceEquals<T>(this ChannelReader<T> reader,
                                                          IEnumerable<T> other,
                                                          IEqualityComparer<T> comparer) =>
      await SequenceEquals(reader, other, comparer, default).ConfigureAwait(false);

    /// <summary>
    /// If reader and other are equal
    /// </summary>
    /// <param name="reader">Reader to compare</param>
    /// <param name="other">Sequence to compare with</param>
    /// <param name="options">Parallel Options</param>
    /// <returns>true, if readers have same items, order matters</returns>
    public static async Task<bool> SequenceEquals<T>(this ChannelReader<T> reader,
                                                          IEnumerable<T> other,
                                                          ChannelParallelOptions options) =>
      await SequenceEquals(reader, other, default, options).ConfigureAwait(false);

    /// <summary>
    /// If reader and other are equal
    /// </summary>
    /// <param name="reader">Reader to compare</param>
    /// <param name="other">Sequence to compare with</param>
    /// <returns>true, if readers have same items, order matters</returns>
    public static async Task<bool> SequenceEquals<T>(this ChannelReader<T> reader,
                                                          IEnumerable<T> other) =>
      await SequenceEquals(reader, other, default, default).ConfigureAwait(false);

    #endregion Public
  }

}
