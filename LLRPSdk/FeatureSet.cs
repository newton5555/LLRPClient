using Org.LLRP.LTK.LLRPV1;
using Org.LLRP.LTK.LLRPV1.DataType;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

#nullable disable
namespace LLRPSdk
{
    /// <summary>
    /// Container class used to encapsulate the features supported by a given
    /// LLRP reader.  An object of this type is returned by a call to
    /// <see cref="M:LLRPSdk.LLRPReader.QueryFeatureSet" />.
    /// </summary>
    public class FeatureSet : SerializableClass
    {
        /// <summary>
        /// EPC Global model number identifier for the  Reader,
        /// e.g. 2001002 for a Impinj 4 port Speedway Revolution.
        /// </summary>
        public uint ModelNumber { get; set; }

        /// <summary>
        /// Model name of the  Reader, e.g. "Speedway R420" for a
        /// 4 port Speedway Revolution.
        /// </summary>
        public string ReaderModel { get; set; }

     


        public uint DeviceManufacturerNumber { get; set; }


        /// <summary>Firmware version running on reader.</summary>
        public string FirmwareVersion { get; set; }

        /// <summary>Number of antennas supported by reader.</summary>
        public uint AntennaCount { get; set; }

        /// <summary>
        /// Number of general purpose input (GPI) ports supported by reader.
        /// </summary>
        public ushort GpiCount { get; set; }

        /// <summary>
        /// Number of general purpose output (GPO) ports supported by reader.
        /// </summary>
        public ushort GpoCount { get; set; }


        /// <summary>
        /// The maximum number of tag operation sequences supported by the reader.
        /// </summary>
        public uint MaxOperationSequences { get; set; }

        /// <summary>
        /// The maximum number of individual tag operations per sequence
        /// supported by the reader.
        /// </summary>
        public uint MaxOperationsPerSequence { get; set; }

        /// <summary>The Regulatory standard supported by the reader.</summary>
        public CommunicationsStandardType CommunicationsStandard { get; set; }

        public ushort CountryCode { get; set; }

        /// <summary>Always returns true.</summary>
        public bool IsTagAccessAvailable
        {
            get => true;
            set
            {
            }
        }

        /// <summary>Always returns true.</summary>
        public bool IsFilteringAvailable
        {
            get => true;
            set
            {
            }
        }

        /// <summary>
        /// The maximum number of tag select filters allowed per query.
        /// </summary>
        public int MaxTagSelectFiltersAllowed { get; set; }

       

        /// <summary>Indicates whether C1G2 multiword block write is supported.</summary>
        public bool IsMultiwordBlockWriteAvailable { get; set; }

        /// <summary>Indicates whether C1G2 multiword block erase is supported.</summary>
        public bool IsMultiwordBlockEraseAvailable { get; set; }

       






        /// <summary>
        /// Table that correlates transmit powers in dBm to an index in the
        /// reader internal power table.
        /// </summary>
        public List<TxPowerTableEntry> TxPowers { get; set; }

        /// <summary>
        /// Table that correlates a receive sensitivity in dBm to an index
        /// in the reader internal receive sensitivity table.
        /// </summary>
        public List<RxSensitivityTableEntry> RxSensitivities { get; set; }

        /// <summary>
        /// Record the actual lowest sensitivity the reader is capable of being set to
        /// 对应FX9600 基准值不一定是-80/-90 实际上返回的数组长度一直是1，这个值不能调节，对应的真实dbm值也要确定
        /// </summary>
        public double ReaderMaxSensitivityActualDbm { get; set; }

        /// <summary>
        /// Indicates whether frequency hopping supported by the current
        /// CommunicationsStandard.
        /// </summary>
        public bool IsHoppingRegion { get; set; }

        /// <summary>Holds the transmit frequencies the reader can hop on.</summary>
        public List<double> TxFrequencies { get; private set; }



