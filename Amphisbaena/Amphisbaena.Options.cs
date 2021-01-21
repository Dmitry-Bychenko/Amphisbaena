using System;
using System.Threading;
using System.Threading.Channels;

namespace Amphisbaena {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Channel Parallel Options
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public sealed class ChannelParallelOptions {
    #region Private Data

    private int m_DegreeOfParallelism;

    #endregion Private Data

    #region Algorithm

    internal Channel<T> CreateChannel<T>() {
      if (Capacity <= 0)
        return Channel.CreateUnbounded<T>();
      else {
        BoundedChannelOptions ops = new BoundedChannelOptions(Capacity) {
          FullMode = BoundedChannelFullMode.Wait
        };

        return Channel.CreateBounded<T>(ops);
      }
    } 

    #endregion Algorithm

    #region Create

    /// <summary>
    /// Standard constructor
    /// </summary>
    public ChannelParallelOptions() { }

    /// <summary>
    /// Standard constructor
    /// </summary>
    public ChannelParallelOptions(CancellationToken token) : this() {
      CancellationToken = token;
    }

    /// <summary>
    /// Clone
    /// </summary>
    public ChannelParallelOptions Clone() => new ChannelParallelOptions() {
      CancellationToken = this.CancellationToken,
      DegreeOfParallelism = this.DegreeOfParallelism,
      Capacity = this.Capacity,
      BalancingStrategy = this.BalancingStrategy,
    };

    #endregion Create

    #region Public

    /// <summary>
    /// Cancellation Token
    /// </summary>
    public CancellationToken CancellationToken { get; set; }

    /// <summary>
    /// Degree Of Parallelism
    /// </summary>
    public int DegreeOfParallelism {
      get => m_DegreeOfParallelism <= 0
        ? Environment.ProcessorCount
        : m_DegreeOfParallelism;
      set {
        m_DegreeOfParallelism = value <= 0
          ? 0
          : value;
      }
    }

    /// <summary>
    /// Channel Capacity (unbounded, if capacity <= 0) 
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// Balancing Strategy
    /// </summary>
    public ChannelBalancerStrategy BalancingStrategy { get; set; }

    #endregion Public
  }

}
