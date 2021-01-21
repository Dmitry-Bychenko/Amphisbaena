using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

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
    /// Random
    /// </summary>
    Random = 2,
    /// <summary>
    /// Default (Round Robin)
    /// </summary>
    Default = RoundRobin
  }

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Channel Balance Strategy
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public static class ChannelBalancerStrategyExtensions {
    #region Public

    public static Balancer<T> Create<T>(this ChannelBalancerStrategy strategy,
                                             IEnumerable<T> actors,
                                             Func<T, double> load) {
      return strategy switch {
        ChannelBalancerStrategy.Even => new EvenBalancer<T>(actors, load),
        ChannelBalancerStrategy.RoundRobin => new RoundRobinBalancer<T>(actors, load),
        ChannelBalancerStrategy.Random => new RandomBalancer<T>(actors, load),
        _ => new RandomBalancer<T>(actors, load)
      };

    }

    #endregion Public
  }

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Balancer (abstract)
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public abstract class Balancer<T> {
    #region Private Data

    private readonly List<T> m_Actors;

    private readonly Func<T, double> m_Load;

    #endregion Private Data

    #region Algorithm

    /// <summary>
    /// Available actors
    /// </summary>
    protected IReadOnlyList<T> Actors => m_Actors;

    /// <summary>
    /// Load of each actor 
    /// </summary>
    protected double Load(T actor) => m_Load(actor);

    #endregion Algorithm

    #region Create

    /// <summary>
    /// Standard constructor
    /// </summary>
    /// <param name="actors">actors</param>
    /// <param name="load">actor's load</param>
    public Balancer(IEnumerable<T> actors, Func<T, double> load) {
      if (actors is null)
        throw new ArgumentNullException(nameof(actors));

      m_Load = load ?? throw new ArgumentNullException(nameof(load));

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
    /// Next Actor 
    /// </summary>
    public abstract T NextActor();

    #endregion Public
  }

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Round Robin Balancer
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public sealed class RoundRobinBalancer<T> : Balancer<T> {
    #region Private Data

    private long m_Index;

    #endregion Private Data

    #region Create

    /// <summary>
    /// Standard Constructor
    /// </summary>
    public RoundRobinBalancer(IEnumerable<T> actors, Func<T, double> load)
      : base(actors, load) { }

    #endregion Create

    #region Public

    /// <summary>
    /// Next Actor
    /// </summary>
    public override T NextActor() =>
      Actors[(int)(Interlocked.Increment(ref m_Index) % Actors.Count)];

    #endregion Public
  }

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Even Balancer
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public sealed class EvenBalancer<T> : Balancer<T> {
    #region Create

    /// <summary>
    /// Standard Constructor
    /// </summary>
    public EvenBalancer(IEnumerable<T> actors, Func<T, double> load)
      : base(actors, load) { }

    #endregion Create

    #region Public

    /// <summary>
    /// Next Actor
    /// </summary>
    public override T NextActor() {
      double min = double.MaxValue;
      T result = default;

      for (int i = Actors.Count - 1; i >= 0; --i) {
        T actor = Actors[i];
        double load = Load(actor);

        if (load < min) {
          min = load;
          result = actor;
        }
      }

      return result;
    }

    #endregion Public
  }

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Random Balancer
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public sealed class RandomBalancer<T> : Balancer<T> {
    #region Private Data

    private static readonly ThreadLocal<Random> s_Random = new ThreadLocal<Random>(() => {
      int seed;

      using (RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider()) {
        byte[] seedData = new byte[sizeof(int)];

        provider.GetBytes(seedData);

        seed = BitConverter.ToInt32(seedData, 0);
      }

      return new Random(seed);
    });

    #endregion Private Data

    #region Create

    /// <summary>
    /// Standard Constructor
    /// </summary>
    public RandomBalancer(IEnumerable<T> actors, Func<T, double> load)
      : base(actors, load) { }

    #endregion Create

    #region Public

    /// <summary>
    /// Next Actor Selector
    /// </summary>
    public override T NextActor() => Actors[s_Random.Value.Next(0, Actors.Count)];

    #endregion Public
  }

}
