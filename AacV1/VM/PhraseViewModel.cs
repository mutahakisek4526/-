using System.Windows.Input;

namespace AacV1.VM;

public sealed class PhraseViewModel : ObservableObject
{
    public PhraseViewModel(INavigationService navigationService)
    {
        GoHomeCommand = new RelayCommand(() => navigationService.NavigateTo<HomeViewModel>());
    }

    public ICommand GoHomeCommand { get; }
}
