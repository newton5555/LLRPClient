
using Org.LLRP.LTK.LLRPV1;
using System;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Timestamp class supporting Impinj Reader and LLRP timestamp data.
  /// Timestamps from the reader are microseconds since the Epoch
  /// (00:00:00 UTC, January 1, 1970) (16 digit numbers)
  /// </summary>
  public class Timestamp
  {
    /// <summary>Default constructor.</summary>
    public Timestamp()
    {
    }

    /// <summary>
    /// Constructor that translates standard DateTime data into
    /// ImpinjTimeStamp format.
    /// </summary>
    /// <param name="dt">Standard DateTime timestamp.</param>
    public Timestamp(DateTime dt) => this.Utc = (ulong) dt.Ticks / 10UL;

    /// <summary>
    /// Constructor that takes unsigned long (Impinj reader) timestamp data.
    /// </summary>
    /// <param name="utc">
    /// Unsigned long format timestamp; microseconds since the
    /// Epoch (00:00:00 UTC, January 1, 1970) (16 digit number).
    /// </param>
    public Timestamp(ulong utc) => this.Utc = utc;

    /// <summary>Constructor that takes LLRP timestamp data.</summary>
    /// <param name="timestampFromLlrp">LLRP timestamp object</param>
    public Timestamp(UNION_Timestamp timestampFromLlrp)
    {
      this.Utc = ((PARAM_UTCTimestamp) timestampFromLlrp[0]).Microseconds;
    }

    /// <summary>
    /// Conversion utility that outputs the ImpinjTimestamp data into
    /// DateTime format, plus a tick count.
    /// A Tick is 100 nanoseconds, so a microsecond is 10 Ticks.
    /// </summary>
    /// <param name="utc">The tick count to add to the ImpinjTimestamp data.</param>
    /// <returns>
    /// A DateTime object comprised of the current ImpinjTimestamp value and
    /// the supplied tick count.
    /// </returns>
    private DateTime FromUtc(ulong utc)
    {
      return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks((long) utc * 10L);
    }

    /// <summary>Time data member variable.</summary>
    public ulong Utc { get; set; }

    /// <summary>
    /// Utility that outputs the ImpinjTimestamp data in local time format.
    /// </summary>
    public DateTime LocalDateTime => this.FromUtc(this.Utc).ToLocalTime();

    /// <summary>
    /// Conversion utility that outputs the ImpinjTimestamp data as a string.
    /// </summary>
    /// <returns></returns>
    public override string ToString() => this.Utc.ToString();
  }
}
