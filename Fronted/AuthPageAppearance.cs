using System;
using System.Runtime.Versioning;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Fronted;

// Додайте через ContentPage.Behaviors на сторінках входу та реєстрації.
#if WINDOWS
[SupportedOSPlatform("windows10.0.17763.0")]
#endif
public sealed class AuthPageAppearance : Behavior<ContentPage>
{
    private static readonly Color Background = Color.FromArgb("#140F24");
    private ContentPage? _page;

    protected override void OnAttachedTo(ContentPage page)
    {
        base.OnAttachedTo(page);
        _page = page;
        page.Loaded += HandleReady;
        page.Appearing += HandleReady;
        page.HandlerChanged += HandleReady;
        ApplyAppearance();
    }

    protected override void OnDetachingFrom(ContentPage page)
    {
        page.Loaded -= HandleReady;
        page.Appearing -= HandleReady;
        page.HandlerChanged -= HandleReady;
        _page = null;
        base.OnDetachingFrom(page);
    }

    private void HandleReady(object? sender, EventArgs e) => ApplyAppearance();

    private void ApplyAppearance()
    {
        var page = _page;
        if (page is null)
            return;

        var dispatcher = page.Dispatcher;
        if (dispatcher is not null && dispatcher.IsDispatchRequired)
        {
            dispatcher.Dispatch(ApplyAppearance);
            return;
        }

        page.BackgroundColor = Background;
        page.Padding = new Thickness(0);
        Shell.SetNavBarIsVisible(page, false);
        Shell.SetBackgroundColor(page, Background);
        NavigationPage.SetHasNavigationBar(page, false);

#if NET10_0_OR_GREATER
        // Фон проходить під системними панелями, форма враховує клавіатуру.
        page.SafeAreaEdges = SafeAreaEdges.None;
        if (page.Content is Layout root)
            root.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.SoftInput);
#endif

#if WINDOWS
        var window = page.Window;
        if (window is null)
            return;

#if NET9_0_OR_GREATER
        // Приховуємо MAUI-панель: вміст займає й область заголовка вікна.
        if (window.TitleBar is TitleBar mauiTitleBar)
            mauiTitleBar.IsVisible = false;
        else
            window.TitleBar = new TitleBar { IsVisible = false };
#endif

        if (window.Handler?.PlatformView is not Microsoft.UI.Xaml.Window nativeWindow ||
            !Microsoft.UI.Windowing.AppWindowTitleBar.IsCustomizationSupported())
            return;

        // Системні кнопки вікна мають прозоре тло над фоном сторінки.
        var titleBar = nativeWindow.AppWindow.TitleBar;
        var dark = global::Windows.UI.Color.FromArgb(255, 20, 15, 36);
        var white = global::Windows.UI.Color.FromArgb(255, 255, 255, 255);
        var muted = global::Windows.UI.Color.FromArgb(255, 196, 189, 207);
        var clear = global::Windows.UI.Color.FromArgb(0, 0, 0, 0);
        titleBar.BackgroundColor = dark;
        titleBar.ForegroundColor = white;
        titleBar.InactiveBackgroundColor = dark;
        titleBar.InactiveForegroundColor = muted;
        titleBar.ButtonBackgroundColor = clear;
        titleBar.ButtonInactiveBackgroundColor = clear;
        titleBar.ButtonForegroundColor = white;
        titleBar.ButtonInactiveForegroundColor = muted;
        titleBar.ButtonHoverForegroundColor = white;
        titleBar.ButtonHoverBackgroundColor = global::Windows.UI.Color.FromArgb(255, 50, 38, 68);
        titleBar.ButtonPressedForegroundColor = white;
        titleBar.ButtonPressedBackgroundColor = global::Windows.UI.Color.FromArgb(255, 68, 50, 85);
#endif
    }
}
