using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using AacV1.Core;

namespace AacV1.VM;

public sealed class KanaBoardViewModel : ObservableObject, IDwellHost
{
    private readonly ISpeechService _speechService;
    private readonly IDwellService _dwellService;
    private readonly IUndoService _undoService;
    private string _text = string.Empty;
    private string? _focusedItem;

    public KanaBoardViewModel(
        INavigationService navigationService,
        ISpeechService speechService,
        IDwellService dwellService,
        IUndoService undoService)
    {
        _speechService = speechService;
        _dwellService = dwellService;
        _undoService = undoService;
        GoHomeCommand = new RelayCommand(() => navigationService.NavigateTo<HomeViewModel>());
        SpeakCommand = new RelayCommand(() => _ = SpeakAsync());
        StopCommand = new RelayCommand(() => _speechService.Stop());
        CommitItemCommand = new CommitCommand(CommitItem);
        UndoCommand = new RelayCommand(() => _undoService.Undo());
        Items = new ObservableCollection<string>
        {
            "あ", "い", "う", "え", "お",
            "、", "。", "消す", "空白", "読み上げ"
        };

        _dwellService.Focused += HandleFocused;
    }

    public ICommand GoHomeCommand { get; }
    public ICommand SpeakCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand CommitItemCommand { get; }
    public ICommand UndoCommand { get; }

    public ObservableCollection<string> Items { get; }

    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }

    public string? FocusedItem
    {
        get => _focusedItem;
        private set => SetProperty(ref _focusedItem, value);
    }

    public IDwellService Dwell => _dwellService;

    public override void OnSelect()
    {
        _ = SpeakAsync();
    }

    public override void OnBack()
    {
        _speechService.Stop();
    }

    public override void OnExit()
    {
        _speechService.Stop();
        _dwellService.Reset();
        FocusedItem = null;
    }

    private void HandleFocused(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            FocusedItem = null;
            return;
        }

        FocusedItem = key;
    }

    private void CommitItem(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        FocusedItem = null;

        if (key == "読み上げ")
        {
            _ = SpeakAsync();
            return;
        }

        var previous = Text;

        if (key == "消す")
        {
            if (Text.Length > 0)
            {
                Text = Text[..^1];
            }
        }
        else if (key == "空白")
        {
            Text += " ";
        }
        else
        {
            Text += key;
        }

        if (!string.Equals(previous, Text, System.StringComparison.Ordinal))
        {
            _undoService.Push(new UndoAction(() => Text = previous));
        }
    }

    private async Task SpeakAsync()
    {
        if (string.IsNullOrWhiteSpace(Text))
        {
            return;
        }

        try
        {
            await _speechService.SpeakLatestAsync(Text).ConfigureAwait(false);
        }
        catch (System.Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }

    private sealed class CommitCommand : ICommand
    {
        private readonly Action<string> _execute;

        public CommitCommand(Action<string> execute)
        {
            _execute = execute ?? throw new System.ArgumentNullException(nameof(execute));
        }

        public event System.EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => parameter is string value && !string.IsNullOrWhiteSpace(value);

        public void Execute(object? parameter)
        {
            if (parameter is string value)
            {
                _execute(value);
            }
        }
    }
}
