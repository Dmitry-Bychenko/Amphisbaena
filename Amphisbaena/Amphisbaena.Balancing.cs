using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;

namespace Amphisbaena {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Channel Balance Strategy
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public enum ChannelBalancerStrategy {
    /// <summary>
    /// Round Robin
    /// </summary>
    RoundRobin = 0,
    /// <summary>
    /// Even
    /// </summary>
    Even = 1,
    /// <summary>
    /// Default (Round Robin)
    /// </summary>
    Default = RoundRobin
  }

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Channel Balance Strategy Extensions
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static class ChannelBalancerStrategyExtensions {
    #region Public

    /// <summary>
    /// Create Balancer
    /// </summary>
    public static ChannelBalancer<T> Create<T>(this ChannelBalancerStrategy strategy, IEnumerable<Channel<T>> actors) {
      if (actors is null)
        throw new ArgumentNullException(nameof(actors));

      return strategy switch {
        ChannelBalancerStrategy.Even => new ChannelEvenBalancer<T>(actors),
        ChannelBalancerStrategy.RoundRobin => new ChannelRoundRobinBalancer<T>(actors),
        _ => new ChannelRoundRobinBalancer<T>(actors)
      };
    }

    #endregion Public
  }

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Abstract Channel Balancer
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public abstract class ChannelBalancer<T> {
    #region Private Data

    private List<Channel<T>> m_Actors;

    #endregion Private Data

    #region Algorithm

    /// <summary>
    /// Actors
    /// </summary>
    protected IReadOnlyList<Channel<T>> Actors => m_Actors;

    #endregion Algorithm

    #region Create

    /// <summary>
    /// Standard Constructor
    /// </summary>
    public ChannelBalancer(IEnumerable<Channel<T>> actors) {
      if (actors is null)
        throw new ArgumentNullException(nameof(actors));

      m_Actors = actors
        .Where(actor => actor is object)
        .Distinct()
        .ToList();

      if (m_Actors.Count <= 0)
        throw new ArgumentOutOfRangeException(nameof(actors), "At least one actor required");
    }

    #endregion Create

    #region Public

    /// <summary>
    /// Next Channel To assign the work to
    /// </summary>
    public abstract Channel<T> Next();

    #endregion Public
  }

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Round Robin Balancer
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public sealed class ChannelRoundRobinBalancer<T> : ChannelBalancer<T> {
    #region Private Data

    private int m_Next;

    #endregion Private Data

    #region Create

    /// <summary>
    /// Standard constructor
    /// </summary>
    public ChannelRoundRobinBalancer(IEnumerable<Channel<T>> actors)
      : base(actors) { }

    #endregion Create

    #region Public

    /// <summary>
    /// Next channel the chunk of work to be assigned
    /// </summary>
    /// <returns></returns>
    public override Channel<T> Next() {
      unchecked {
        int next = (Actors.Count + Interlocked.Increment(ref m_Next) % Actors.Count) % Actors.Count;

        return Actors[next];
      }
    }

    #endregion Public
  }

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Even Balancer
  /// <summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public sealed class ChannelEvenBalancer<T> : ChannelBalancer<T> {
    #region Create

    /// <summary>
    /// Standard constructor
    /// </summary>
    public ChannelEvenBalancer(IEnumerable<Channel<T>> actors)
      : base(actors) { }

    #endregion Create

    #region Public

    /// <summary>
    /// Next channel the chunk of work to be assigned
    /// </summary>
    public override Channel<T> Next() {
      Channel<T> result = null;
      int minWorkLoad = -1;

      for (int i = Actors.Count - 1; i >= 0; --i) {
        int load = Actors[i].Reader.Count;

        if (result == null || minWorkLoad < load) {
          minWorkLoad = load;
          result = Actors[i];
        }
      }

      return result;
    }

    #endregion Public
  }

}
