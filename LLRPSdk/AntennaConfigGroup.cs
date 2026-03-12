

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// Container class encapsulating all of the antenna configuration objects.
  /// This is for internal use only, and does not appear in the generated
  /// documentation.
  /// </summary>
  [XmlInclude(typeof (AntennaConfig))]
  public class AntennaConfigGroup : IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
  {
    private List<AntennaConfig> antennaConfigs = new List<AntennaConfig>();
    private bool _AntennaCollectionCreatedBySerializer;
    private double _TxPowerInDbm;
    private double _RxSensitivityInDbm;
    private bool _MaxTxPower;
    private bool _MaxRxSensitivity;
    private const string LengthName = "Length";
    private const string IndexerName = "Item[]";

    /// <summary>Default Constructor</summary>
    public AntennaConfigGroup()
    {
      this._AntennaCollectionCreatedBySerializer = true;
      this._MaxTxPower = this._MaxRxSensitivity = false;
    }

    internal AntennaConfigGroup(uint numAntennas)
    {
      this._AntennaCollectionCreatedBySerializer = false;
      for (ushort index = 0; (uint) index < numAntennas; ++index)
        this.antennaConfigs.Add(new AntennaConfig((ushort) ((uint) index + 1U)));
    }

    private void CheckIdListLength(ICollection<ushort> ids)
    {
      if (ids.Count > this.antennaConfigs.Count)
      {
        string[] strArray = new string[5]
        {
          "Number of antenna IDs provided (",
          null,
          null,
          null,
          null
        };
        int count = ids.Count;
        strArray[1] = count.ToString();
        strArray[2] = ") is greater than the number of available antennas (";
        count = this.antennaConfigs.Count;
        strArray[3] = count.ToString();
        strArray[4] = ")";
        throw new LLRPSdkException(string.Concat(strArray));
      }
    }

    /// <summary>
    /// Adds the provided antenna configuration object to the collection.
    /// For internal library use only.
    /// </summary>
    /// <param name="config">The AntennaConfig object to add.</param>
    public void Add(object config)
    {
      if (!this._AntennaCollectionCreatedBySerializer)
        throw new LLRPSdkException("Illegal operation - cannot Add to AntennaConfigGroup object!");
      this.antennaConfigs.Add((AntennaConfig) config);
      this.OnPropertyChanged("Length");
      this.OnPropertyChanged("Item[]");
      this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, config, this.Length));
    }

    /// <summary>Get or set the list of Antenna Configurations.</summary>
    public List<AntennaConfig> AntennaConfigs
    {
      get => this.antennaConfigs;
      set
      {
        List<AntennaConfig> list = this.antennaConfigs.ToList<AntennaConfig>();
        this.SetProperty<List<AntennaConfig>>(ref this.antennaConfigs, value, nameof (AntennaConfigs));
        this.OnPropertyChanged("Length");
        this.OnPropertyChanged("Item[]");
        this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, (IList) this.antennaConfigs, (IList) list));
      }
    }

    /// <summary></summary>
    /// <param name="index" />
    /// <returns />
    [Obsolete("Use foreach iterator or GetAntenna(port) instead of directly accessing the antenna list", false)]
    public AntennaConfig this[int index]
    {
      get => this.antennaConfigs[index];
      set
      {
        AntennaConfig antennaConfig = this.antennaConfigs[index];
        this.antennaConfigs[index] = value;
        this.OnPropertyChanged("Item[]");
        this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, (object) antennaConfig, (object) value, index));
      }
    }

    /// <summary>Enable all antennas in the collection.</summary>
    public void EnableAll()
    {
      foreach (AntennaConfig antennaConfig in this.antennaConfigs)
        antennaConfig.IsEnabled = true;
    }

    /// <summary>Disable all antennas in the collection.</summary>
    public void DisableAll()
    {
      foreach (AntennaConfig antennaConfig in this.antennaConfigs)
        antennaConfig.IsEnabled = false;
    }

    /// <summary>Enable antennas by providing an array of antenna IDs.</summary>
    /// <param name="ids">An array of antenna IDs to enable.</param>
    public void EnableById(ICollection<ushort> ids)
    {
      this.CheckIdListLength(ids);
      foreach (ushort id in (IEnumerable<ushort>) ids)
        this.GetAntenna(id).IsEnabled = true;
    }

    /// <summary>
    /// Disable antennas by providing an array of antenna IDs.
    /// </summary>
    /// <param name="ids">An array of antenna IDs to disable.</param>
    public void DisableById(ICollection<ushort> ids)
    {
      this.CheckIdListLength(ids);
      foreach (ushort id in (IEnumerable<ushort>) ids)
        this.GetAntenna(id).IsEnabled = false;
    }



   



    /// <summary>Sets or gets the transmit power for all antennas</summary>
    public double TxPowerInDbm
    {
      get => this._TxPowerInDbm;
      set
      {
        foreach (AntennaConfig antennaConfig in this.antennaConfigs)
          antennaConfig.TxPowerInDbm = value;
      }
    }

    /// <summary>Sets or gets the receive sensitivity for all antennas</summary>
    public double RxSensitivityInDbm
    {
      get => this._RxSensitivityInDbm;
      set
      {
        foreach (AntennaConfig antennaConfig in this.antennaConfigs)
          antennaConfig.RxSensitivityInDbm = value;
      }
    }

    /// <summary>
    /// Specifies that the maximum antenna transmit power should be used.
    /// </summary>
    public bool TxPowerMax
    {
      get => this._MaxTxPower;
      set
      {
        foreach (AntennaConfig antennaConfig in this.antennaConfigs)
          antennaConfig.MaxTxPower = value;
        if (!value)
          return;
        this._TxPowerInDbm = 0.0;
      }
    }

    /// <summary>
    /// Specifies that the maximum antenna receive sensitivity should be used.
    /// </summary>
    public bool RxSensitivityMax
    {
      get => this._MaxRxSensitivity;
      set
      {
        foreach (AntennaConfig antennaConfig in this.antennaConfigs)
          antennaConfig.MaxRxSensitivity = value;
        if (!value)
          return;
        this._RxSensitivityInDbm = 0.0;
      }
    }

    /// <summary>Returns the settings for the specified antenna port.</summary>
    /// <note>Added virtual to allow for overriding in tests</note>
    /// <returns>
    /// The antenna settings or throws an exception if the port does not exist.
    /// </returns>
    public virtual AntennaConfig GetAntenna(ushort portNumber)
    {
      foreach (AntennaConfig antennaConfig in this.antennaConfigs)
      {
        if ((int) antennaConfig.PortNumber == (int) portNumber)
          return antennaConfig;
      }
      throw new LLRPSdkException("Invalid antenna port number specified.");
    }

    /// <summary>The number of antennas in the collection.</summary>
    public int Length => this.antennaConfigs.Count;

    /// <summary>
    /// Get a handle to the the IEnumerator interface of the antenna
    /// configuration collection.
    /// </summary>
    /// <returns>
    /// An IEnumerator interface object to the antenna configuration
    /// collection.</returns>
    public IEnumerator GetEnumerator() => (IEnumerator) this.antennaConfigs.GetEnumerator();

    /// <summary>Occurs when a collection changes.</summary>
    public event NotifyCollectionChangedEventHandler CollectionChanged;

    /// <summary>Raises the CollectionChanged event.</summary>
    /// <param name="e"></param>
    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
      NotifyCollectionChangedEventHandler collectionChanged = this.CollectionChanged;
      if (collectionChanged == null)
        return;
      collectionChanged((object) this, e);
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
      propertyChanged((object) this, new PropertyChangedEventArgs(propertyName));
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
      if (object.Equals((object) storage, (object) value))
        return false;
      storage = value;
      this.OnPropertyChanged(propertyName);
      return true;
    }
  }
}
