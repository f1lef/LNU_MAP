using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Fronted;

public sealed class ResponsiveLoginLayout : Grid
{
    private bool _updating;
    private double _lastWidth = -1;

    public ResponsiveLoginLayout()
    {
        RowDefinitions.Add(
            new RowDefinition { Height = GridLength.Auto });

        RowDefinitions.Add(
            new RowDefinition { Height = GridLength.Auto });

        ColumnDefinitions.Add(
            new ColumnDefinition { Width = GridLength.Star });

        Loaded += (_, _) => UpdateLayout();
        SizeChanged += (_, _) => UpdateLayout();
    }

    private void UpdateLayout()
    {
        if (_updating || Width <= 0 || Children.Count < 2 ||
            Children[0] is not View hero ||
            Children[1] is not Border form)
            return;

        if (Math.Abs(Width - _lastWidth) < 0.5)
            return;

        bool wide = false;

#if WINDOWS
        wide = Width >= 700;
#endif

        _updating = true;
        _lastWidth = Width;

        try
        {
            RowDefinitions.Clear();
            ColumnDefinitions.Clear();

            ColumnSpacing = 0;
            RowSpacing = 0;

            ColumnDefinitions.Add(
                new ColumnDefinition { Width = GridLength.Star });

            if (wide)
            {
                double formWidth = Math.Clamp(
                    Width * 0.44, 380, 480);

                double gap = Math.Clamp(
                    Width * 0.05, 32, 64);

                double heroWidth = Math.Min(
                    600, Width - formWidth - gap);

                ColumnDefinitions.Add(
                    new ColumnDefinition
                    {
                        Width = new GridLength(heroWidth)
                    });

                ColumnDefinitions.Add(
                    new ColumnDefinition
                    {
                        Width = new GridLength(gap)
                    });

                ColumnDefinitions.Add(
                    new ColumnDefinition
                    {
                        Width = new GridLength(formWidth)
                    });

                ColumnDefinitions.Add(
                    new ColumnDefinition
                    {
                        Width = GridLength.Star
                    });

                RowDefinitions.Add(
                    new RowDefinition
                    {
                        Height = GridLength.Auto
                    });

                Grid.SetRow((BindableObject)hero, 0);
                Grid.SetColumn((BindableObject)hero, 1);
                Grid.SetRow((BindableObject)form, 0);
                Grid.SetColumn((BindableObject)form, 3);

                hero.HeightRequest = Math.Clamp(
                    heroWidth * 0.7, 280, 420);

                hero.VerticalOptions = LayoutOptions.Center;

                form.Padding = new Thickness(
                    Math.Clamp(formWidth * 0.067, 24, 32));

                form.BackgroundColor = Color.FromArgb("#1C152D");

                form.Stroke = new SolidColorBrush(
                    Color.FromArgb("#3C2F51"));

                form.StrokeThickness = 1;
                form.VerticalOptions = LayoutOptions.Center;
            }
            else
            {
                double formWidth = Math.Min(440, Width);

                ColumnDefinitions.Add(
                    new ColumnDefinition
                    {
                        Width = new GridLength(formWidth)
                    });

                ColumnDefinitions.Add(
                    new ColumnDefinition
                    {
                        Width = GridLength.Star
                    });

                RowDefinitions.Add(
                    new RowDefinition
                    {
                        Height = GridLength.Auto
                    });

                RowDefinitions.Add(
                    new RowDefinition
                    {
                        Height = GridLength.Auto
                    });

                Grid.SetRow((BindableObject)hero, 0);
                Grid.SetColumn((BindableObject)hero, 1);
                Grid.SetRow((BindableObject)form, 1);
                Grid.SetColumn((BindableObject)form, 1);

                hero.HeightRequest = 228;
                hero.VerticalOptions = LayoutOptions.Start;

                form.Padding = new Thickness(0);
                form.BackgroundColor = Colors.Transparent;

                form.Stroke = new SolidColorBrush(
                    Colors.Transparent);

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