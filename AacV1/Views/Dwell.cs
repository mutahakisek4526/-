using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Data;
using AacV1.Core;

namespace AacV1.Views;

public static class Dwell
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(Dwell),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static readonly DependencyProperty KeyProperty =
        DependencyProperty.RegisterAttached(
            "Key",
            typeof(string),
            typeof(Dwell),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty OnCommittedCommandProperty =
        DependencyProperty.RegisterAttached(
            "OnCommittedCommand",
            typeof(ICommand),
            typeof(Dwell),
            new PropertyMetadata(null));

    private static readonly DependencyProperty SubscriptionProperty =
        DependencyProperty.RegisterAttached(
            "Subscription",
            typeof(Subscription),
            typeof(Dwell),
            new PropertyMetadata(null));

    public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

    public static string GetKey(DependencyObject obj) => (string)obj.GetValue(KeyProperty);
    public static void SetKey(DependencyObject obj, string value) => obj.SetValue(KeyProperty, value);

    public static ICommand? GetOnCommittedCommand(DependencyObject obj) =>
        (ICommand?)obj.GetValue(OnCommittedCommandProperty);

    public static void SetOnCommittedCommand(DependencyObject obj, ICommand? value) =>
        obj.SetValue(OnCommittedCommandProperty, value);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
        {
            return;
        }

        var enabled = (bool)e.NewValue;
        if (enabled)
        {
            element.MouseEnter += HandleMouseEnter;
            element.MouseLeave += HandleMouseLeave;
            element.Loaded += HandleLoaded;
            element.DataContextChanged += HandleDataContextChanged;
            UpdateSubscription(element);
        }
        else
        {
            element.MouseEnter -= HandleMouseEnter;
            element.MouseLeave -= HandleMouseLeave;
            element.Loaded -= HandleLoaded;
            element.DataContextChanged -= HandleDataContextChanged;
            ClearSubscription(element);
        }
    }

    private static void HandleLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            UpdateSubscription(element);
        }
    }

    private static void HandleDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            UpdateSubscription(element);
        }
    }

    private static void HandleMouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is not FrameworkElement element)
        {
            return;
        }

        if (element.DataContext is not IDwellHost host)
        {
            return;
        }

        var key = GetKey(element);
        host.Dwell.PointerEnter(key);
    }

    private static void HandleMouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is not FrameworkElement element)
        {
            return;
        }

        if (element.DataContext is not IDwellHost host)
        {
            return;
        }

        var key = GetKey(element);
        host.Dwell.PointerLeave(key);
    }

    private static void UpdateSubscription(FrameworkElement element)
    {
        ClearSubscription(element);

        if (!GetIsEnabled(element))
        {
            return;
        }

        if (element.DataContext is not IDwellHost host)
        {
            return;
        }

        var subscription = new Subscription(host.Dwell, key =>
        {
            var expectedKey = GetKey(element);
            if (!string.Equals(key, expectedKey, StringComparison.Ordinal))
            {
                return;
            }

            var command = GetOnCommittedCommand(element);
            if (command?.CanExecute(expectedKey) == true)
            {
                command.Execute(expectedKey);
            }
        });

        element.SetValue(SubscriptionProperty, subscription);
    }

    private static void ClearSubscription(FrameworkElement element)
    {
        if (element.GetValue(SubscriptionProperty) is Subscription subscription)
        {
            subscription.Dispose();
            element.ClearValue(SubscriptionProperty);
        }
    }

    private sealed class Subscription : IDisposable
    {
        private readonly IDwellService _service;
        private readonly Action<string> _handler;

        public Subscription(IDwellService service, Action<string> handler)
        {
            _service = service;
            _handler = handler;
            _service.Committed += _handler;
        }

        public void Dispose()
        {
            _service.Committed -= _handler;
        }
    }
}

public sealed class FocusMatchToFontWeightConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2)
        {
            return FontWeights.Normal;
        }

        var focused = values[0] as string;
        var item = values[1] as string;
        if (!string.IsNullOrEmpty(focused) && string.Equals(focused, item, StringComparison.Ordinal))
        {
            return FontWeights.Bold;
        }

        return FontWeights.Normal;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
