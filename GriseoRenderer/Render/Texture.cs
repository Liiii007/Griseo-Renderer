using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GriseoRenderer.Render;

public class Texture
{
    private Image<Rgba32> _bitmap;

    public Texture(string path)
    {
        _bitmap = Image.Load<Rgba32>(path);
    }

    public int Width => _bitmap.Width;
    public int Height => _bitmap.Height;

    public RealColor this[int x, int y]
    {
        get
        {
            var color = _bitmap[Math.Clamp(x, 0, Width - 1), Math.Clamp(y, 0, Height - 1)];
            return new RealColor((float)color.R / 256, (float)color.G / 256, (float)color.B / 256,
                (float)color.A / 256);
        }
    }
}