        /// <summary>
        /// A readonly list of RF modes supported by the connected reader.
        /// </summary>
        public List<uint?> RfModes { get; }

        /// <summary>
        /// RF mode detail text keyed by mode id.
        /// Example: 1002 -> "M4/0K Tari: 0 uS, (PIE: 0.0)".
        /// </summary>
        [XmlIgnore]
        public Dictionary<uint, string> RfModeDetails { get; }

        /// <summary>Default Constructor</summary>
        public FeatureSet()
        {
            this.RfModeDetails = new Dictionary<uint, string>();
        }

        internal FeatureSet(FeatureSet featureSet)
        {
            this.AntennaCount = featureSet != null ? featureSet.AntennaCount : throw new LLRPSdkException("Invalid copy constructor parameter.");
            this.FirmwareVersion = featureSet.FirmwareVersion;
            this.DeviceManufacturerNumber = featureSet.DeviceManufacturerNumber;
            this.GpiCount = featureSet.GpiCount;
            this.GpoCount = featureSet.GpoCount;
            this.MaxOperationSequences = featureSet.MaxOperationSequences;
            this.MaxOperationsPerSequence = featureSet.MaxOperationsPerSequence;
            this.ModelNumber = featureSet.ModelNumber;
            this.IsHoppingRegion = featureSet.IsHoppingRegion;
            this.CommunicationsStandard = featureSet.CommunicationsStandard;
            this.CountryCode = featureSet.CountryCode;
            this.MaxTagSelectFiltersAllowed = featureSet.MaxTagSelectFiltersAllowed;
            this.IsMultiwordBlockWriteAvailable = featureSet.IsMultiwordBlockWriteAvailable;
            this.IsMultiwordBlockEraseAvailable = featureSet.IsMultiwordBlockEraseAvailable;
            this.ReaderModel = featureSet.ReaderModel;
            List<TxPowerTableEntry> txPowers = featureSet.TxPowers;
            this.TxPowers = txPowers != null ? txPowers.ToList<TxPowerTableEntry>() : (List<TxPowerTableEntry>)null;
            List<RxSensitivityTableEntry> rxSensitivities = featureSet.RxSensitivities;
            this.RxSensitivities = rxSensitivities != null ? rxSensitivities.ToList<RxSensitivityTableEntry>() : (List<RxSensitivityTableEntry>)null;
            this.ReaderMaxSensitivityActualDbm = featureSet.ReaderMaxSensitivityActualDbm;
            List<double> txFrequencies = featureSet.TxFrequencies;
            this.TxFrequencies = txFrequencies != null ? txFrequencies.ToList<double>() : (List<double>)null;
            List<uint?> rfModes = featureSet.RfModes;
            this.RfModes = rfModes != null ? rfModes.ToList<uint?>() : (List<uint?>)null;
            this.RfModeDetails = featureSet.RfModeDetails != null
                ? featureSet.RfModeDetails.ToDictionary(x => x.Key, x => x.Value)
                : new Dictionary<uint, string>();
        }

