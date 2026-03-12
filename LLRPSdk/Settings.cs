
using Org.LLRP.LTK.LLRPV1;
using Org.LLRP.LTK.LLRPV1.DataType;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Serialization;

#nullable disable
namespace LLRPSdk
{
    /// <summary>
    /// Class for containing all the settings necessary for a reader to begin
    /// singulating. It is a composite class consisting of other composite
    /// classes containing individual settings, and is consumed by the
    /// following methods:
    /// <see cref="M:LLRPSdk.LLRPReader.ApplySettings(LLRPSdk.Settings)" />,
    /// <see cref="M:LLRPSdk.LLRPReader.ApplySettingsWithoutFactoryReset(LLRPSdk.Settings)" />
    /// and produced by the following methods:
    /// <see cref="M:LLRPSdk.LLRPReader.QuerySettings" />,
    /// <see cref="M:LLRPSdk.LLRPReader.QueryDefaultSettings" />,
    /// </summary>
    public class Settings : SerializableClass, INotifyPropertyChanged
    {
        private AutoStartConfig _autoStart;
        private AutoStopConfig _autoStop;
        private uint? _rfMode;

        private ushort _session;
        private ushort _tagPopulationEstimate;
        private FilterSettings _filters;

        private ReportConfig _report;
        private AntennaConfigGroup _antennas;
        private GpiConfigGroup _gpis;
        private GpoConfigGroup _gpos;//gpo 的配置参数，不是实际的gpo状态控制,有单独的Set方法控制gpo状态
        private KeepaliveConfig _keepalives;
        private bool _holdReportsOnDisconnect;
        private List<double> _txFrequenciesInMhz;
        private List<double> _reducedPowerFrequenciesInMhz;

        private bool _startOfAntennaEvent;
        private bool _endOfCycleEvent;
        private AttachedDataConfig _attachedData;



        /// <summary>
        /// The conditions in which an LLRP reader will automatically start
        /// operation.
        /// </summary>
        public AutoStartConfig AutoStart
        {
            get => this._autoStart;
            set => this.SetProperty<AutoStartConfig>(ref this._autoStart, value, nameof(AutoStart));
        }

        /// <summary>
        /// The conditions in which an LLRP reader will automatically stop
        /// operation.
        /// </summary>
        public AutoStopConfig AutoStop
        {
            get => this._autoStop;
            set => this.SetProperty<AutoStopConfig>(ref this._autoStop, value, nameof(AutoStop));
        }



        /// <summary>The selected common RF mode for this configuration.</summary>
        public uint? RfMode
        {
            get => this._rfMode;
            set => this.SetProperty<uint?>(ref this._rfMode, value, nameof(RfMode));
        }



        /// <summary>
        /// Session number (0 - 3) to use for the inventory operation for this
        /// configuration.
        /// </summary>
        public ushort Session
        {
            get => this._session;
            set => this.SetProperty<ushort>(ref this._session, value, nameof(Session));
        }

        /// <summary>
        /// An estimate of the tag population in view of the RF field of the antenna.
        /// </summary>
        public ushort TagPopulationEstimate
        {
            get => this._tagPopulationEstimate;
            set
            {
                this.SetProperty<ushort>(ref this._tagPopulationEstimate, value, nameof(TagPopulationEstimate));
            }
        }



        /// <summary>
        /// The settings for defining any tag filters that the reader must use to
        /// select a portion of the tag population to participate in singulation.
        /// </summary>
        public FilterSettings Filters
        {
            get => this._filters;
            set => this.SetProperty<FilterSettings>(ref this._filters, value, nameof(Filters));
        }



        /// <summary>
        /// Set how tags are reported and select optional report fields.
        /// </summary>
        public ReportConfig Report
        {
            get => this._report;
            set => this.SetProperty<ReportConfig>(ref this._report, value, nameof(Report));
        }

        /// <summary>
        /// Per-antenna settings: power, sensitivity, etc. may be iterated with
        /// foreach or subscripted by antenna port number.
        /// </summary>
        [XmlArray("Antennas")]
        [XmlArrayItem("Antenna", typeof(AntennaConfig))]
        public AntennaConfigGroup Antennas
        {
            get => this._antennas;
            set => this.SetProperty<AntennaConfigGroup>(ref this._antennas, value, nameof(Antennas));
        }

        /// <summary>
        /// Enable general purpose input (GPI) events on specific GPI ports.
        /// May be iterated with foreach or subscripted by GPI port number.
        /// </summary>
        [XmlArray("Gpis")]
        [XmlArrayItem("Gpi", typeof(GpiConfig))]
        public GpiConfigGroup Gpis
        {
            get => this._gpis;
            set => this.SetProperty<GpiConfigGroup>(ref this._gpis, value, nameof(Gpis));
        }

