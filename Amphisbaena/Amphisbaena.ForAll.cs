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
  /// 
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static class ChannelReaderForAll {
    #region Algorithm

    // Single Thread ForAll
    private static async Task CoreForAll<T>(ChannelReader<T> reader, Action<T> action, CancellationToken token) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));
      if (action is null)
        throw new ArgumentNullException(nameof(action));

      token.ThrowIfCancellationRequested();

      await foreach (T item in reader.ReadAllAsync(token).WithCancellation(token).ConfigureAwait(false)) {
        action(item);
      }
    }

    #endregion Algorithm

    #region Public

    /// <summary>
    /// For All
    /// </summary>
    /// <param name="reader">Reader</param>
    /// <param name="action">Action To Perform</param>
    /// <param name="degreeOfParallelism">Degree Of Parallelism</param>
    /// <param name="token">Cancelation Token</param>
    /// <returns></returns>
    public static async Task ForAll<T>(ChannelReader<T> reader, 
                                       Action<T> action, 
                                       int degreeOfParallelism,
                                       CancellationToken token) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));
      if (action is null)
        throw new ArgumentNullException(nameof(action));

      token.ThrowIfCancellationRequested();

      if (degreeOfParallelism <= 0)
        degreeOfParallelism = Environment.ProcessorCount;

      if (degreeOfParallelism == 1) {
        await CoreForAll(reader, action, token);

        return;
      }

      async Task Executor(ChannelReader<T> source) {
        await foreach (T item in source.ReadAllAsync(token).WithCancellation(token).ConfigureAwait(false)) {
          action(item);
        }
      };

      await Task.Run(async () => {
        Channel<T>[] readers = Enumerable
          .Range(0, degreeOfParallelism)
          .Select(i => Channel.CreateUnbounded<T>())
          .ToArray();

        Task[] executors = Enumerable
          .Range(0, degreeOfParallelism)
          .Select(i => Executor(readers[i].Reader))
          .ToArray();
        
        await foreach (T item in reader.ReadAllAsync(token).WithCancellation(token).ConfigureAwait(false)) {
          int minCount = 0;
          int minIndex = -1;
          
          for (int i = 0; i < readers.Length; ++i) {
            int actualCount = readers[i].Reader.Count;

            if (minIndex < 0 || minCount > actualCount) {
              minCount = actualCount;
              minIndex = i;
            }
          }

          await readers[minIndex].Writer.WriteAsync(item, token);
        }

        foreach (var item in readers)
          item.Writer.Complete();

        await Task.WhenAll(executors);

      }, token);
    }

    #endregion Public
  }

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// 
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static partial class ChannelReaderExtension {
    #region Public

    /// <summary>
    /// 
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="action"></param>
    /// <param name="degreeOfParallelism"></param>
    /// <param name="token"></param>
    public static async Task ForAll<T>(this ChannelReader<T> reader,
                                            Action<T> action,
                                            int degreeOfParallelism,
                                            CancellationToken token) =>
      await ChannelReaderForAll.ForAll(reader, action, degreeOfParallelism, token);


    #endregion Public
  }

}
