

using System;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Enum used to identify the supported regions on llrp readers, which has
  /// an impact on the RF frequencies, transmit power level and dwell times
  /// that the reader is allowed to use.
  /// </summary>
  [Serializable]
  public enum CommunicationsStandardType
  {
    /// <summary>Unspecified regulatory region.</summary>
    Unspecified,
    /// <summary>
    /// Comply with Federal Communications Commission (FCC) Part 15.247 and
    /// 15.249. Required for use in North America.
    /// </summary>
    US_FCC_Part_15,
    /// <summary>
    /// Comply with ETSI EN 302 208 specification. Required for use
    /// in the European Union and United Arab Emirates (UAE).
    /// </summary>
    ETSI_302_208,
    /// <summary>
    /// Comply with ETSI EN 30 2220 specification, replaced in 2004 by the
    /// ETSI EN 302 208 specification. Not supported on Speedway Revolution.
    /// </summary>
    ETSI_300_220,
    /// <summary>
    /// Comply with ACMA’s Low Interference Potential Devices (LIPD) Class
    /// License 2000, with a maximum transmit power of 1 Watt.
    /// </summary>
    Australia_LIPD_1W,
    /// <summary>
    /// Comply with ACMA’s Low Interference Potential Devices (LIPD) Class
    /// License 2000, with a maximum transmit power of 4 Watts.
    /// Required for use in Australia.
    /// </summary>
    Australia_LIPD_4W,
    /// <summary>
    /// Comply with ARIB's STD-T89 specification. Required for use in Japan.
    /// </summary>
    Japan_ARIB_STD_T89,
    /// <summary>
    /// Comply with the Office of the Telecommunications Authority (OFTA)
    /// of Hong Kong, China, standard HKTA 1049.
    /// Required for use in Hong Kong.
    /// </summary>
    Hong_Kong_OFTA_1049,
    /// <summary>
    /// Comply with Taiwan Directorate General of Telecommunications (DGT)
    /// specification LP0002.
    /// Required for use in Taiwan.
    /// </summary>
    Taiwan_DGT_LP0002,
    /// <summary>
    /// Comply with Ministry of Information and Communication (MIC) of
    /// Republic of Korea (South Korea) RFID standard.
    /// Required for use in South Korea.
    /// </summary>
    Korea_MIC_Article_5_2,
  }
}
