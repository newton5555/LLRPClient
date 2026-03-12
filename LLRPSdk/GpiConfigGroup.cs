

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
  /// Container class encapsulating all of the GPI configuration objects.
  /// This is for internal use only, and does not appear in the generated
  /// documentation.
  /// </summary>
  [XmlInclude(typeof (GpiConfig))]
  public class GpiConfigGroup : IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
  {
    /// <summary>
    /// The GPIO Port number scheme is not 0 indexed. The first port is "1", which this value represents.
    /// </summary>
    public static readonly int GPI_PORT_START_INDEX = 1;
    private List<GpiConfig> gpiConfigs = new List<GpiConfig>();
    private bool _GPICollectionCreatedBySerializer;
    private const string LengthName = "Length";
    private const string IndexerName = "Item[]";

    internal GpiConfigGroup() => this._GPICollectionCreatedBySerializer = true;

    /// <summary>
    /// Creates a new instance of the GpiConfigGroup class that is
    /// initialized with the specified number of GPI objects.
    /// </summary>
    /// <param name="numGpis">The number of GPI configuration objects to create.</param>
    internal GpiConfigGroup(ushort numGpis)
    {
      this._GPICollectionCreatedBySerializer = false;
      for (int index = 0; index < (int) numGpis; ++index)
        this.gpiConfigs.Add(new GpiConfig());
    }

    /// <summary>
    /// Adds the provided GPI configuration object to the collection.
    /// For internal library use only.
    /// </summary>
    /// <param name="config">The GpiConfig object to add.</param>
    public void Add(object config)
    {
      if (!this._GPICollectionCreatedBySerializer)
        throw new LLRPSdkException("Illegal operation - cannot Add to GPIConfigGroup object!");
      this.gpiConfigs.Add((GpiConfig) config);
      this.OnPropertyChanged("Length");
      this.OnPropertyChanged("Item[]");
      this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, config, this.Length));
    }

    /// <summary>General Purpose Input Configurations</summary>
    public List<GpiConfig> GpiConfigs
    {
      get => this.gpiConfigs;
      set
      {
        List<GpiConfig> list = this.gpiConfigs.ToList<GpiConfig>();
        this.SetProperty<List<GpiConfig>>(ref this.gpiConfigs, value, nameof (GpiConfigs));
        this.OnPropertyChanged("Length");
        this.OnPropertyChanged("Item[]");
        this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, (IList) this.gpiConfigs, (IList) list));
      }
    }

    /// <summary />
    /// <param name="index" />
    /// <returns />
    [Obsolete("You can longer access GPIs by index. Use foreach or GetGpi() instead.", true)]
    public GpiConfig this[int index]
    {
      get => this.gpiConfigs[index];
      set
      {
        GpiConfig gpiConfig = this.gpiConfigs[index];
        this.gpiConfigs[index] = value;
        this.OnPropertyChanged("Item[]");
        this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, (object) gpiConfig, (object) value, index));
      }
    }

    /// <summary>Enable all GPIs in the collection.</summary>
    public void EnableAll()
    {
      foreach (GpiConfig gpiConfig in this.gpiConfigs)
        gpiConfig.IsEnabled = true;
    }

    /// <summary>Disable all GPIs in the collection.</summary>
    public void DisableAll()
    {
      foreach (GpiConfig gpiConfig in this.gpiConfigs)
        gpiConfig.IsEnabled = false;
    }

    /// <summary>The number of GPIs in the collection.</summary>
    public int Length => this.gpiConfigs.Count;

    /// <summary>Returns the settings for the specified GPI port.</summary>
    /// <returns>The GPI settings or throws an exception if the port does not exist.</returns>
    public GpiConfig GetGpi(ushort portNumber)
    {
      foreach (GpiConfig gpiConfig in this.gpiConfigs)
      {
        if ((int) gpiConfig.PortNumber == (int) portNumber)
          return gpiConfig;
      }
      throw new LLRPSdkException("Invalid GPI port number specified.");
    }

    /// <summary>
    /// Get a handle to the IEnumerator interface of the GPI configuration
    /// collection .
    /// </summary>
    /// <returns>
    /// An IEnumerator interface object to the GPI configuration
    /// collection.
    /// </returns>
    public IEnumerator GetEnumerator() => (IEnumerator) this.gpiConfigs.GetEnumerator();

    /// <summary>Occurs when the collection changes.</summary>
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
