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
        Singleton<InputManager>.Instance.OnW += () => { MainCamera.AddPosition(new Vector3(0, 0, -0.1f)); };
        Singleton<InputManager>.Instance.OnS += () => { MainCamera.AddPosition(new Vector3(0, 0, 0.1f)); };
        Singleton<InputManager>.Instance.OnA += () => { MainCamera.AddPosition(new Vector3(-0.1f, 0, 0)); };
        Singleton<InputManager>.Instance.OnD += () => { MainCamera.AddPosition(new Vector3(0.1f, 0, 0)); };
        Singleton<InputManager>.Instance.OnQ += () => { MainCamera.AddPosition(new Vector3(0, -0.1f, 0)); };
        Singleton<InputManager>.Instance.OnE += () => { MainCamera.AddPosition(new Vector3(0, 0.1f, 0)); };
    }
}