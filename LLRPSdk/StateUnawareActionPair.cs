
using System.ComponentModel;
using System.Runtime.CompilerServices;

#nullable disable
namespace LLRPSdk
{
  /// <summary>
  /// This is a class for containing a StateUnawareAction pair with a matching and non-matching action
  /// </summary>
  internal class StateUnawareActionPair : INotifyPropertyChanged
  {
    private StateUnawareAction _matchingAction;
    private StateUnawareAction _nonMatchingAction;

    /// <summary>The action to perform when the tag filter matches.</summary>
    public StateUnawareAction MatchingAction
    {
      get => this._matchingAction;
      set
      {
        this.SetProperty<StateUnawareAction>(ref this._matchingAction, value, nameof (MatchingAction));
      }
    }

    /// <summary>
    /// The action to perform when the tag filter does not match.
    /// </summary>
    public StateUnawareAction NonMatchingAction
    {
      get => this._matchingAction;
      set
      {
        this.SetProperty<StateUnawareAction>(ref this._nonMatchingAction, value, nameof (NonMatchingAction));
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
      if (propertyChanged == null)
        return;
      propertyChanged((object) this, new PropertyChangedEventArgs(propertyName));
    }

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
