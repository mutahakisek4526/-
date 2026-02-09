namespace AacV1.VM;

public interface INavigationService
{
    void NavigateTo<TViewModel>() where TViewModel : ObservableObject;
}