        /// <summary>
        /// Enable general purpose output (GPO) events on specific GPO ports.
        /// May be iterated with foreach or subscripted by GPO port number.
        /// </summary>
        [XmlArray("Gpos")]
        [XmlArrayItem("Gpo", typeof(GpoConfig))]
        public GpoConfigGroup Gpos
        {
            get => this._gpos;
            set => this.SetProperty<GpoConfigGroup>(ref this._gpos, value, nameof(Gpos));
        }

        /// <summary>
        /// Optionally cause the reader to send a keep-alive message periodically.
        /// </summary>
        public KeepaliveConfig Keepalives
        {
            get => this._keepalives;
            set => this.SetProperty<KeepaliveConfig>(ref this._keepalives, value, nameof(Keepalives));
        }

        /// <summary>
        /// Indicates whether the reader has been configured to hold reports
        /// and events; i.e. the reader events and reports parameter
        /// HoldEventsAndReportsUponReconnect is set to true.
        /// </summary>
        public bool HoldReportsOnDisconnect
        {
            get => this._holdReportsOnDisconnect;
            set
            {
                this.SetProperty<bool>(ref this._holdReportsOnDisconnect, value, nameof(HoldReportsOnDisconnect));
            }
        }

        /// <summary>
        /// Transmit frequencies to use in regions that allow it.
        /// An empty list means the reader chooses.
        /// </summary>
        public List<double> TxFrequenciesInMhz
        {
            get => this._txFrequenciesInMhz;
            set
            {
                this.SetProperty<List<double>>(ref this._txFrequenciesInMhz, value, nameof(TxFrequenciesInMhz));
            }
        }

        /// <summary>Reduced power frequencies</summary>
        public List<double> ReducedPowerFrequenciesInMhz
        {
            get => this._reducedPowerFrequenciesInMhz;
            set
            {
                this.SetProperty<List<double>>(ref this._reducedPowerFrequenciesInMhz, value, nameof(ReducedPowerFrequenciesInMhz));
            }
        }





        /// <summary>Enable or disable start/attempt of antenna event</summary>
        public bool StartOfAntennaEvent
        {
            get => this._startOfAntennaEvent;
            set
            {
                this.SetProperty<bool>(ref this._startOfAntennaEvent, value, nameof(StartOfAntennaEvent));
            }
        }

        /// <summary>Enable or disable End Of Cycle/End_Of_AISpec</summary>
        public bool EndOfCycleEvent
        {
            get => this._endOfCycleEvent;
            set => this.SetProperty<bool>(ref this._endOfCycleEvent, value, nameof(EndOfCycleEvent));
        }

        /// <summary>
        /// Configuration for automatic attached-data access operation during inventory.
        /// </summary>
        public AttachedDataConfig AttachedData
        {
            get => this._attachedData;
            set => this.SetProperty<AttachedDataConfig>(ref this._attachedData, value, nameof(AttachedData));
        }





        /// <summary>Default Constructor</summary>
        public Settings() => this.InitVars();

        internal Settings(
          uint numberOfAntennas,
          ushort numberOfGpis,
          ushort numberOfGpos,
          string firmwareVersion)
        {
            this.InitVars();
            this.SetConfigDefaults(numberOfAntennas, numberOfGpis, numberOfGpos, firmwareVersion);
        }

        internal void LoadFilterData(PARAM_C1G2Filter[] filters)
        {
            if (filters != null)
            {
                this.Filters.TagFilter1.MemoryBank = (MemoryBank)filters[0].C1G2TagInventoryMask.MB.ToInt();
                this.Filters.TagFilter1.BitPointer = filters[0].C1G2TagInventoryMask.Pointer;
                this.Filters.TagFilter1.TagMask = filters[0].C1G2TagInventoryMask.TagMask.ToHexString();
                if (filters.Length == 1)
                    this.Filters.Mode = TagFilterMode.OnlyFilter1;
                else if (filters.Length == 2)
                {
                    this.Filters.TagFilter2.MemoryBank = (MemoryBank)filters[1].C1G2TagInventoryMask.MB.ToInt();
                    this.Filters.TagFilter2.BitPointer = filters[1].C1G2TagInventoryMask.Pointer;
                    this.Filters.TagFilter2.TagMask = filters[1].C1G2TagInventoryMask.TagMask.ToHexString();
                    this.Filters.Mode = filters[1].C1G2TagInventoryStateUnawareFilterAction.Action != ENUM_C1G2StateUnawareAction.DoNothing_Unselect ? TagFilterMode.Filter1OrFilter2 : TagFilterMode.Filter1AndFilter2;
                }
                else if (filters.Length > 2)
                    this.Filters.Mode = TagFilterMode.UseTagSelectFilters;
                List<TagSelectFilter> tagSelectFilters = this.Filters.TagSelectFilters;
                tagSelectFilters.Clear();
                foreach (PARAM_C1G2Filter filter in filters)
                {
                    TagSelectFilter tagSelectFilter = new TagSelectFilter()
                    {
                        TagMask = filter.C1G2TagInventoryMask.TagMask.ToHexString(),
                        BitPointer = filter.C1G2TagInventoryMask.Pointer,
                        MemoryBank = (MemoryBank)filter.C1G2TagInventoryMask.MB.ToInt()
                    };
                    StateUnawareActionPair unawareActionPair = StateUnawareActionExtensions.ConvertFromC1G2StateUnawareAction(filter.C1G2TagInventoryStateUnawareFilterAction.Action);
                    tagSelectFilter.MatchAction = unawareActionPair.MatchingAction;
                    tagSelectFilter.NonMatchAction = unawareActionPair.NonMatchingAction;
                    tagSelectFilters.Add(tagSelectFilter);
                }
            }
            else
                this.Filters.Mode = TagFilterMode.None;
        }

