using System;

namespace AacV1.VM;

public sealed class NavigationService : INavigationService
{
    private readonly Action<ObservableObject> _setCurrent;
    private readonly Func<Type, ObservableObject> _factory;

    public NavigationService(Action<ObservableObject> setCurrent, Func<Type, ObservableObject> factory)
    {
        _setCurrent = setCurrent ?? throw new ArgumentNullException(nameof(setCurrent));
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public void NavigateTo<TViewModel>() where TViewModel : ObservableObject
    {
        var viewModel = _factory(typeof(TViewModel));
        _setCurrent(viewModel);
    }
}
