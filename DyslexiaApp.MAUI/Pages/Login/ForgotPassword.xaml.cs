#if ANDROID
using Android.Content.PM;
#endif

using DyslexiaApp.MAUI.ViewModels;
namespace DyslexiaApp.MAUI.Pages.Login;

public partial class ForgotPassword : ContentPage
{
    private readonly ForgotPasswordViewModel _viewModel;

    public ForgotPassword(ForgotPasswordViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    private async void OnSendEmailButtonClicked(object sender, EventArgs e)
    {
        if (_viewModel.SendCodeCommand.CanExecute(null))
        {
            _viewModel.SendCodeCommand.Execute(null);
        }
    }

    private async void OnVerifyCodeButtonClicked(object sender, EventArgs e)
    {
        if (_viewModel.VerifyCodeCommand.CanExecute(null))
        {
            _viewModel.VerifyCodeCommand.Execute(null);
        }
    }

    private void OnCloseButtonClicked(object sender, EventArgs e)
    {
        Navigation.PopModalAsync();
    }

#if ANDROID

    private void SetOrientation(ScreenOrientation orientation)
    {
        var activity = Platform.CurrentActivity;
        activity.RequestedOrientation = orientation;
    }
#endif

    protected override void OnAppearing()
    {
        base.OnAppearing();
#if ANDROID
        SetOrientation(ScreenOrientation.Portrait);
#endif
    }
}