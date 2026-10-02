using System;
using Microsoft.Maui.Controls;

namespace Fronted;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();

        var pageContent = Content;
        if (pageContent == null)
            return;

        pageContent.AbortAnimation("PageTransition");

        pageContent.Opacity = 0;
        pageContent.TranslationX = -40;

        var animation = new Animation(progress =>
        {
            pageContent.Opacity = progress;
            pageContent.TranslationX = -40 * (1 - progress);
        }, 0, 1);

        animation.Commit(
            pageContent,
            "PageTransition",
            length: 300,
            easing: Easing.CubicOut,
            finished: (value, cancelled) =>
            {
                pageContent.Opacity = 1;
                pageContent.TranslationX = 0;
            });
    }
    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        await DisplayAlertAsync(
            "Вхід",
            "Успішний вхід, вітаємо тебе першокурснику!",
            "OK");
    }

    private async void OnRegisterTapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PushModalAsync(new RegisterPage(), false);
    }
}