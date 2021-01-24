using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Amphisbaena.Linq {

  //-------------------------------------------------------------------------------------------------------------------
  //
  /// <summary>
  /// Statistics Info
  /// </summary>
  //
  //-------------------------------------------------------------------------------------------------------------------

  public sealed class Statistics<T> {
    #region Private Data

    private T m_MinArg;
    private T m_MaxArg;

    #endregion Private Data

    #region Algorithm

    private void UpdateCount(double value) {
      if (double.IsNaN(value))
        CountNaN += 1;
      else if (double.IsPositiveInfinity(value))
        CountPositiveInf += 1;
      else if (double.IsNegativeInfinity(value))
        CountNegativeInf += 1;
      else
        CountRegular += 1;
    }

    private void UpdateSums(double value) {
      if (double.IsNaN(value))
        return;

      double x = value;

      Sum += x;
      Sum2 += (x *= value);
      Sum3 += (x *= value);
      Sum4 += (x *= value);
    }

    private void UpdateExtemums(T item, long index, double value) {
      if (double.IsNaN(value))
        return;

      if (double.IsNaN(Min)) {
        Min = value;
        MinIndex = index;
        m_MinArg = item;

        Max = value;
        MaxIndex = index;
        m_MaxArg = item;
      }
      else if (Min > value) {
        Min = value;
        MinIndex = index;
        m_MinArg = item;
      }
      else if (Max < value) {
        Max = value;
        MaxIndex = index;
        m_MaxArg = item;
      }
    }

    internal void Add(T item, long index, double value) {
      UpdateCount(value);
      UpdateSums(value);
      UpdateExtemums(item, index, value);
    }

    #endregion Algorithm

    #region Create

    // Standard cosntructor 
    internal Statistics() { }

    #endregion Create

    #region Public

    #region Count

    /// <summary>
    /// Count (regular values only; NaN, Inf excluded)
    /// </summary>
    public long CountRegular { get; private set; }

    /// <summary>
    /// Count (NaN excluded)
    /// </summary>
    public long Count => CountRegular + CountPositiveInf + CountNegativeInf;

    /// <summary>
    /// Count NaN
    /// </summary>
    public long CountNaN { get; private set; }

    /// <summary>
    /// Count +Inf
    /// </summary>
    public long CountPositiveInf { get; private set; }

    /// <summary>
    /// Count -Inf
    /// </summary>
    public long CountNegativeInf { get; private set; }

    /// <summary>
    /// Count Inf (+Inf, -Inf)
    /// </summary>
    public long CountInf => CountPositiveInf + CountNegativeInf;

    /// <summary>
    /// Count Total
    /// </summary>
    public long CountTotal => CountRegular + CountNaN + CountPositiveInf + CountNegativeInf;

    #endregion Count

    #region Sums

    /// <summary>
    /// Sum
    /// </summary>
    public double Sum { get; private set; }

    /// <summary>
    /// Sum of Squares
    /// </summary>
    public double Sum2 { get; private set; }

    /// <summary>
    /// Sum of Cubes
    /// </summary>
    public double Sum3 { get; private set; }

    /// <summary>
    /// Sums of Fourth Powers
    /// </summary>
    public double Sum4 { get; private set; }

    #endregion Sums

    #region Extremums

    /// <summary>
    /// Min Index
    /// </summary>
    public long MinIndex { get; private set; } = -1;

    /// <summary>
    /// Min
    /// </summary>
    public double Min { get; private set; } = double.NaN;

    /// <summary>
    /// Argmin
    /// </summary>
    public T ArgMin => !double.IsNaN(Min)
      ? m_MinArg
      : throw new InvalidOperationException("No ArgMin for empty reqular collection.");

    /// <summary>
    /// Max Index
    /// </summary>
    public long MaxIndex { get; private set; } = -1;

    /// <summary>
    /// Max
    /// </summary>
    public double Max { get; private set; } = double.NaN;

    /// <summary>
    /// Argmax
    /// </summary>
    public T ArgMax => !double.IsNaN(Max)
      ? m_MaxArg
      : throw new InvalidOperationException("No ArgMax for empty reqular collection.");

    #endregion Extremums

    #region Standard

    /// <summary>
    /// Average
    /// </summary>
    public double Average => Sum / Count;

    /// <summary>
    /// Variance
    /// </summary>
    public double Variance => Sum2 / Count - Sum * Sum / Count / Count;

    /// <summary>
    /// Standard Error
    /// </summary>
    public double StandardError => Math.Sqrt(Variance);

    /// <summary>
    /// Skew
    /// </summary>
    public double Skew {
      get {
        if (Count <= 0)
          return double.NaN;

        double m1 = Sum / Count;
        double m2 = Sum2 / Count;
        double m3 = Sum3 / Count;

        double s = m3 - 3 * m2 * m1 + 2 * m1 * m1 * m1;
        double d = m2 - m1 * m1;

        if (d == 0)
          if (s < 0)
            return Double.NegativeInfinity;
          else
            return Double.PositiveInfinity;
        else
          return s / Math.Sqrt(d * d * d);
      }
    }

    /// <summary>
    /// Kurtosis
    /// </summary>
    public double Kurtosis {
      get {
        if (Count <= 0)
          return double.NaN;

        double m1 = Sum / Count;
        double m2 = Sum2 / Count;
        double m3 = Sum3 / Count;
        double m4 = Sum4 / Count;

        double s = m4 - 4 * m3 * m1 + 6 * m2 * m1 * m1 - 3 * m1 * m1 * m1 * m1;
        double d = m2 - m1 * m1;

        if (d == 0)
          if (s < 0)
            return double.NegativeInfinity;
          else
            return double.PositiveInfinity;
        else
          return s / d * d - 3.0;
      }
    }

    #endregion Standard

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
    /// To Statistics
    /// </summary>
    public static async Task<Statistics<T>> Statistics<T>(this ChannelReader<T> reader,
                                                               Func<T, double> selector,
                                                               ChannelParallelOptions options) {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));
      if (selector is null)
        throw new ArgumentNullException(nameof(selector));

      ChannelParallelOptions op = options is null
        ? new ChannelParallelOptions()
        : options.Clone();

      op.CancellationToken.ThrowIfCancellationRequested();

      Statistics<T> result = new Statistics<T>();

      long index = -1;

      await foreach (T item in reader.ReadAllAsync(op.CancellationToken).ConfigureAwait(false))
        result.Add(item, ++index, selector(item));

      return result;
    }

    /// <summary>
    /// To Statistics
    /// </summary>
    public static async Task<Statistics<T>> Statistics<T>(this ChannelReader<T> reader, Func<T, double> selector) =>
      await Statistics(reader, selector, default);

    /// <summary>
    /// To Statistics
    /// </summary>
    public static async Task<Statistics<T>> Statistics<T>(this ChannelReader<T> reader, ChannelParallelOptions options) =>
      await Statistics(reader, item => (double)Convert.ChangeType(item, typeof(double)), options);

    /// <summary>
    /// To Statistics
    /// </summary>
    public static async Task<Statistics<T>> Statistics<T>(this ChannelReader<T> reader) =>
      await Statistics(reader, item => (double)Convert.ChangeType(item, typeof(double)), default);

    #endregion Public
  }

}
