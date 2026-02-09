using System;
using System.Collections.Generic;
using System.Diagnostics;
using AacV1.Core;

namespace AacV1.VM;

public sealed class NavigationService : INavigationService
{
    private readonly Action<ObservableObject> _setCurrent;
    private readonly Func<Type, ObservableObject> _factory;
    private readonly Stack<ObservableObject> _history = new();
    private ObservableObject? _current;

    public NavigationService(Action<ObservableObject> setCurrent, Func<Type, ObservableObject> factory)
    {
        _setCurrent = setCurrent ?? throw new ArgumentNullException(nameof(setCurrent));
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public void NavigateTo<TViewModel>() where TViewModel : ObservableObject
    {
        var viewModel = _factory(typeof(TViewModel));
        if (_current is not null && !ReferenceEquals(_current, viewModel))
        {
            _history.Push(_current);
        }

        SwitchTo(viewModel);
    }

    public void Back()
    {
        if (_history.Count > 0)
        {
            var viewModel = _history.Pop();
            SwitchTo(viewModel);
            return;
        }

        var home = _factory(typeof(HomeViewModel));
        SwitchTo(home);
    }

    private void SwitchTo(ObservableObject viewModel)
    {
        try
        {
            if (_current is INavigationAware exiting)
            {
                exiting.OnExit();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }

        _current = viewModel;
        _setCurrent(viewModel);

        try
        {
            if (viewModel is INavigationAware entering)
            {
                entering.OnEnter();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }
}
