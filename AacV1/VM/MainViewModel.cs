using System;

namespace AacV1.VM;

public sealed class MainViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private ObservableObject _current;

    public MainViewModel()
    {
        _navigationService = new NavigationService(SetCurrent, CreateViewModel);
        _current = CreateViewModel(typeof(HomeViewModel));
    }

    public ObservableObject Current
    {
        get => _current;
        private set => SetProperty(ref _current, value);
    }

    private void SetCurrent(ObservableObject viewModel)
    {
        Current = viewModel;
    }

    private ObservableObject CreateViewModel(Type viewModelType)
    {
        if (viewModelType == typeof(HomeViewModel))
        {
            return new HomeViewModel(_navigationService);
        }

        if (viewModelType == typeof(KanaBoardViewModel))
        {
            return new KanaBoardViewModel(_navigationService);
        }

        if (viewModelType == typeof(PhraseViewModel))
        {
            return new PhraseViewModel(_navigationService);
        }

        if (viewModelType == typeof(SupporterViewModel))
        {
            return new SupporterViewModel(_navigationService);
        }

        throw new InvalidOperationException($"Unknown ViewModel type: {viewModelType}");
    }
}
