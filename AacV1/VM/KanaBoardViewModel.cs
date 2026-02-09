using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using AacV1.Core;

namespace AacV1.VM;

public sealed class KanaBoardViewModel : ObservableObject
{
    private readonly ISpeechService _speechService;
    private string _text = string.Empty;

    public KanaBoardViewModel(INavigationService navigationService, ISpeechService speechService)
    {
        _speechService = speechService;
        GoHomeCommand = new RelayCommand(() => navigationService.NavigateTo<HomeViewModel>());
        SpeakCommand = new RelayCommand(() => _ = SpeakAsync());
        StopCommand = new RelayCommand(() => _speechService.Stop());
    }

    public ICommand GoHomeCommand { get; }
    public ICommand SpeakCommand { get; }
    public ICommand StopCommand { get; }

    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }

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
}
