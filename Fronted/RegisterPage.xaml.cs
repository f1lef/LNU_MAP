using Microsoft.Maui.Controls;

namespace Fronted;

public partial class RegisterPage : ContentPage
{
    public RegisterPage()
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

        // Початковий стан: прозоро і трохи правіше
        pageContent.Opacity = 0;
        pageContent.TranslationX = 40;

        var animation = new Animation(progress =>
        {
            pageContent.Opacity = progress;
            pageContent.TranslationX = 40 * (1 - progress);
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
    private async void OnBackToLoginTapped(
        object? sender, TappedEventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}