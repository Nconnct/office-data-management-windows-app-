using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UniversalOfficeDataManager.ViewModels.Common
{
    /// <summary>
    /// Minimal INotifyPropertyChanged base class. Hand-rolled instead of
    /// pulling in CommunityToolkit.Mvvm (or similar) to keep Stage 1's
    /// dependency surface to just Microsoft.Data.Sqlite.
    /// </summary>
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
