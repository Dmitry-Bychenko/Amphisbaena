using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
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

  public sealed class ChannelParallelOptions
    : IEquatable<ChannelParallelOptions>,
      ISerializable {

    #region Private Data

    private int m_DegreeOfParallelism;
    private int m_Capacity;

    #endregion Private Data

    #region Algorithm

    internal Channel<T> CreateChannel<T>() {
      if (Capacity <= 0)
        return Channel.CreateUnbounded<T>();
      else {
        BoundedChannelOptions ops = new BoundedChannelOptions(Capacity) {
          FullMode = BoundedChannelFullMode.Wait,
          SingleReader = false,
          SingleWriter = false,
        };

        return Channel.CreateBounded<T>(ops);
      }
    }

    internal Balancer<Channel<T>> CreateBalancer<T>(IEnumerable<Channel<T>> actors) => BalancingStrategy
      .Create<Channel<T>>(actors, channel => channel.Reader.Count);

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

    // Deserialization constructor
    private ChannelParallelOptions(SerializationInfo info, StreamingContext context) {
      if (info is null)
        throw new ArgumentNullException(nameof(info));

      m_DegreeOfParallelism = info.GetInt32("Degree");
      m_Capacity = info.GetInt32("Capacity");
      BalancingStrategy = (ChannelBalancerStrategy)(info.GetInt32("Balancing"));

      if (context.State == StreamingContextStates.Clone)
        CancellationToken = (CancellationToken)(info.GetValue("Token", typeof(CancellationToken)));
    }

    /// <summary>
    /// Clone
    /// </summary>
    public ChannelParallelOptions Clone() => new ChannelParallelOptions() {
      CancellationToken = this.CancellationToken,
      m_DegreeOfParallelism = this.m_DegreeOfParallelism,
      m_Capacity = this.m_Capacity,
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
      set => m_DegreeOfParallelism = value <= 0 ? 0 : value;
    }

    /// <summary>
    /// Channel Capacity (unbounded, if capacity <= 0) 
    /// </summary>
    public int Capacity {
      get => m_Capacity;
      set => m_Capacity = value <= 0 ? 0 : value;
    }

    /// <summary>
    /// Balancing Strategy
    /// </summary>
    public ChannelBalancerStrategy BalancingStrategy { get; set; }

    /// <summary>
    /// To String (for debug only)
    /// </summary>
    public override string ToString() {
      return string.Join(Environment.NewLine,
        $"Cancellation:          {(CancellationToken.IsCancellationRequested ? "Cancelled" : CancellationToken.CanBeCanceled ? "Available" : "Not Available")}",
        $"Degree of Parallelism: {(m_DegreeOfParallelism <= 0 ? $"CPU based ({DegreeOfParallelism})" : $"{DegreeOfParallelism}")}",
        $"Capacity:              {(m_Capacity <= 0 ? "Unbounded" : $"{Capacity}")}",
        $"Balancing Strategy:    {BalancingStrategy}"
      );
    }

    #endregion Public

    #region IEquatable<ChannelParallelOptions>

    /// <summary>
    /// Equals
    /// </summary>
    public bool Equals(ChannelParallelOptions other) {
      if (other is null)
        return false;

      return (CancellationToken == other.CancellationToken) &&
             (m_DegreeOfParallelism == other.m_DegreeOfParallelism) &&
             (Capacity == other.Capacity) &&
             (BalancingStrategy == other.BalancingStrategy);
    }

    /// <summary>
    /// Equals 
    /// </summary>
    public override bool Equals(object obj) => obj is ChannelParallelOptions other && Equals(other);

    /// <summary>
    /// HashCode
    /// </summary>
    public override int GetHashCode() {
      unchecked {
        return (int)BalancingStrategy ^
               (m_DegreeOfParallelism << 3) ^
               (m_Capacity << 14) ^
               (CancellationToken.GetHashCode() << 24);
      }
    }

    #endregion IEquatable<ChannelParallelOptions>

    #region ISerializable

    /// <summary>
    /// Collect Data for serialization
    /// </summary>
    public void GetObjectData(SerializationInfo info, StreamingContext context) {
      if (info is null)
        throw new ArgumentNullException(nameof(info));

      if (context.State == StreamingContextStates.Clone)
        info.AddValue("Token", CancellationToken);

      info.AddValue("Degree", m_DegreeOfParallelism);
      info.AddValue("Capacity", m_Capacity);
      info.AddValue("Balancing", (int)BalancingStrategy);
    }

    #endregion ISerializable
  }

}
