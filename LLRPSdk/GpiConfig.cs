using System.ComponentModel;
using System.Runtime.CompilerServices;

#nullable disable
namespace LLRPSdk
{
    /// <summary>
    /// Class that is used to define the configuration settings for an
    /// individual general purpose input (GPI) port.
    /// </summary>
    public class GpiConfig : INotifyPropertyChanged
    {
        private ushort _PortNumber;
        private bool _isEnabled;

        /// <summary>Specifies whether this port should be enabled.</summary>
        public bool IsEnabled
        {
            get => this._isEnabled;
            set => this.SetProperty<bool>(ref this._isEnabled, value, nameof(IsEnabled));
        }

        /// <summary>
        /// Specifies the GPI port number (starting at port 1) to configure.
        /// </summary>
        /// <exception cref="T:LLRPSdk.LLRPSdkException">
        /// Thrown when PortNumber is set to 0;
        /// </exception>
        public ushort PortNumber
        {
            get => this._PortNumber;
            set
            {
                if (value == (ushort)0)
                    throw new LLRPSdkException("GPI port number 0 is illegal.");
                this.SetProperty<ushort>(ref this._PortNumber, value, nameof(PortNumber));
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
