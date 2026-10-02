namespace Fronted;

public partial class SplashPage : ContentPage
{
    // Кроки завантаження: текст + вага (скільки % смужки займає крок).
    // Замість Task.Delay підставте реальну ініціалізацію.
    private readonly (string Text, double Progress, Func<Task> Work)[] _steps =
    {
        ("Підготовка застосунку...", 0.25, () => Task.Delay(500)),
        ("Завантаження карт кампусу...", 0.60, () => Task.Delay(700)),
        ("Перевірка авторизації...", 0.85, () => Task.Delay(500)),
        ("Готово!", 1.00, () => Task.Delay(300)),
    };

    private bool _started;

    public SplashPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Щоб не запускати двічі (наприклад, при поверненні на сторінку)
        if (_started) return;
        _started = true;

        await RunSplashAsync();
    }

    private async Task RunSplashAsync()
    {
        // 1. Поява логотипа
        await Task.WhenAll(
            LogoImage.FadeToAsync(1, 600, Easing.CubicOut),
            LogoImage.ScaleToAsync(1, 600, Easing.CubicOut),
            TitleLabel.FadeToAsync(1, 600),
            LoadingBar.FadeToAsync(1, 600),
            StatusLabel.FadeToAsync(1, 600));

        // 2. Кроки завантаження + анімація смужки
        foreach (var step in _steps)
        {
            StatusLabel.Text = step.Text;

            await Task.WhenAll(
                step.Work(),
                LoadingBar.ProgressTo(step.Progress, 400, Easing.CubicInOut));
        }

        // 3. Плавне зникнення
        await Task.WhenAll(
            LogoImage.FadeToAsync(0, 300),
            TitleLabel.FadeToAsync(0, 300),
            LoadingBar.FadeToAsync(0, 300),
            StatusLabel.FadeToAsync(0, 300));

        // 4. Перехід на екран входу
        NavigateToLogin();
    }

    private void NavigateToLogin()
    {
        // Замініть LoginPage на назву вашої сторінки входу
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window != null)
        {
            window.Page = new NavigationPage(new MainPage());
        }
    }
}
