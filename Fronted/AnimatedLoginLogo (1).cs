using System;
using System.Diagnostics;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Fronted;

// Використовує справжнє Resources/Images/logo.jpg.
// Файл містить анімований логотип і кнопку показу пароля.
public sealed class AnimatedLoginLogo : ContentView
{
    private readonly Image _logo = new()
    {
        Source = ImageSource.FromFile("logo.jpg"),
        Aspect = Aspect.AspectFit,
        InputTransparent = true
    };
    private readonly AbsoluteLayout _scene = new()
    {
        BackgroundColor = Colors.Transparent,
        InputTransparent = true
    };
    private readonly LogoPaws _artwork = new();
    private readonly GraphicsView _overlay;
    private readonly LogoOrbit _orbit = new();
    private readonly GraphicsView _decoration;
    private readonly Stopwatch _clock = new();
    private IDispatcherTimer? _timer;
    private Page? _page;
    private bool _loaded;
    private bool _pageActive;
    private double _lastSeconds;
    private float _targetX;
    private float _targetTilt;
    private float _targetCover;
    private float _moveX;
    private float _tilt;
    private double _designScale = 1;

    // На реєстрації — орбіта й пульсація; на вході — звичне погойдування.
    public static readonly BindableProperty IsRegistrationAnimationProperty = BindableProperty.Create(
        nameof(IsRegistrationAnimation), typeof(bool), typeof(AnimatedLoginLogo), false,
        propertyChanged: (view, oldValue, newValue) =>
            ((AnimatedLoginLogo)view).RenderFrame());

    public bool IsRegistrationAnimation
    {
        get => (bool)GetValue(IsRegistrationAnimationProperty);
        set => SetValue(IsRegistrationAnimationProperty, value);
    }

    public static readonly BindableProperty LogoSourceProperty = BindableProperty.Create(
        nameof(LogoSource), typeof(ImageSource), typeof(AnimatedLoginLogo),
        ImageSource.FromFile("logo.jpg"),
        propertyChanged: (view, oldValue, newValue) =>
            ((AnimatedLoginLogo)view)._logo.Source = (ImageSource)newValue);

    public ImageSource LogoSource
    {
        get => (ImageSource)GetValue(LogoSourceProperty);
        set => SetValue(LogoSourceProperty, value);
    }

    public static readonly BindableProperty EmailLengthProperty = BindableProperty.Create(
        nameof(EmailLength), typeof(int), typeof(AnimatedLoginLogo), 0,
        propertyChanged: HandleInputChanged);
    public static readonly BindableProperty PasswordLengthProperty = BindableProperty.Create(
        nameof(PasswordLength), typeof(int), typeof(AnimatedLoginLogo), 0,
        propertyChanged: HandleInputChanged);
    public static readonly BindableProperty IsEmailFocusedProperty = BindableProperty.Create(
        nameof(IsEmailFocused), typeof(bool), typeof(AnimatedLoginLogo), false,
        propertyChanged: HandleInputChanged);
    public static readonly BindableProperty IsPasswordFocusedProperty = BindableProperty.Create(
        nameof(IsPasswordFocused), typeof(bool), typeof(AnimatedLoginLogo), false,
        propertyChanged: HandleInputChanged);
    public static readonly BindableProperty IsPasswordHiddenProperty = BindableProperty.Create(
        nameof(IsPasswordHidden), typeof(bool), typeof(AnimatedLoginLogo), true,
        propertyChanged: HandleInputChanged);

    public int EmailLength
    {
        get => (int)GetValue(EmailLengthProperty);
        set => SetValue(EmailLengthProperty, value);
    }

    public int PasswordLength
    {
        get => (int)GetValue(PasswordLengthProperty);
        set => SetValue(PasswordLengthProperty, value);
    }

    public bool IsEmailFocused
    {
        get => (bool)GetValue(IsEmailFocusedProperty);
        set => SetValue(IsEmailFocusedProperty, value);
    }

