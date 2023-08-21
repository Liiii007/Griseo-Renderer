using System;
using System.Numerics;

namespace GriseoRender.Render;

public class Camera
{
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }
    
    public Vector3 Up => Vector3.Transform(new Vector3(0, 1, 0), Rotation);
    public Vector3 Forward => Vector3.Transform(new Vector3(0, 0, -1), Rotation);
    public Vector3 Right => Vector3.Transform(new Vector3(1, 0, 0), Rotation);
    
    public float Fov = 40;
    public float Ratio => 1280f / 720f;
    public float n = 0.1f;
    public float f = 10000f;
    public float t => MathF.Tan(MathF.PI / 180 * Fov / 2);
    public float b => -t;
    public float r => Ratio * t;
    public float l => -r;

    public Camera(Vector3 position, Quaternion rotation)
    {
        Position = position;
        Rotation = rotation;
    }

    public void AddPosition(Vector3 offset)
    {
        Position += offset;
    }

    public Matrix4x4 VP
    {
        get
        {
            //Move to camera position
            var tra = Matrix4x4.Identity;
            tra[0, 3] = -Position.X;
            tra[1, 3] = -Position.Y;
            tra[2, 3] = -Position.Z;

            //Rotate to camera space
            var rot = new Matrix4x4();
            var u = Right;
            var v = Up;
            var w = Forward;
            rot[0, 0] = u.X;
            rot[0, 1] = u.Y;
            rot[0, 2] = u.Z;
            rot[1, 0] = v.X;
            rot[1, 1] = v.Y;
            rot[1, 2] = v.Z;
            rot[2, 0] = w.X;
            rot[2, 1] = w.Y;
            rot[2, 2] = w.Z;
            rot[3, 3] = 1;

            var matrixView = rot * tra;

            //Persp to ortho
            var per = new Matrix4x4();
            per[0, 0] = n;
            per[1, 1] = n;
            per[2, 2] = n + f;
            per[2, 3] = -n * f;
            per[3, 2] = 1;

            var ort = new Matrix4x4();
            ort[0, 0] = 2 / (r - l);
            ort[0, 3] = -(r + l) / (r - l);
            ort[1, 1] = 2 / (t - b);
            ort[1, 3] = -(t + b) / (t - b);
            ort[2, 2] = 2 / (n - f);
            ort[2, 3] = -(n + f) / (n - f);
            ort[3, 3] = 1;

            var matrixProject = ort * per; 

            return matrixProject * matrixView;
        }
    }
}