        internal FeatureSet(MSG_GET_READER_CAPABILITIES_RESPONSE capabilities)
        {
            this.AntennaCount = (uint)capabilities.GeneralDeviceCapabilities.MaxNumberOfAntennaSupported;
            this.FirmwareVersion = capabilities.GeneralDeviceCapabilities.ReaderFirmwareVersion;
            this.GpiCount = capabilities.GeneralDeviceCapabilities.GPIOCapabilities.NumGPIs;
            this.GpoCount = capabilities.GeneralDeviceCapabilities.GPIOCapabilities.NumGPOs;
            this.MaxOperationSequences = capabilities.LLRPCapabilities.MaxNumAccessSpecs;
            this.MaxOperationsPerSequence = capabilities.LLRPCapabilities.MaxNumOpSpecsPerAccessSpec;
            this.ModelNumber = capabilities.GeneralDeviceCapabilities.ModelName;
            this.DeviceManufacturerNumber = capabilities.GeneralDeviceCapabilities.DeviceManufacturerName;
            this.ReaderModel = "Unknown";
            if (this.DeviceManufacturerNumber == (uint)ManufacturerNumber.Impinj)
            {
                this.ReaderModel = ((ReaderModel_Impinj)this.ModelNumber).ToString();
            }
            else if (this.DeviceManufacturerNumber == (uint)ManufacturerNumber.Zebra
                || this.DeviceManufacturerNumber == (uint)ManufacturerNumber.Motorola)
            {
                this.ReaderModel = ((ReaderModel_Zebra)this.ModelNumber).ToString();
            }
            if (this.DeviceManufacturerNumber == (uint)ManufacturerNumber.Seuic)
            {
                this.ReaderModel = ((ReaderModel_Seuic)this.ModelNumber).ToString();
            }




            this.IsHoppingRegion = capabilities.RegulatoryCapabilities.UHFBandCapabilities.FrequencyInformation.Hopping;
            this.CommunicationsStandard = (CommunicationsStandardType)capabilities.RegulatoryCapabilities.CommunicationsStandard;
            this.CountryCode = capabilities.RegulatoryCapabilities.CountryCode;
            this.IsMultiwordBlockWriteAvailable = false;
            this.IsMultiwordBlockEraseAvailable = false;
            for (int index = 0; index < capabilities.AirProtocolLLRPCapabilities.Count; ++index)
            {
                if (capabilities.AirProtocolLLRPCapabilities[index] is PARAM_C1G2LLRPCapabilities)
                {
                    var c1g2 = capabilities.AirProtocolLLRPCapabilities[index] as PARAM_C1G2LLRPCapabilities;
                    this.MaxTagSelectFiltersAllowed = (int)c1g2.MaxNumSelectFiltersPerQuery;
                    this.IsMultiwordBlockWriteAvailable = c1g2.CanSupportBlockWrite;
                    this.IsMultiwordBlockEraseAvailable = c1g2.CanSupportBlockErase;
                }
            }

            this.BuildRxTxTables(capabilities);
            this.BuildFreqHopTable(capabilities);
            this.RfModes = new List<uint?>();
            this.RfModeDetails = new Dictionary<uint, string>();
            UNION_AirProtocolUHFRFModeTable protocolUhfrfModeTable = capabilities.RegulatoryCapabilities.UHFBandCapabilities.AirProtocolUHFRFModeTable;
            for (int index = 0; index < protocolUhfrfModeTable.Length; ++index)
            {
                if (protocolUhfrfModeTable[index] is PARAM_C1G2UHFRFModeTable g2UhfrfModeTable)
                {
                    foreach (PARAM_C1G2UHFRFModeTableEntry uhfrfModeTableEntry in g2UhfrfModeTable.C1G2UHFRFModeTableEntry)
                    {
                        var modeId = uhfrfModeTableEntry.ModeIdentifier;
                        this.RfModeDetails[modeId] = BuildRfModeDetail(uhfrfModeTableEntry);

                        this.RfModes.Add(new uint?(modeId));
                    }
                }
            }




           if (this.DeviceManufacturerNumber == (uint)ManufacturerNumber.Motorola)
            {
                this.RfModes[0] = 0;
                this.RfModeDetails.Add(0, "Auto");
            }

        }

        private static string BuildRfModeDetail(PARAM_C1G2UHFRFModeTableEntry entry)
        {
            string m = entry.MValue switch
            {
                ENUM_C1G2MValue.MV_FM0 => "FM0",
                ENUM_C1G2MValue.MV_2 => "M2",
                ENUM_C1G2MValue.MV_4 => "M4",
                ENUM_C1G2MValue.MV_8 => "M8",
                _ => entry.MValue.ToString()
            };

            string bdr = entry.BDRValue > 0 ? $"{entry.BDRValue / 1000.0:0.#}K" : "0K";
            var tariUs = entry.MinTariValue / 1000.0;
            var pieUs = entry.PIEValue / 1000.0;

            return $"{m}/{bdr} Tari: {tariUs:0.###} uS, (PIE: {pieUs:0.###})";
        }

