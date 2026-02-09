using System.Windows.Input;

namespace AacV1.VM;

public sealed class HomeViewModel : ObservableObject
{
    public HomeViewModel(INavigationService navigationService)
    {
        GoKanaCommand = new RelayCommand(() => navigationService.NavigateTo<KanaBoardViewModel>());
        GoPhraseCommand = new RelayCommand(() => navigationService.NavigateTo<PhraseViewModel>());
        GoSupporterCommand = new RelayCommand(() => navigationService.NavigateTo<SupporterViewModel>());
    }

    public ICommand GoKanaCommand { get; }
    public ICommand GoPhraseCommand { get; }
    public ICommand GoSupporterCommand { get; }
}