        private void InitVars()
        {
            this.AutoStart = new AutoStartConfig();
            this.AutoStop = new AutoStopConfig();
            this.Keepalives = new KeepaliveConfig();
            this.Report = new ReportConfig();
            this.Filters = new FilterSettings();
            this.AttachedData = new AttachedDataConfig();
            this.TxFrequenciesInMhz = new List<double>();
            this.ReducedPowerFrequenciesInMhz = new List<double>();

        }

        private void SetConfigDefaults(
          uint numberOfAntennas,
          ushort numberOfGpis,
          ushort numberOfGpos,
          string firmwareVersion)
        {
            this.Antennas = new AntennaConfigGroup(numberOfAntennas);
            foreach (AntennaConfig antenna in this.Antennas)
            {
                antenna.IsEnabled = true;
                antenna.MaxRxSensitivity = true;
                antenna.MaxTxPower = true;
            }
            this.Gpis = new GpiConfigGroup(numberOfGpis);
            int num1 = 1;//Todo   Gpi and Gpo port numbers are 1 indexed !!! 但是不同的reader型号会有差异
            foreach (GpiConfig gpi in this.Gpis)
            {
                gpi.PortNumber = (ushort)num1;
                gpi.IsEnabled = false;
                ++num1;
            }
            this.Gpos = new GpoConfigGroup(numberOfGpos);
            int num2 = 1;
            foreach (GpoConfig gpo in this.Gpos)
            {
                gpo.GpoPulseDurationMsec = 0U;
                gpo.Mode = GpoMode.Normal;
                gpo.PortNumber = (ushort)num2;
                ++num2;
            }
            this.AutoStart.Mode = AutoStartMode.None;
            this.AutoStop.Mode = AutoStopMode.None;


            this.Session = (ushort)2;
            this.TagPopulationEstimate = (ushort)32;
            this.Report.IncludeAntennaPortNumber = false;
            this.Report.IncludeChannel = false;
            this.Report.IncludeFirstSeenTime = false;
            this.Report.IncludeLastSeenTime = false;

            this.Report.IncludeSeenCount = false;
            this.Report.IncludePeakRssi = false;

            this.Report.IncludePcBits = false;
            this.Report.IncludeCrc = false;
            this.Report.Mode = ReportMode.Individual;
            this.Keepalives.Enabled = false;

            this.AutoStart.Mode = AutoStartMode.None;
            this.Filters.Mode = TagFilterMode.None;
            this.Filters.TagFilter1.BitCount = 0;
            this.Filters.TagFilter2.BitCount = 0;
            this.TxFrequenciesInMhz.Clear();
            this.ReducedPowerFrequenciesInMhz.Clear();
            this.HoldReportsOnDisconnect = false;

        }

        /// <summary>
        /// Method for importing reader configuration settings from an XML string.
        /// </summary>
        /// <param name="xml">XML string to parse.</param>
        /// <returns>Populated Settings object.</returns>
        public static Settings FromXmlString(string xml)
        {
            return (Settings)new XmlSerializer(typeof(Settings)).Deserialize((TextReader)new StringReader(xml));
        }

        /// <summary>
        /// Read the XML representation of settings from a file and return a
        /// new Settings instance with those values.
        /// </summary>
        /// <param name="path">The path and filename of the XML file to load.</param>
        /// <returns>Populated instance of Settings class based on file contents.</returns>
        public static Settings Load(string path)
        {
            return Settings.FromXmlString(File.ReadAllText(path, Encoding.Default));
        }








        /// <summary>Occurs when a property value changes.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Raises the PropertyChanged event.</summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if (propertyChanged == null)
                return;
            propertyChanged((object)this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Sets a property, raising the PropertyChanged event if the value of the property changes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storage"></param>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals((object)storage, (object)value))
                return false;
            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }
    }
}
