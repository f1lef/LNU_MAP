using System;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;

namespace Fronted;

// Додай цей клас у MAUI-проєкт Fronted поруч зі сторінкою реєстрації.
// Малюнок створюється кодом: файли PNG/SVG і додаткові пакети не потрібні.
public sealed class AnimatedRegistrationBackground : GraphicsView
{
    private const double AnimationDurationSeconds = 8;
    private readonly RegistrationArtwork _artwork = new();
    private readonly Stopwatch _clock = new();
    private IDispatcherTimer? _timer;
    private Page? _page;
    private bool _loaded;
    private bool _pageActive;

    public AnimatedRegistrationBackground()
    {
        Drawable = _artwork;
        InputTransparent = true;
        BackgroundColor = Colors.Transparent;
        HorizontalOptions = LayoutOptions.Fill;
        VerticalOptions = LayoutOptions.Fill;

        Loaded += HandleLoaded;
        Unloaded += HandleUnloaded;
        HandlerChanged += (_, _) =>
        {
            if (Handler is null)
                StopAnimation();
            else
                StartAnimation();
        };
        SizeChanged += (_, _) =>
        {
            Invalidate();
            StartAnimation();
        };
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(IsVisible))
                return;

            if (IsVisible)
                StartAnimation();
            else
                StopAnimation();
        };
    }

    // 1.0 — звичайний розмір, 1.2 — на 20% більший малюнок.
    public double MapZoom
    {
        get => _artwork.Zoom;
        set
        {
            _artwork.Zoom = (float)Math.Clamp(value, 1.0, 1.25);
            Invalidate();
        }
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

        _artwork.Phase = 0;
        _clock.Restart();
        Invalidate();
        _timer.Start();
    }

    private void HandleTick(object? sender, EventArgs e)
    {
        if (!_loaded || !_pageActive || !IsVisible || Handler is null)
        {
            StopAnimation();
            return;
        }

        // Фаза залежить від часу, а не від кількості кадрів.
        _artwork.Phase = (float)(
            (_clock.Elapsed.TotalSeconds % AnimationDurationSeconds)
            / AnimationDurationSeconds);
        Invalidate();
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

    private sealed class RegistrationArtwork : IDrawable
    {
        private static readonly Color RoadColor = Color.FromArgb("#2E2249");
        private static readonly Color RouteColor = Color.FromArgb("#80629F");
        private static readonly Color Purple = Color.FromArgb("#B094E1");
        private static readonly Color Gold = Color.FromArgb("#FFC15A");
        private static readonly Color Light = Color.FromArgb("#F4EDFF");
        private static readonly Color Dark = Color.FromArgb("#140F24");

        private static readonly PointF[] Stops =
        {
            new(70, 174), new(210, 110), new(342, 169)
        };

        private readonly PathF[] _roads = CreateRoads();
        private readonly PathF _pin = CreatePin();
        private readonly PointF[] _routeDots = CreateRouteDots();

        public float Phase { get; set; }
        public float Zoom { get; set; } = 1.2f;

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (dirtyRect.Width <= 0 || dirtyRect.Height <= 0)
                return;

            float scale = MathF.Min(dirtyRect.Width / 400f,
                dirtyRect.Height / 270f) * Zoom;
            float offsetX = dirtyRect.X + (dirtyRect.Width - 400f * scale) / 2f;

            canvas.SaveState();
            canvas.Translate(offsetX, dirtyRect.Y);
            canvas.Scale(scale, scale);
            canvas.StrokeLineCap = LineCap.Round;
            canvas.StrokeColor = RoadColor;
            canvas.StrokeSize = 5;
            foreach (PathF road in _roads)
                canvas.DrawPath(road);

            // Замкнений маршрут: точки повертаються без стрибка.
            canvas.FillColor = RouteColor;
            foreach (PointF dot in _routeDots)
                FillCircle(canvas, dot.X, dot.Y, 1.4f);

            DrawTraveller(canvas, Phase, 244, 237, 255);
            DrawTraveller(canvas, (Phase + 0.46f) % 1f, 255, 193, 90);

            for (int i = 0; i < Stops.Length; i++)
            {
                PointF stop = Stops[i];
                float wave = (Phase * 2f + i / 3f) % 1f;
                canvas.FillColor = Color.FromRgba(176, 148, 225,
                    (int)(55f * (1f - wave)));
                FillCircle(canvas, stop.X, stop.Y, 4f + 15f * wave);

                canvas.FillColor = Purple;
                FillCircle(canvas, stop.X, stop.Y, 3f);

                float bob = 3f * MathF.Sin(Phase * MathF.PI * 2f + i * 1.4f);
                canvas.SaveState();
                canvas.Translate(stop.X, stop.Y - 6f + bob);
                canvas.FillColor = i == 1 ? Gold : i == 2 ? Light : Purple;
                canvas.FillPath(_pin);
                canvas.FillColor = Dark;
                FillCircle(canvas, 0, -19, 3.6f);
                canvas.RestoreState();
            }

            canvas.RestoreState();
        }

        private static void DrawTraveller(ICanvas canvas, float phase,
            int red, int green, int blue)
        {
            // Короткий хвіст робить рух помітним і на малому екрані.
            for (int i = 6; i >= 1; i--)
            {
                float tailPhase = (phase - i * 0.007f + 1f) % 1f;
                PointF tail = RoutePoint(tailPhase);
                canvas.FillColor = Color.FromRgba(red, green, blue,
                    (int)(100f * (1f - i / 7f)));
                FillCircle(canvas, tail.X, tail.Y, 2.2f);
            }

            PointF point = RoutePoint(phase);
            canvas.FillColor = Color.FromRgba(red, green, blue, 45);
            FillCircle(canvas, point.X, point.Y, 10f);
            canvas.FillColor = Color.FromRgba(red, green, blue, 100);
            FillCircle(canvas, point.X, point.Y, 6f);
            canvas.FillColor = Color.FromRgba(red, green, blue, 255);
            FillCircle(canvas, point.X, point.Y, 3.4f);
        }

        private static void FillCircle(ICanvas canvas, float x, float y, float radius)
            => canvas.FillEllipse(x - radius, y - radius, radius * 2, radius * 2);

        private static PointF[] CreateRouteDots()
        {
            var dots = new PointF[72];
            for (int i = 0; i < dots.Length; i++)
                dots[i] = RoutePoint(i / (float)dots.Length);
            return dots;
        }

        private static PointF RoutePoint(float phase)
        {
            float section = (phase % 1f) * 3f;
            int index = Math.Min(2, (int)section);
            float t = section - index;
            return index switch
            {
                0 => Bezier(Stops[0], new PointF(94, 119),
                    new PointF(149, 150), Stops[1], t),
                1 => Bezier(Stops[1], new PointF(265, 78),
                    new PointF(281, 168), Stops[2], t),
                _ => Bezier(Stops[2], new PointF(300, 232),
                    new PointF(129, 236), Stops[0], t)
            };
        }

        private static PointF Bezier(PointF start, PointF first,
            PointF second, PointF end, float t)
        {
            float u = 1f - t;
            float a = u * u * u;
            float b = 3f * u * u * t;
            float c = 3f * u * t * t;
            float d = t * t * t;
            return new PointF(a * start.X + b * first.X + c * second.X + d * end.X,
                a * start.Y + b * first.Y + c * second.Y + d * end.Y);
        }

        private static PathF CreatePin()
        {
            var pin = new PathF();
            pin.MoveTo(0, 0);
            pin.CurveTo(-4, -5, -12, -11, -12, -19);
            pin.CurveTo(-12, -35, 12, -35, 12, -19);
            pin.CurveTo(12, -11, 4, -5, 0, 0);
            pin.Close();
            return pin;
        }

        private static PathF[] CreateRoads()
        {
            var upper = new PathF();
            upper.MoveTo(12, 99);
            upper.CurveTo(120, 130, 206, 66, 390, 116);

            var lower = new PathF();
            lower.MoveTo(8, 202);
            lower.CurveTo(122, 183, 278, 234, 394, 190);

            var diagonal = new PathF();
            diagonal.MoveTo(18, 250);
            diagonal.CurveTo(118, 190, 245, 127, 375, 23);

            var left = new PathF();
            left.MoveTo(90, 12);
            left.CurveTo(121, 72, 114, 168, 151, 258);

            var right = new PathF();
            right.MoveTo(301, 10);
            right.CurveTo(270, 99, 341, 191, 321, 256);

            return new[] { upper, lower, diagonal, left, right };
        }
    }
}