        private void BuildFreqHopTable(MSG_GET_READER_CAPABILITIES_RESPONSE capabilities)
        {
            this.TxFrequencies = new List<double>();
            UInt32Array uint32Array = !this.IsHoppingRegion ? capabilities.RegulatoryCapabilities.UHFBandCapabilities.FrequencyInformation.FixedFrequencyTable.Frequency : capabilities.RegulatoryCapabilities.UHFBandCapabilities.FrequencyInformation.FrequencyHopTable[0].Frequency;
            this.TxFrequencies.Clear();
            for (int index = 0; index < uint32Array.Count; ++index)
                this.TxFrequencies.Add((double)uint32Array[index] / 1000.0);
        }

        private void BuildRxTxTables(MSG_GET_READER_CAPABILITIES_RESPONSE capabilities)
        {
            this.TxPowers = new List<TxPowerTableEntry>();
            this.RxSensitivities = new List<RxSensitivityTableEntry>();
            this.RxSensitivities.Clear();
            PARAM_ReceiveSensitivityTableEntry[] sensitivityTableEntry1 = capabilities.GeneralDeviceCapabilities.ReceiveSensitivityTableEntry;
            double num;
            this.ReaderMaxSensitivityActualDbm = this.ReaderModel == ReaderModel_Impinj.R700.ToString() 
                || this.ReaderModel == ((ReaderModel_Impinj)2001060).ToString()
                || this.ReaderModel == ((ReaderModel_Impinj)2001063).ToString()
                || this.ReaderModel == ((ReaderModel_Impinj)2001053).ToString()
                || this.ReaderModel == (ReaderModel_Impinj.R720.ToString()) 
                || this.ReaderModel == ((ReaderModel_Impinj)2001061).ToString() ? (num = -90.0) : (num = -80.0);
            foreach (PARAM_ReceiveSensitivityTableEntry sensitivityTableEntry2 in sensitivityTableEntry1)
            {
                RxSensitivityTableEntry sensitivityTableEntry3;
                sensitivityTableEntry3.Dbm = num + (double)sensitivityTableEntry2.ReceiveSensitivityValue;
                sensitivityTableEntry3.Index = sensitivityTableEntry2.Index;
                this.RxSensitivities.Add(sensitivityTableEntry3);
            }
            this.TxPowers.Clear();
            foreach (PARAM_TransmitPowerLevelTableEntry powerLevelTableEntry in capabilities.RegulatoryCapabilities.UHFBandCapabilities.TransmitPowerLevelTableEntry)
            {
                TxPowerTableEntry txPowerTableEntry;
                txPowerTableEntry.Dbm = (double)powerLevelTableEntry.TransmitPowerValue / 100.0;
                txPowerTableEntry.Index = powerLevelTableEntry.Index;
                this.TxPowers.Add(txPowerTableEntry);
            }
        }

        /// <summary>
        /// Method for importing reader feature set data from an XML string.
        /// </summary>
        /// <param name="xml">XML string to parse.</param>
        /// <returns>Populated FeatureSet object.</returns>
        public static FeatureSet FromXmlString(string xml)
        {
            return (FeatureSet)new XmlSerializer(typeof(FeatureSet)).Deserialize((TextReader)new StringReader(xml));
        }

        /// <summary>
        /// Read the XML representation of FeatureSet from a file and return a
        /// new FeatureSet instance with those values.
        /// </summary>
        /// <param name="path">The path and filename of the XML file to load.</param>
        /// <returns>Populated instance of FeatureSet class based on file contents.</returns>
        public static FeatureSet Load(string path)
        {
            return FeatureSet.FromXmlString(File.ReadAllText(path, Encoding.Default));
        }
    }
}
