using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Fronted;

// Перший дочірній елемент — Grid з картою, другий — Border з формою.
public sealed class ResponsiveLoginLayout : Grid
{
    private bool _updating;
    private double _lastWidth = -1;

    public ResponsiveLoginLayout()
    {
        RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

        Loaded += (_, _) => UpdateLayout();
        SizeChanged += (_, _) => UpdateLayout();
    }

    private void UpdateLayout()
    {
        if (_updating || Width <= 0 || Children.Count < 2 ||
            Children[0] is not View hero || Children[1] is not Border form)
            return;

        if (Math.Abs(Width - _lastWidth) < 0.5)
            return;

        bool wide = false;
#if WINDOWS
        wide = Width >= 960;
#endif

        _updating = true;
        _lastWidth = Width;

        try
        {
            RowDefinitions.Clear();
            ColumnDefinitions.Clear();
            ColumnSpacing = 0;
            RowSpacing = 0;

            ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

            if (wide)
            {
                double heroWidth = Math.Min(600, Width - 480 - 64);

                ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(heroWidth) });
                ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(64) });
                ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(480) });
                ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                SetRow(hero, 0);
                SetColumn(hero, 1);
                SetRow(form, 0);
                SetColumn(form, 3);

                hero.HeightRequest = 420;
                hero.VerticalOptions = LayoutOptions.Center;

                form.Padding = new Thickness(32);
                form.BackgroundColor = Color.FromArgb("#1C152D");
                form.Stroke = new SolidColorBrush(Color.FromArgb("#3C2F51"));
                form.StrokeThickness = 1;
                form.VerticalOptions = LayoutOptions.Center;
            }
            else
            {
                double formWidth = Math.Min(440, Width);

                ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(formWidth) });
                ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                SetRow(hero, 0);
                SetColumn(hero, 1);
                SetRow(form, 1);
                SetColumn(form, 1);

                hero.HeightRequest = 228;
                hero.VerticalOptions = LayoutOptions.Start;

                form.Padding = new Thickness(0);
                form.BackgroundColor = Colors.Transparent;
                form.Stroke = new SolidColorBrush(Colors.Transparent);
                form.StrokeThickness = 0;
                form.VerticalOptions = LayoutOptions.Start;
            }
        }
        finally
        {
            _updating = false;
        }
    }
}
