using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Maui.Controls;

namespace Fronted;

public partial class RegisterPage : ContentPage
{
    private readonly HttpClient _httpClient;

    public RegisterPage()
    {
        InitializeComponent();

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5147/"),
            Timeout = TimeSpan.FromSeconds(15)
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        var pageContent = Content;

        if (pageContent == null)
            return;

        pageContent.AbortAnimation("PageTransition");

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

    // =====================================================
    // REGISTER
    // =====================================================

    private async void OnRegisterClicked(
        object? sender,
        EventArgs e)
    {
        string email =
            RegisterEmailEntry.Text?.Trim()
            ?? string.Empty;

        string group =
            RegisterGroupEntry.Text?.Trim()
            ?? string.Empty;

        string password =
            RegisterPasswordEntry.Text
            ?? string.Empty;

        string confirmPassword =
            RegisterConfirmPasswordEntry.Text
            ?? string.Empty;

        // Перевіряємо, чи всі поля заповнені.
        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(group) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(confirmPassword))
        {
            await DisplayAlertAsync(
                "Помилка",
                "Заповни всі поля.",
                "OK"
            );

            return;
        }

        // Перевіряємо повтор пароля.
        if (password != confirmPassword)
        {
            await DisplayAlertAsync(
                "Помилка",
                "Паролі не співпадають.",
                "OK"
            );

            return;
        }

        // Дані для Backend.
        var request = new
        {
            Email = email,
            Password = password,
            Group = group
        };

        try
        {
            RegisterButton.IsEnabled = false;
            RegisterButton.Text = "Реєстрація...";

            // POST /api/auth/register
            HttpResponseMessage response =
                await _httpClient.PostAsJsonAsync(
                    "api/auth/register",
                    request
                );

            string responseBody =
                await response.Content.ReadAsStringAsync();

            // Успішна реєстрація.
            if (response.IsSuccessStatusCode)
            {
                await DisplayAlertAsync(
                    "Успішно",
                    "Акаунт успішно створено.",
                    "OK"
                );

                await Navigation.PopModalAsync();

                return;
            }

            // Помилка від Backend.
            string message =
                GetBackendMessage(responseBody);

            await DisplayAlertAsync(
                "Помилка",
                message,
                "OK"
            );
        }
        catch (HttpRequestException)
        {
            await DisplayAlertAsync(
                "Помилка з'єднання",
                "Не вдалося підключитися до Backend.",
                "OK"
            );
        }
        catch (TaskCanceledException)
        {
            await DisplayAlertAsync(
                "Помилка",
                "Сервер занадто довго не відповідає.",
                "OK"
            );
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Помилка",
                ex.Message,
                "OK"
            );
        }
        finally
        {
            RegisterButton.IsEnabled = true;
            RegisterButton.Text = "Зареєструватися";
        }
    }

    // =====================================================
    // BACKEND MESSAGE
    // =====================================================

    private static string GetBackendMessage(
        string responseBody)
    {
        try
        {
            using JsonDocument json =
                JsonDocument.Parse(responseBody);

            if (json.RootElement.TryGetProperty(
                    "message",
                    out JsonElement message))
            {
                return message.GetString()
                    ?? "Сталася помилка.";
            }
        }
        catch (JsonException)
        {
        }

        if (!string.IsNullOrWhiteSpace(responseBody))
        {
            return responseBody;
        }

        return "Сталася помилка.";
    }

    // =====================================================
    // BACK TO LOGIN
    // =====================================================

    private async void OnBackToLoginTapped(
        object? sender,
        TappedEventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}