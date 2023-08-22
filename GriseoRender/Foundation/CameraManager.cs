using System;
using System.Numerics;
using GriseoRenderer.Render;

namespace GriseoRenderer.Foundation;

public class CameraManager
{
    public Camera MainCamera;

    public void Init()
    {
        MainCamera = new Camera(new Vector3(0, 0, 5), Quaternion.CreateFromYawPitchRoll(0, 0, 0));
    }
}