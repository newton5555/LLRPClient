
using System;
using System.Xml.Serialization;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Enum for defining how often inventory reports are sent from the reader.
  /// </summary>
  [Serializable]
  public enum ReportMode
  {
    /// <summary>
    /// Buffer on the reader and only send the tag report once asked for.
    /// </summary>
    WaitForQuery,
    /// <summary>Generate reports one at a time.</summary>
    Individual,
    /// <summary />
    [Obsolete("Use Individual instead")]
    [XmlIgnore]
    IndividualUnbuffered,
    /// <summary>Buffer on the reader until the reader has stopped.</summary>
    BatchAfterStop,
  }
}
