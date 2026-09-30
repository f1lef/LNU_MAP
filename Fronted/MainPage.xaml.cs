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
}