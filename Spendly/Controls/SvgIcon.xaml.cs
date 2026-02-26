using System.Collections.Concurrent;
using System.IO;
using System.Windows;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using Svg.Skia;

namespace Spendly.Controls;

public partial class SvgIcon
{
    private static readonly ConcurrentDictionary<string, SKPicture?> Cache = new();

    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.Register(
            nameof(Source),
            typeof(string),
            typeof(SvgIcon),
            new PropertyMetadata(null, OnAnyPropertyChanged));

    public string? Source
    {
        get => (string?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public SvgIcon()
    {
        InitializeComponent();
    }

    private static void OnAnyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SvgIcon icon)
            icon.Sk.InvalidateVisual();
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        
        canvas.Clear(SKColors.Transparent);

        if (string.IsNullOrWhiteSpace(Source))
            return;

        var picture = Cache.GetOrAdd(Source, LoadPicture);
        if (picture is null)
            return;

        var bounds = picture.CullRect;
        if (bounds.Width <= 0 || bounds.Height <= 0)
            return;

        var scale = Math.Min(e.Info.Width / bounds.Width, e.Info.Height / bounds.Height);
        var dx = (e.Info.Width - bounds.Width * scale) / 2f;
        var dy = (e.Info.Height - bounds.Height * scale) / 2f;

        canvas.Translate(dx, dy);
        canvas.Scale(scale);
        canvas.Translate(-bounds.Left, -bounds.Top);

        canvas.DrawPicture(picture);
    }

    private static SKPicture? LoadPicture(string key)
    {
        try
        {
            if (Uri.TryCreate(key, UriKind.Absolute, out var uri) && uri.Scheme == "pack")
            {
                var sri = System.Windows.Application.GetResourceStream(uri);
                var stream = sri?.Stream;

                stream ??= System.Windows.Application.GetContentStream(uri)?.Stream;

                if (stream is null) return null;

                var svg = new SKSvg();
                svg.Load(stream);
                return svg.Picture;
            }

            if (!File.Exists(key)) return null;

            using var fs = File.OpenRead(key);
            var svgFile = new SKSvg();
            svgFile.Load(fs);
            return svgFile.Picture;
        }
        catch
        {
            return null;
        }
    }
}
