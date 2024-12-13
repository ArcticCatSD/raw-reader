using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace JLib.Wpf
{
    public class ObservableObject : INotifyPropertyChanged
    {
        protected void SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return;
            }

            field = newValue;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        #region Implement INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion
    }
}
