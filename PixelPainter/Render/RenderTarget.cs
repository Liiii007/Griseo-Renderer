using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixelPainter.Render;

public struct MColor
{
    public float R
    {
        get => (float)_r / 255;
        set => _r = (ushort)(value * 255);
    }

    public float G
    {
        get => (float)_g / 255;
        set => _g = (ushort)(value * 255);
    }

    public float B
    {
        get => (float)_b / 255;
        set => _b = (ushort)(value * 255);
    }

    public float A
    {
        get => (float)_a / 255;
        set => _a = (ushort)(value * 255);
    }

    public static MColor GetColor(int argbValue)
    {
        var color = new MColor
        {
            _a = (ushort)((argbValue >> 24) & 0xFF),
            _r = (ushort)((argbValue >> 16) & 0xFF),
            _g = (ushort)((argbValue >> 8)  & 0xFF),
            _b = (ushort)(argbValue         & 0xFF)
        };
        return color;
    }

    public int Argb => (_a << 24) | (_r << 16) | (_g << 8) | _b;

    private ushort _r;
    private ushort _g;
    private ushort _b;
    private ushort _a;
}

public class RenderTarget
{
    public  int             Width  { get; private set; }
    public  int             Height { get; private set; }
    public  int             Length => Width * Height;
    public int[]           _pixels;
    private WriteableBitmap _bitmap;

    public RenderTarget(int width, int height)
    {
        Width   = width;
        Height  = height;
        _pixels = new int[Width * Height];
        _bitmap = new WriteableBitmap(Width, Height, 96, 96, PixelFormats.Bgra32, null);
    }

    public void ApplyTo(Image image)
    {
        _bitmap.Lock();
        Int32Rect rect = new Int32Rect(0, 0, Width, Height);
        _bitmap.WritePixels(rect, _pixels, Width * 4, 0);
        image.Source = _bitmap;
        _bitmap.Unlock();
    }

    public void Clear()
    {
        for (int i = 0; i < _pixels.Length; i++)
        {
            _pixels[i] = unchecked((int)0xFF000000);
        }
    }

    public void SetColor(MColor color, int index)
    {
        _pixels[index] = color.Argb;
    }

    public void SetColor(MColor color, int x, int y)
    {
        _pixels[x + y * Width] = color.Argb;
    }

    public MColor GetColor(int x, int y)
    {
        return MColor.GetColor(_pixels[x + y * Width]);
    }

    public int[] GetPixelsArray()
    {
        return _pixels;
    }
}