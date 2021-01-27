using System.Collections.Generic;
using System.Linq;
using System;
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
    /// Concat
    /// </summary>
    /// <param name="reader">initial reader</param>
    /// <param name="other">other readers to concat</param>
    /// <param name="options">parallel options</param>
    public static ChannelReader<T> Concat<T>(this ChannelReader<T> reader, 
                                                  IEnumerable<ChannelReader<T>> other,
                                                  ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));
      if (other is null)
        throw new ArgumentNullException(nameof(reader));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      ChannelReader<T>[] data = other
        .Where(item => item != null && !ReferenceEquals(item, reader))
        .Distinct()
        .ToArray();

      if (data.Length <= 0)
        return reader;

      Channel<T> result = op.CreateChannel<T>();

      Task.Run(async () => {
        await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false)) 
          await result.Writer.WriteAsync(item, op.CancellationToken).ConfigureAwait(false);
        
        foreach (var rd in data)
          await foreach (T item in rd.ReadAllAsync(op.CancellationToken).ConfigureAwait(false))
            await result.Writer.WriteAsync(item, op.CancellationToken).ConfigureAwait(false);

        result.Writer.TryComplete();
      });

      return result.Reader;
    }

    /// <summary>
    /// Concat
    /// </summary>
    /// <param name="reader">initial reader</param>
    /// <param name="other">other readers to concat</param>
    public static ChannelReader<T> Concat<T>(this ChannelReader<T> reader,
                                                  IEnumerable<ChannelReader<T>> other) =>
      Concat(reader, other, default);

    /// <summary>
    /// Concat
    /// </summary>
    /// <param name="reader">initial reader</param>
    /// <param name="other">other readers to concat</param>
    /// <param name="options">parallel options</param>
    public static ChannelReader<T> Concat<T>(this ChannelReader<T> reader,
                                                  ChannelParallelOptions options,
                                                  params ChannelReader<T>[] other) =>
      Concat(reader, other, options);

    /// <summary>
    /// Concat
    /// </summary>
    /// <param name="reader">initial reader</param>
    /// <param name="other">other readers to concat</param>
    public static ChannelReader<T> Concat<T>(this ChannelReader<T> reader,
                                                  params ChannelReader<T>[] other) =>
      Concat(reader, other, default);

    /// <summary>
    /// Concat
    /// </summary>
    /// <param name="reader">initial reader</param>
    /// <param name="other">other readers to concat</param>
    /// <param name="options">parallel options</param>
    public static ChannelReader<T> Concat<T>(this ChannelReader<T> reader,
                                                  IEnumerable<IEnumerable<T>> other,
                                                  ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));
      if (other is null)
        throw new ArgumentNullException(nameof(reader));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      IEnumerable<T>[] data = other
        .Where(item => item != null)
        .Distinct()
        .ToArray();

      if (data.Length <= 0)
        return reader;

      Channel<T> result = op.CreateChannel<T>();

      Task.Run(async () => {
        await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false))
          await result.Writer.WriteAsync(item, op.CancellationToken).ConfigureAwait(false);

        foreach (var rd in data)
          foreach (T item in rd)
            await result.Writer.WriteAsync(item, op.CancellationToken).ConfigureAwait(false);

        result.Writer.TryComplete();
      });

      return result.Reader;
    }

    /// <summary>
    /// Concat
    /// </summary>
    /// <param name="reader">initial reader</param>
    /// <param name="other">other readers to concat</param>
    public static ChannelReader<T> Concat<T>(this ChannelReader<T> reader,
                                                  IEnumerable<IEnumerable<T>> other) =>
      Concat(reader, other, default);

    /// <summary>
    /// Concat
    /// </summary>
    /// <param name="reader">initial reader</param>
    /// <param name="other">other readers to concat</param>
    /// <param name="options">parallel options</param>
    public static ChannelReader<T> Concat<T>(this ChannelReader<T> reader,
                                                  ChannelParallelOptions options,
                                                  params IEnumerable<T>[] other) =>
      Concat(reader, other, options);

    /// <summary>
    /// Concat
    /// </summary>
    /// <param name="reader">initial reader</param>
    /// <param name="other">other readers to concat</param>
    /// <param name="options">parallel options</param>
    public static ChannelReader<T> Concat<T>(this ChannelReader<T> reader,
                                                  params IEnumerable<T>[] other) =>
      Concat(reader, other, default);

    #endregion Public
  }

}
