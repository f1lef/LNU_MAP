using System;
using Microsoft.Maui.Controls;

namespace Fronted;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        await DisplayAlertAsync(
            "Вхід",
            "Успішний вхід, вітаємо тебе першокурснику!",
            "OK");
    }

    private async void OnRegisterTapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushModalAsync(new RegisterPage(), false);
    }
}