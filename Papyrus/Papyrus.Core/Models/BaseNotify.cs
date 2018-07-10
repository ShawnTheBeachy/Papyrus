using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Papyrus
{
    public class BaseNotify : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public bool Set<T>(ref T storage, T value, [CallerMemberName()]string propertyName = null)
        {
            if (!Equals(storage, value))
            {
                storage = value;
                RaisePropertyChanged(propertyName);
                return true;
            }

            return false;
        }
    }
}
