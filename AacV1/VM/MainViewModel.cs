using System;
using System.Diagnostics;
using AacV1.Core;

namespace AacV1.VM;

public sealed class MainViewModel : ObservableObject
{
    private readonly NavigationService _navigationService;
    private readonly IInputService _inputService;
    private readonly ISpeechService _speechService;
    private ObservableObject _current;

    public MainViewModel()
    {
        _navigationService = new NavigationService(SetCurrent, CreateViewModel);
        _inputService = new InputService();
        _speechService = new SpeechService();
        _inputService.SelectPressed += HandleSelect;
        _inputService.BackPressed += HandleBack;

        _current = CreateViewModel(typeof(HomeViewModel));
        _navigationService.NavigateTo<HomeViewModel>();
    }

    public ObservableObject Current
    {
        get => _current;
        private set => SetProperty(ref _current, value);
    }

    public IInputService InputService => _inputService;

    private void SetCurrent(ObservableObject viewModel)
    {
        Current = viewModel;
    }

    private void HandleSelect()
    {
        if (Current is IInputTarget inputTarget)
        {
            TryInvoke(inputTarget.OnSelect);
        }
    }

    private void HandleBack()
    {
        if (Current is IInputTarget inputTarget)
        {
            TryInvoke(inputTarget.OnBack);
        }

        _navigationService.Back();
    }

    private static void TryInvoke(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }

    private ObservableObject CreateViewModel(Type viewModelType)
    {
        if (viewModelType == typeof(HomeViewModel))
        {
            return new HomeViewModel(_navigationService);
        }

        if (viewModelType == typeof(KanaBoardViewModel))
        {
            return new KanaBoardViewModel(_navigationService, _speechService);
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
