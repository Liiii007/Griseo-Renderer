using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixelPainter.Render;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ScreenColor
{
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

    private byte _b;
    private byte _g;
    private byte _r;
    private byte _a;
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
        void* targetPtr = (void*)_bitmap.BackBuffer;
        fixed (void* sourcePtr = _pixels)
        {
            Buffer.MemoryCopy(sourcePtr, targetPtr, _pixels.Length * 4, _pixels.Length * 4);
        }
        
        _bitmap.AddDirtyRect(new Int32Rect(0,0,_bitmap.PixelWidth, _bitmap.PixelHeight));

        _bitmap.Unlock();
        image.Source = _bitmap;
    }

    public void Clear()
    {
        ScreenColor black = new()
        {
            R = 0, B = 0, G = 0, A = 1
        };

        for (int i = 0; i < _pixels.Length; i++)
        {
            _pixels[i] = black;
        }
    }

    public ScreenColor this[int index]
    {
        get => _pixels[index];
        set => _pixels[index] = value;
    }

    public ScreenColor[] GetPixelsArray()
    {
        return _pixels;
    }
}