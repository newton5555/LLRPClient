

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

#nullable disable
namespace LLRPSdk
{
  /// <summary>Class used to define inventory tag filter settings.</summary>
  public class FilterSettings : INotifyPropertyChanged
  {
    private TagFilterMode _mode;
    private TagFilter _tagFilter1 = new TagFilter();
    private TagFilter _tagFilter2 = new TagFilter();
    private List<TagSelectFilter> _tagSelectFilters = new List<TagSelectFilter>();
   

    /// <summary>Inventory tag filter mode.</summary>
    public TagFilterMode Mode
    {
      get => this._mode;
      set => this.SetProperty<TagFilterMode>(ref this._mode, value, nameof (Mode));
    }

    /// <summary>Tag filter 1.</summary>
    public TagFilter TagFilter1
    {
      get => this._tagFilter1;
      set => this.SetProperty<TagFilter>(ref this._tagFilter1, value, nameof (TagFilter1));
    }

    /// <summary>Tag filter 2.</summary>
    public TagFilter TagFilter2
    {
      get => this._tagFilter2;
      set => this.SetProperty<TagFilter>(ref this._tagFilter2, value, nameof (TagFilter2));
    }

    /// <summary>Tag select filters.</summary>
    public List<TagSelectFilter> TagSelectFilters
    {
      get => this._tagSelectFilters;
      set
      {
        this.SetProperty<List<TagSelectFilter>>(ref this._tagSelectFilters, value, nameof (TagSelectFilters));
      }
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
