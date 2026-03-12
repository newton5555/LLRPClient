

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
  /// Container class encapsulating all of the GPO configuration objects.
  /// This is for internal use only, and does not appear in the generated
  /// documentation.
  /// </summary>
  [XmlInclude(typeof (GpoConfig))]
  public class GpoConfigGroup : IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
  {
    /// <summary>
    /// The GPIO Port number scheme is not 0 indexed. The first port is "1", which this value represents.
    /// </summary>
    public static readonly int GPO_PORT_START_INDEX = 1;
    private List<GpoConfig> gpoConfigs = new List<GpoConfig>();
    private bool _GPOCollectionCreatedBySerializer;
    private const string LengthName = "Length";
    private const string IndexerName = "Item[]";

    internal GpoConfigGroup() => this._GPOCollectionCreatedBySerializer = true;

    internal GpoConfigGroup(ushort numGpos)
    {
      this._GPOCollectionCreatedBySerializer = false;
      for (int index = 0; index < (int) numGpos; ++index)
        this.gpoConfigs.Add(new GpoConfig());
    }

    /// <summary>
    /// Adds the provided GPO configuration object to the collection.
    /// For internal library use only.
    /// </summary>
    /// <param name="config">The GpOConfig object to add.</param>
    public void Add(object config)
    {
      if (!this._GPOCollectionCreatedBySerializer)
        throw new LLRPSdkException("Illegal operation - cannot Add to GPOConfigGroup object!");
      this.gpoConfigs.Add((GpoConfig) config);
      this.OnPropertyChanged("Length");
      this.OnPropertyChanged("Item[]");
      this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, config, this.Length));
    }

    /// <summary>General Purpose Output Configurations</summary>
    public List<GpoConfig> GpoConfigs
    {
      get => this.gpoConfigs;
      set
      {
        List<GpoConfig> list = this.gpoConfigs.ToList<GpoConfig>();
        this.SetProperty<List<GpoConfig>>(ref this.gpoConfigs, value, nameof (GpoConfigs));
        this.OnPropertyChanged("Length");
        this.OnPropertyChanged("Item[]");
        this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, (IList) this.gpoConfigs, (IList) list));
      }
    }

    /// <summary />
    /// <param name="index" />
    /// <returns />
    [Obsolete("You can no longer access GPOs by index. Use foreach or GetGpo() instead.", true)]
    public GpoConfig this[int index]
    {
      get => this.gpoConfigs[index];
      set
      {
        GpoConfig gpoConfig = this.gpoConfigs[index];
        this.gpoConfigs[index] = value;
        this.OnPropertyChanged("Item[]");
        this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, (object) gpoConfig, (object) value, index));
      }
    }

    /// <summary>The number of GPOs in the collection.</summary>
    public int Length => this.gpoConfigs.Count;

    /// <summary>Returns the settings for the specified GPO port.</summary>
    /// <returns>The GPO settings or throws an exception if the port does not exist.</returns>
    public GpoConfig GetGpo(ushort portNumber)
    {
      foreach (GpoConfig gpoConfig in this.gpoConfigs)
      {
        if ((int) gpoConfig.PortNumber == (int) portNumber)
          return gpoConfig;
      }
      throw new LLRPSdkException("Invalid GPO port number specified.");
    }

    /// <summary>
    /// Get a handle to the IEnumerator interface of the GPO configuration
    /// collection.
    /// </summary>
    /// <returns>
    /// An IEnumerator interface object to the GPO configuration
    /// collection.
    /// </returns>
    public IEnumerator GetEnumerator() => (IEnumerator) this.gpoConfigs.GetEnumerator();

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