    public bool IsPasswordFocused
    {
        get => (bool)GetValue(IsPasswordFocusedProperty);
        set => SetValue(IsPasswordFocusedProperty, value);
    }

    public bool IsPasswordHidden
    {
        get => (bool)GetValue(IsPasswordHiddenProperty);
        set => SetValue(IsPasswordHiddenProperty, value);
    }

    // Додаткові поля реєстрації. Значення за замовчуванням зберігають поведінку входу.
    public static readonly BindableProperty GroupLengthProperty = BindableProperty.Create(
        nameof(GroupLength), typeof(int), typeof(AnimatedLoginLogo), 0,
        propertyChanged: HandleInputChanged);
    public static readonly BindableProperty IsGroupFocusedProperty = BindableProperty.Create(
        nameof(IsGroupFocused), typeof(bool), typeof(AnimatedLoginLogo), false,
        propertyChanged: HandleInputChanged);
    public static readonly BindableProperty ConfirmPasswordLengthProperty = BindableProperty.Create(
        nameof(ConfirmPasswordLength), typeof(int), typeof(AnimatedLoginLogo), 0,
        propertyChanged: HandleInputChanged);
    public static readonly BindableProperty IsConfirmPasswordFocusedProperty = BindableProperty.Create(
        nameof(IsConfirmPasswordFocused), typeof(bool), typeof(AnimatedLoginLogo), false,
        propertyChanged: HandleInputChanged);
    public static readonly BindableProperty IsConfirmPasswordHiddenProperty = BindableProperty.Create(
        nameof(IsConfirmPasswordHidden), typeof(bool), typeof(AnimatedLoginLogo), true,
        propertyChanged: HandleInputChanged);

    public int GroupLength
    {
        get => (int)GetValue(GroupLengthProperty);
        set => SetValue(GroupLengthProperty, value);
    }

    public bool IsGroupFocused
    {
        get => (bool)GetValue(IsGroupFocusedProperty);
        set => SetValue(IsGroupFocusedProperty, value);
    }

    public int ConfirmPasswordLength
    {
        get => (int)GetValue(ConfirmPasswordLengthProperty);
        set => SetValue(ConfirmPasswordLengthProperty, value);
    }

    public bool IsConfirmPasswordFocused
    {
        get => (bool)GetValue(IsConfirmPasswordFocusedProperty);
        set => SetValue(IsConfirmPasswordFocusedProperty, value);
    }

    public bool IsConfirmPasswordHidden
    {
        get => (bool)GetValue(IsConfirmPasswordHiddenProperty);
        set => SetValue(IsConfirmPasswordHiddenProperty, value);
    }

