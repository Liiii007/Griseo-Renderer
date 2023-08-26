using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GriseoRenderer.Render;

public struct RealColor
{
    public float B;
    public float G;
    public float R;
    public float A;

    public RealColor(float r, float g, float b, float a = 1)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public RealColor(float c) : this(c, c, c, 1)
    {
    }

    public RealColor(ScreenColor color) : this(color.R, color.G, color.B, color.A)
    {
    }

    public ScreenColor AsScreenColor
    {
        get
        {
            return new ScreenColor(RenderMath.Clamp01(R), RenderMath.Clamp01(G), RenderMath.Clamp01(B),
                RenderMath.Clamp01(A));
        }
    }

    public RealColor Step0 => new(Math.Max(0, R), Math.Max(0, G), Math.Max(0, B), Math.Max(0, A));

    public static RealColor operator +(RealColor a, RealColor b)
    {
        return new RealColor(a.R + b.R, a.G + b.G, a.B + b.B, a.A + b.A);
    }

    public static RealColor operator -(RealColor a, RealColor b)
    {
        return new RealColor(a.R - b.R, a.G - b.G, a.B - b.B, a.A - b.A);
    }

    public static RealColor operator *(RealColor a, float rate)
    {
        return new RealColor(a.R * rate, a.G * rate, a.B * rate, a.A * rate);
    }

    public static RealColor operator *(float rate, RealColor a)
    {
        return a * rate;
    }

    public static RealColor operator /(RealColor a, float rate)
    {
        return new RealColor(a.R / rate, a.G / rate, a.B / rate, a.A / rate);
    }

    public static RealColor operator /(float rate, RealColor a)
    {
        return a / rate;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ScreenColor
{
    private byte _b;
    private byte _g;
    private byte _r;
    private byte _a;

    public ScreenColor()
    {
    }

    public ScreenColor(float r, float g, float b, float a = 1)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public ScreenColor(float c, float a = 1)
    {
        R = c;
        G = c;
        B = c;
        A = a;
    }

    public float R
    {
        get => (float)_r / 255;
        set => _r = (byte)(Math.Clamp(value, 0, 1) * 255);
    }

    public float G
    {
        get => (float)_g / 255;
        set => _g = (byte)(Math.Clamp(value, 0, 1) * 255);
    }

    public float B
    {
        get => (float)_b / 255;
        set => _b = (byte)(Math.Clamp(value, 0, 1) * 255);
    }

    public float A
    {
        get => (float)_a / 255;
        set => _a = (byte)(Math.Clamp(value, 0, 1) * 255);
    }

    public static ScreenColor GetColor(int argbValue)
    {
        var color = new ScreenColor
        {
            _a = (byte)((argbValue >> 24) & 0xFF),
            _r = (byte)((argbValue >> 16) & 0xFF),
            _g = (byte)((argbValue >> 8) & 0xFF),
            _b = (byte)(argbValue & 0xFF)
        };
        return color;
    }

    public int Argb => (_a << 24) | (_r << 16) | (_g << 8) | _b;

    public static ScreenColor Black => new ScreenColor(0);
    public static ScreenColor White => new ScreenColor(1);
    public static ScreenColor Red => new ScreenColor(1, 0, 0);
}

public class DepthRenderTarget
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int Length => Width * Height;
    public float[] _pixels;
    private WriteableBitmap _bitmap;

    public DepthRenderTarget(int width, int height)
    {
        Width = width;
        Height = height;
        _pixels = new float[Width * Height];
        _bitmap = new WriteableBitmap(Width, Height, 96, 96, PixelFormats.Bgra32, null);
    }
    
    public unsafe void ApplyTo(Image image)
    {
        _bitmap.Lock();

        for (int i = 0; i < _pixels.Length; i++)
        {
            var color = new ScreenColor(_pixels[i]);
        }

        _bitmap.AddDirtyRect(new Int32Rect(0, 0, _bitmap.PixelWidth, _bitmap.PixelHeight));
        _bitmap.Unlock();
        image.Source = _bitmap;
    }

    public void Clear(float value)
    {
        var span = new Span<float>(_pixels);
        span.Fill(value);
    }

    public float this[int x, int y]
    {
        get => _pixels[x + (Height - y - 1) * Width];
        set => _pixels[x + (Height - y - 1) * Width] = value;
    }

    public float[] GetPixelsArray()
    {
        return _pixels;
    }
}

public class RenderTarget
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int Length => Width * Height;
    public ScreenColor[] _pixels;
    private WriteableBitmap _bitmap;

    public RenderTarget(int width, int height)
    {
        Width = width;
        Height = height;
        _pixels = new ScreenColor[Width * Height];
        _bitmap = new WriteableBitmap(Width, Height, 96, 96, PixelFormats.Bgra32, null);
    }

    public unsafe void ApplyTo(Image image)
    {
        _bitmap.Lock();

        var sourceSpan = new Span<ScreenColor>(_pixels);
        var targetSpan = new Span<ScreenColor>((void*)_bitmap.BackBuffer, _pixels.Length);
        sourceSpan.CopyTo(targetSpan);

        _bitmap.AddDirtyRect(new Int32Rect(0, 0, _bitmap.PixelWidth, _bitmap.PixelHeight));
        _bitmap.Unlock();
        image.Source = _bitmap;
    }

    public void Clear(ScreenColor color)
    {
        var span = new Span<ScreenColor>(_pixels);
        span.Fill(color);
    }

    public ScreenColor this[int x, int y]
    {
        get => _pixels[x + (Height - y - 1) * Width];
        set => _pixels[x + (Height - y - 1) * Width] = value;
    }

    public ScreenColor[] GetPixelsArray()
    {
        return _pixels;
    }
}