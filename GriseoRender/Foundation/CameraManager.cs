using System.Numerics;
using GriseoRender.Render;

namespace GriseoRender.Foundation;

public class CameraManager
{
    public Camera MainCamera;

    public void Init()
    {
        MainCamera = new Camera(new Vector3(0, 0, 5), Quaternion.CreateFromYawPitchRoll(0, 0, 0));
    }
}