    public AnimatedLoginLogo()
    {
        _decoration = new GraphicsView
        {
            Drawable = _orbit,
            BackgroundColor = Colors.Transparent,
            InputTransparent = true,
            IsVisible = false
        };
        _overlay = new GraphicsView
        {
            Drawable = _artwork,
            BackgroundColor = Colors.Transparent,
            InputTransparent = true
        };
        WidthRequest = 200;
        HeightRequest = 150;
        HorizontalOptions = LayoutOptions.Center;
        InputTransparent = true;
        BackgroundColor = Colors.Transparent;

        _scene.Children.Add(_decoration);
        AbsoluteLayout.SetLayoutBounds((BindableObject)_decoration, new Rect(0, 0, 1, 1));
        AbsoluteLayout.SetLayoutFlags((BindableObject)_decoration, AbsoluteLayoutFlags.All);
        _scene.Children.Add(_logo);
        _scene.Children.Add(_overlay);
        AbsoluteLayout.SetLayoutBounds((BindableObject)_overlay, new Rect(0, 0, 1, 1));
        AbsoluteLayout.SetLayoutFlags((BindableObject)_overlay, AbsoluteLayoutFlags.All);
        Content = _scene;

        Loaded += HandleLoaded;
        Unloaded += HandleUnloaded;
        _scene.SizeChanged += (_, _) => UpdateArtworkSize();
        HandlerChanged += (_, _) =>
        {
            if (Handler is null)
                StopAnimation();
            else
                StartAnimation();
        };
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(IsVisible))
            {
                if (IsVisible)
                    StartAnimation();
                else
                    StopAnimation();
            }
        };
    }

    private void UpdateArtworkSize()
    {
        double width = _scene.Width;
        double height = _scene.Height;
        if (width <= 0 || height <= 0)
            return;

        _designScale = Math.Min(width / 200, height / 150);
        double x = (width - 200 * _designScale) / 2;
        double y = (height - 150 * _designScale) / 2;
        AbsoluteLayout.SetLayoutBounds((BindableObject)_logo,
            new Rect(x + 40 * _designScale, y + 10 * _designScale,
                     120 * _designScale, 120 * _designScale));
        RenderFrame();
    }

    private static void HandleInputChanged(BindableObject view, object oldValue, object newValue)
        => ((AnimatedLoginLogo)view).UpdateExpression();

    private void UpdateExpression()
    {
        bool typing = IsEmailFocused || IsGroupFocused ||
                      IsPasswordFocused || IsConfirmPasswordFocused;
        int length = IsConfirmPasswordFocused ? ConfirmPasswordLength :
                     IsPasswordFocused ? PasswordLength :
                     IsGroupFocused ? GroupLength : EmailLength;
        _targetX = typing ? -6f + Math.Clamp(length, 0, 20) * 0.6f : 0f;
        _targetTilt = typing ? -2f + Math.Clamp(length, 0, 20) * 0.2f : 0f;
        _targetCover = IsPasswordHidden && IsConfirmPasswordHidden ? 0f : 1f;

        if (_timer?.IsRunning != true)
        {
            _moveX = _targetX;
            _tilt = _targetTilt;
            _artwork.Cover = _targetCover;
            RenderFrame();
        }
    }

    private void RenderFrame()
    {
        // Рухається вся композиція: логотип і лапки залишаються разом.
        double time = _artwork.Time;
        _decoration.IsVisible = IsRegistrationAnimation;
        if (IsRegistrationAnimation)
        {
            // Інша анімація для реєстрації: дихання й точки по орбіті.
            _scene.TranslationX = _moveX * _designScale * 0.5;
            _scene.TranslationY = Math.Sin(time * 2.1) * 1.5 * _designScale;
            _scene.Rotation = _tilt + Math.Sin(time * 0.9) * 3;
            _scene.Scale = 1 + Math.Sin(time * 2.1) * 0.035;
            _orbit.Time = _artwork.Time;
            _decoration.Invalidate();
        }
        else
        {
            _scene.TranslationX = _moveX * _designScale;
            _scene.TranslationY = Math.Sin(time * 1.7) * 3 * _designScale;
            _scene.Rotation = _tilt + Math.Sin(time * 1.2) * 0.6;
            _scene.Scale = 1 + Math.Sin(time * 1.7) * 0.012;
        }
        _overlay.Invalidate();
    }

    private void HandleLoaded(object? sender, EventArgs e)
    {
        _loaded = true;
        DetachPage();

        Element? parent = Parent;
        while (parent is not null && parent is not Page)
            parent = parent.Parent;

        _page = parent as Page;
        if (_page is not null)
        {
            _page.Appearing += HandleAppearing;
            _page.Disappearing += HandleDisappearing;
        }

        _pageActive = true;
        StartAnimation();
    }

    private void HandleUnloaded(object? sender, EventArgs e)
    {
        _loaded = false;
        _pageActive = false;
        StopAnimation();
        DetachPage();
    }

    private void DetachPage()
    {
        if (_page is null)
            return;

        _page.Appearing -= HandleAppearing;
        _page.Disappearing -= HandleDisappearing;
        _page = null;
    }

    private void HandleAppearing(object? sender, EventArgs e)
    {
        _pageActive = true;
        StartAnimation();
    }

    private void HandleDisappearing(object? sender, EventArgs e)
    {
        _pageActive = false;
        StopAnimation();
    }

    public void StartAnimation()
    {
        if (!_loaded || !_pageActive || !IsVisible || Handler is null)
            return;

        var dispatcher = Dispatcher;
        if (dispatcher is null)
            return;

        if (dispatcher.IsDispatchRequired)
        {
            dispatcher.Dispatch(StartAnimation);
            return;
        }

        if (_timer?.IsRunning == true)
            return;

        StopAnimation();

        // Таймер виконує Tick у черзі UI. Animation.Commit тут не потрібний.
        _timer = dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(33);
        _timer.IsRepeating = true;
        _timer.Tick += HandleTick;

        _lastSeconds = 0;
        _clock.Restart();
        RenderFrame();
        _timer.Start();
    }

    private void HandleTick(object? sender, EventArgs e)
    {
        if (!_loaded || !_pageActive || !IsVisible || Handler is null)
        {
            StopAnimation();
            return;
        }

        double seconds = _clock.Elapsed.TotalSeconds;
        float delta = (float)Math.Clamp(seconds - _lastSeconds, 0, 0.05);
        _lastSeconds = seconds;
        _artwork.Time = (float)(seconds % 3600);

        // Плавний рух незалежно від швидкості пристрою.
        float blend = 1f - MathF.Exp(-12f * delta);
        _moveX += (_targetX - _moveX) * blend;
        _tilt += (_targetTilt - _tilt) * blend;
        _artwork.Cover += (_targetCover - _artwork.Cover) * blend;
        RenderFrame();
    }

    public void StopAnimation()
    {
        if (_timer is not null)
        {
            _timer.Stop();
            _timer.Tick -= HandleTick;
            _timer = null;
        }

        _clock.Stop();
    }

    private sealed class LogoOrbit : IDrawable
    {
        private static readonly Color Ring = Color.FromArgb("#CDB2FF");
        private static readonly Color Light = Color.FromArgb("#F7EEFF");
        private static readonly Color Gold = Color.FromArgb("#FFE2A8");
        public float Time { get; set; }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (dirtyRect.Width <= 0 || dirtyRect.Height <= 0)
                return;

            float scale = MathF.Min(dirtyRect.Width / 200f, dirtyRect.Height / 150f);
            float x = dirtyRect.X + (dirtyRect.Width - 200f * scale) / 2f;
            float y = dirtyRect.Y + (dirtyRect.Height - 150f * scale) / 2f;
            canvas.SaveState();
            canvas.Translate(x, y);
            canvas.Scale(scale, scale);
            canvas.Alpha = 0.42f;
            canvas.StrokeColor = Ring;
            canvas.StrokeSize = 1.5f;
            canvas.DrawEllipse(15, 20, 170, 100);

            float angle = Time * 1.1f;
            DrawLight(canvas, angle, Light);
            DrawLight(canvas, angle + MathF.PI, Gold);
            canvas.RestoreState();
        }

        private static void DrawLight(ICanvas canvas, float angle, Color color)
        {
            // Короткий світлий шлейф позаду кожної точки.
            canvas.FillColor = color;
            for (int i = 5; i >= 1; i--)
            {
                float trailAngle = angle - i * 0.1f;
                float trailX = 100 + MathF.Cos(trailAngle) * 85;
                float trailY = 70 + MathF.Sin(trailAngle) * 50;
                canvas.Alpha = (6 - i) * 0.055f;
                canvas.FillEllipse(trailX - 2, trailY - 2, 4, 4);
            }
            float x = 100 + MathF.Cos(angle) * 85;
            float y = 70 + MathF.Sin(angle) * 50;
            canvas.Alpha = 0.1f;
            canvas.FillEllipse(x - 9, y - 9, 18, 18);
            canvas.Alpha = 0.22f;
            canvas.FillEllipse(x - 6, y - 6, 12, 12);
            canvas.Alpha = 1;
            canvas.FillEllipse(x - 3.5f, y - 3.5f, 7, 7);
        }
    }

    private sealed class LogoPaws : IDrawable
    {
        private static readonly Color Purple = Color.FromArgb("#8B5BD0");
        private static readonly Color Outline = Color.FromArgb("#614099");
        private static readonly Color Pink = Color.FromArgb("#ED9BCB");
        private readonly PathF _arm = CreateArm();

        public float Time { get; set; }
        public float Cover { get; set; }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (dirtyRect.Width <= 0 || dirtyRect.Height <= 0 || Cover < 0.001f)
                return;

            float scale = MathF.Min(dirtyRect.Width / 200f, dirtyRect.Height / 150f);
            float x = dirtyRect.X + (dirtyRect.Width - 200f * scale) / 2f;
            float y = dirtyRect.Y + (dirtyRect.Height - 150f * scale) / 2f;
            float progress = Math.Clamp(Cover, 0f, 1f);

            canvas.SaveState();
            canvas.Translate(x, y);
            canvas.Scale(scale, scale);
            canvas.Alpha = progress;
            canvas.StrokeLineCap = LineCap.Round;

            // Лапки підіймаються знизу до середини логотипа.
            float top = 154f - 94f * progress;
            DrawPaw(canvas, 82, top);
            DrawPaw(canvas, 118, top);
            canvas.RestoreState();
        }

        private void DrawPaw(ICanvas canvas, float x, float top)
        {
            canvas.SaveState();
            canvas.Translate(x, top);
            canvas.FillColor = Purple;
            canvas.StrokeColor = Outline;
            canvas.StrokeSize = 2.5f;
            canvas.FillPath(_arm);
            canvas.DrawPath(_arm);
            FillCircle(canvas, 0, 0, 17);
            canvas.DrawEllipse(-17, -17, 34, 34);
            canvas.FillColor = Pink;
            canvas.FillEllipse(-8, 1, 16, 11);
            FillCircle(canvas, -9, -5, 3f);
            FillCircle(canvas, -3, -10, 3f);
            FillCircle(canvas, 5, -9, 3f);
            FillCircle(canvas, 11, -3, 3f);
            canvas.RestoreState();
        }

        private static void FillCircle(ICanvas canvas, float x, float y, float radius)
            => canvas.FillEllipse(x - radius, y - radius, radius * 2, radius * 2);

        private static PathF CreateArm()
        {
            var path = new PathF();
            path.MoveTo(-14, 6);
            path.CurveTo(-14, -4, 14, -4, 14, 6);
            path.LineTo(14, 76);
            path.LineTo(-14, 76);
            path.Close();
            return path;
        }
    }
}

// Кнопка синхронізується з IsPassword у PasswordEntry через TwoWay binding.
public sealed class PasswordRevealButton : Button
{
    public static readonly BindableProperty IsPasswordHiddenProperty = BindableProperty.Create(
        nameof(IsPasswordHidden), typeof(bool), typeof(PasswordRevealButton), true,
        defaultBindingMode: BindingMode.TwoWay,
        propertyChanged: (view, oldValue, newValue) =>
            ((PasswordRevealButton)view).UpdateCaption());

    public bool IsPasswordHidden
    {
        get => (bool)GetValue(IsPasswordHiddenProperty);
        set => SetValue(IsPasswordHiddenProperty, value);
    }

    public PasswordRevealButton()
    {
        BackgroundColor = Colors.Transparent;
        TextColor = Color.FromArgb("#C4BDCF");
        FontSize = 11;
        FontAttributes = FontAttributes.Bold;
        BorderWidth = 0;
        CornerRadius = 10;
        Padding = new Thickness(8, 0);
        UpdateCaption();
        Clicked += (_, _) => IsPasswordHidden = !IsPasswordHidden;
    }

    private void UpdateCaption()
        => Text = IsPasswordHidden ? "Показати" : "Сховати";
}
