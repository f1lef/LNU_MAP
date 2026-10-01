using Microsoft.Maui.Controls;

namespace Fronted;

public partial class RegisterPage : ContentPage
{
    public RegisterPage()
    {
        InitializeComponent();
    }

    private async void OnBackToLoginTapped(
        object sender, TappedEventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}