using System;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;

namespace Fronted;

public sealed class AnimatedMapBackground : GraphicsView
{
    private const double AnimationDurationSeconds = 6;

    private readonly MapArtwork _artwork = new();
    private readonly Stopwatch _clock = new();

    private IDispatcherTimer? _timer;
    private Page? _page;
    private bool _loaded;
    private bool _pageActive;

    public AnimatedMapBackground()
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

    private sealed class MapArtwork : IDrawable
    {
        private static readonly Color RoadColor =
            Color.FromArgb("#2E2249");

        private static readonly Color Purple =
            Color.FromArgb("#B094E1");

        private static readonly Color Gold =
            Color.FromArgb("#FFC15A");

        private static readonly Color Dark =
            Color.FromArgb("#140F24");

        private readonly PathF[] _roads = CreateRoads();
        private readonly PathF _pin = CreatePin();
        private readonly PointF[] _routeDots = CreateRouteDots();

        public float Phase { get; set; }
        public float Zoom { get; set; } = 1.2f;

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (dirtyRect.Width <= 0 || dirtyRect.Height <= 0)
                return;

            float scale = MathF.Min(
                dirtyRect.Width / 400f,
                dirtyRect.Height / 270f) * Zoom;

            float offsetX =
                dirtyRect.X + (dirtyRect.Width - 400f * scale) / 2f;

            canvas.SaveState();
            canvas.Translate(offsetX, dirtyRect.Y);
            canvas.Scale(scale, scale);
            canvas.StrokeLineCap = LineCap.Round;

            // Вулиці
            canvas.StrokeColor = RoadColor;
            canvas.StrokeSize = 5;

            foreach (PathF road in _roads)
                canvas.DrawPath(road);

            // Пунктир маршруту
            canvas.FillColor = Purple;

            foreach (PointF dot in _routeDots)
                FillCircle(canvas, dot.X, dot.Y, 1.5f);

            // Пульс на початку маршруту
            float wave = (Phase * 2f) % 1f;

            canvas.FillColor = Color.FromRgba(
                176, 148, 225,
                (int)(48f * (1f - wave)));

            FillCircle(canvas, 56, 167, 5f + 10f * wave);

            canvas.FillColor = Purple;
            FillCircle(canvas, 56, 167, 4.2f);

            // Крапка, яка рухається маршрутом
            PointF traveller = RoutePoint(Phase);

            float visibility = MathF.Min(
                1f,
                MathF.Min(Phase * 14f, (1f - Phase) * 14f));

            canvas.FillColor = Color.FromRgba(
                176, 148, 225,
                (int)(55f * visibility));

            FillCircle(canvas, traveller.X, traveller.Y, 8);

            canvas.FillColor = Color.FromRgba(
                231, 218, 255,
                (int)(255f * visibility));

            FillCircle(canvas, traveller.X, traveller.Y, 3.3f);

            // Рух жовтої мітки
            float bob = 6f * MathF.Sin(Phase * MathF.PI * 4f);

            canvas.FillColor = Color.FromRgba(255, 193, 90, 35);
            canvas.FillEllipse(317, 108, 30, 7);

            canvas.SaveState();
            canvas.Translate(332, 106 + bob);

            canvas.FillColor = Gold;
            canvas.FillPath(_pin);

            canvas.FillColor = Dark;
            FillCircle(canvas, 0, -33, 6.2f);

            canvas.RestoreState();
            canvas.RestoreState();
        }

        private static void FillCircle(
            ICanvas canvas,
            float x,
            float y,
            float radius)
        {
            canvas.FillEllipse(
                x - radius,
                y - radius,
                radius * 2,
                radius * 2);
        }

        private static PathF[] CreateRoads()
        {
            var top = new PathF();
            top.MoveTo(20, 106);
            top.CurveTo(126, 87, 163, 145, 258, 114);
            top.CurveTo(302, 100, 347, 80, 382, 89);

            var bottom = new PathF();
            bottom.MoveTo(18, 182);
            bottom.CurveTo(103, 169, 137, 205, 216, 193);
            bottom.CurveTo(285, 181, 320, 158, 383, 176);

            var left = new PathF();
            left.MoveTo(84, 8);
            left.CurveTo(95, 91, 81, 169, 94, 249);

            var middle = new PathF();
            middle.MoveTo(196, 2);
            middle.CurveTo(190, 87, 225, 177, 209, 252);

            var right = new PathF();
            right.MoveTo(320, 13);
            right.CurveTo(339, 83, 319, 187, 336, 248);

            return new[] { top, bottom, left, middle, right };
        }

        private static PathF CreatePin()
        {
            var pin = new PathF();

            pin.MoveTo(0, 0);
            pin.CurveTo(-5, -7, -20, -20, -20, -33);
            pin.CurveTo(-20, -59, 20, -59, 20, -33);
            pin.CurveTo(20, -20, 5, -7, 0, 0);
            pin.Close();

            return pin;
        }

        private static PointF[] CreateRouteDots()
        {
            var dots = new PointF[37];

            for (int i = 0; i < dots.Length; i++)
                dots[i] = RoutePoint(i / 36f);

            return dots;
        }

        private static PointF RoutePoint(float t)
        {
            t = Math.Clamp(t, 0f, 1f);

            return t <= 0.5f
                ? Bezier(
                    new PointF(56, 167),
                    new PointF(102, 170),
                    new PointF(133, 117),
                    new PointF(198, 136),
                    t * 2f)
                : Bezier(
                    new PointF(198, 136),
                    new PointF(250, 149),
                    new PointF(268, 157),
                    new PointF(332, 106),
                    (t - 0.5f) * 2f);
        }

        private static PointF Bezier(
            PointF a,
            PointF b,
            PointF c,
            PointF d,
            float t)
        {
            float u = 1f - t;

            return new PointF(
                u * u * u * a.X
                + 3 * u * u * t * b.X
                + 3 * u * t * t * c.X
                + t * t * t * d.X,

                u * u * u * a.Y
                + 3 * u * u * t * b.Y
                + 3 * u * t * t * c.Y
                + t * t * t * d.Y);
        }
    }
}