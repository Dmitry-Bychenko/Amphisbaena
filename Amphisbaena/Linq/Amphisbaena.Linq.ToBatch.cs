using System;
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
    /// Batch grouping
    /// </summary>
    /// <param name="reader">reader to batch</param>
    /// <param name="addToBatch">should item be added to current batch</param>
    /// <param name="options">parallel options</param>
    public static ChannelReader<T[]> ToBatch<T>(this ChannelReader<T> reader,
                                                     Func<ICollection<T>, T, long, bool> addToBatch,
                                                     ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));
      if (addToBatch is null)
        throw new ArgumentNullException(nameof(addToBatch));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      Channel<T[]> result = op.CreateChannel<T[]>();

      Task.Run(async () => {
        long index = -1;
        List<T> batch = new ();

        await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false)) {
          index += 1;

          if (batch.Count > 0 && !addToBatch(batch, item, index)) {
            await result.Writer.WriteAsync(batch.ToArray(), op.CancellationToken).ConfigureAwait(false);

            batch.Clear();
          }

          batch.Add(item);
        }

        if (batch.Count > 0)
          await result.Writer.WriteAsync(batch.ToArray(), op.CancellationToken).ConfigureAwait(false);

        result.Writer.TryComplete();
      }, op.CancellationToken);

      return result.Reader;
    }

    /// <summary>
    /// Batch grouping
    /// </summary>
    /// <param name="reader">reader to batch</param>
    /// <param name="addToBatch">should item be added to current batch</param>
    public static ChannelReader<T[]> ToBatch<T>(this ChannelReader<T> reader,
                                                     Func<ICollection<T>, T, long, bool> addToBatch) =>
      ToBatch(reader, addToBatch, null);

    #endregion Public
  }

}
