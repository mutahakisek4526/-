using System.Windows.Input;

namespace AacV1.VM;

public sealed class KanaBoardViewModel : ObservableObject
{
    public KanaBoardViewModel(INavigationService navigationService)
    {
        GoHomeCommand = new RelayCommand(() => navigationService.NavigateTo<HomeViewModel>());
    }

    public ICommand GoHomeCommand { get; }
}
