using System.Windows.Input;

namespace AacV1.VM;

public sealed class SupporterViewModel : ObservableObject
{
    public SupporterViewModel(INavigationService navigationService)
    {
        GoHomeCommand = new RelayCommand(() => navigationService.NavigateTo<HomeViewModel>());
    }

    public ICommand GoHomeCommand { get; }
}
