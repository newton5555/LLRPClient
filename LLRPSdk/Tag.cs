
using System;
using System.Collections.Generic;

#nullable disable
namespace LLRPSdk
{
  /// <summary>Class used to contain the details for a specific tag.</summary>
  public class Tag
  {
    /// <summary>Contents of the tag EPC memory bank.</summary>
    public TagData Epc { get; set; }

    /// <summary>
    /// The reader antenna port number for the antenna that last saw the tag;
    /// requires this option to be enabled in the reader settings report configuration.
    /// </summary>
    public ushort AntennaPortNumber { get; set; }

    /// <summary>
    /// The Reader channel, defined in Megahertz, that was being used when
    /// the tag was last seen; requires this option to be enabled in the
    /// reader settings report configuration.
    /// </summary>
    public double ChannelInMhz { get; set; }

    /// <summary>
    /// The time that the reader first saw the tag; requires this
    /// option to be enabled in the reader settings report configuration.
    /// </summary>
    public Timestamp FirstSeenTime { get; set; }

    /// <summary>
    /// The time that the reader last saw the tag; requires this
    /// option to be enabled in the reader settings report configuration.
    /// </summary>
    public Timestamp LastSeenTime { get; set; }

    /// <summary>
    /// The maximum RSSI, that was seen for this tag (standard LLRP parameter);
    /// requires this option to be enabled in the reader settings report configuration.
    /// </summary>
    public double PeakRssi { get; set; }



    /// <summary>
    /// The number of times the reader has seen this tag; requires this
    /// option to be enabled in the reader settings report configuration.
    /// </summary>
    public ushort TagSeenCount { get; set; }





    /// <summary>
    /// Contents of the CRC 16-bit word (word 0) in the tag EPC memory bank; requires this
    /// option to be enabled in the reader settings report configuration.
    /// </summary>
    public ushort Crc { get; set; }

    /// <summary>
    /// Contents of the PC Bits 16-bit word (word 1) in the tag EPC memory
    /// bank; requires this option to be enabled in the reader settings
    /// report configuration.
    /// </summary>
    public ushort PcBits { get; set; }

    /// <summary>
    /// The value of the TxPower; requires this option to be enabled in
    /// the reader settings report configuration.
    /// </summary>
    public ushort TxPower { get; set; }



    /// <summary>Results of an Optimized Read operation.</summary>
    public List<TagReadOpResult> ReadOperationResults { get; set; }





   

   

    /// <summary>Does the tag data include antenna port number data?</summary>
    public bool IsAntennaPortNumberPresent { get; set; }

    /// <summary>Does the tag data include channel frequency data?</summary>
    public bool IsChannelInMhzPresent { get; set; }

    /// <summary>
    /// Does the tag data include the first seen timestamp data?
    /// </summary>
    public bool IsFirstSeenTimePresent { get; set; }

    /// <summary>
    /// Does the tag data include the last seen timestamp data?
    /// </summary>
    public bool IsLastSeenTimePresent { get; set; }


    /// <summary>Does the tag data include the peak RSSI data (standard parameter)?</summary>
    public bool IsPeakRssiPresent { get; set; }


    /// <summary>
    /// Does the tag data include data on the number of times the
    /// tag has been seen?
    /// </summary>
    public bool IsSeenCountPresent { get; set; }

    /// <summary>Does the tag data include the EPC CRC data?</summary>
    public bool IsCrcPresent { get; set; }

    /// <summary>Does the tag data include the EPC PC Bits data?</summary>
    public bool IsPcBitsPresent { get; set; }


    /// <summary>Does the tag data include the Tx Power?</summary>
    public bool IsTxPowerPresent { get; set; }


    /// <summary>
    /// Does the tag report include an Endpoint IC Verification report?
    /// </summary>
    public bool IsEndpointICVerificationReportPresent { get; set; }

   

    internal Tag()
    {
      this.Epc = new TagData();
      this.FirstSeenTime = new Timestamp();
      this.LastSeenTime = new Timestamp();
     

      this.ReadOperationResults = new List<TagReadOpResult>();
     
      this.IsAntennaPortNumberPresent = false;
      this.IsChannelInMhzPresent = false;
      this.IsFirstSeenTimePresent = false;
      this.IsLastSeenTimePresent = false;
   
      this.IsPeakRssiPresent = false;
  
      this.IsSeenCountPresent = false;
      this.IsCrcPresent = false;
      this.IsPcBitsPresent = false;
 
      this.IsTxPowerPresent = false;
      this.IsEndpointICVerificationReportPresent = false;
    }
  }
}
