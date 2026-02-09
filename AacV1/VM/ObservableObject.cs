using System.ComponentModel;
using System.Runtime.CompilerServices;
using AacV1.Core;

namespace AacV1.VM;

public abstract class ObservableObject : INotifyPropertyChanged, INavigationAware, IInputTarget
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public virtual void OnEnter()
    {
    }

    public virtual void OnExit()
    {
    }

    public virtual void OnSelect()
    {
    }

    public virtual void OnBack()
    {
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
