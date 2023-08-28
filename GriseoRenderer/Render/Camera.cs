using System;
using System.Numerics;

namespace GriseoRenderer.Render;

public class Camera
{
    public bool IsOrith { get; set; }
    public Vector4 Position { get; set; }
    public Quaternion Rotation { get; set; }

    public Vector3 Up => Vector3.Transform(new Vector3(0, 1, 0), Rotation);
    public Vector4 Forward => Vector4.Transform(new Vector4(0, 0, -1, 0), Rotation);
    public Vector3 Right => Vector3.Transform(new Vector3(1, 0, 0), Rotation);

    public float Width { get; set; } = 1280f;
    public float Height { get; set; } = 720f;

    public float Fov = 45f;
    public float Ratio => Width / Height;
    public float n = 0.1f;
    public float f = 100f;
    public float t => MathF.Tan(MathF.PI / 180 * Fov / 2) * (IsOrith ? f : n);
    public float b => -t;
    public float r => Ratio * t;
    public float l => -r;

    public Camera(Vector3 position, Quaternion rotation)
    {
        Position = new(position.X, position.Y, position.Z, 1);
        Rotation = rotation;
    }

    public void AddPosition(Vector3 offset)
    {
        Position = Position.Add(offset);
    }

    public Matrix4x4 View
    {
        get
        {
            //Move to camera position
            var tra = Matrix4x4.Identity;
            tra[0, 3] = -Position.X;
            tra[1, 3] = -Position.Y;
            tra[2, 3] = -Position.Z;

            //Rotate to camera space
            var rot = Matrix4x4.Identity;
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

            var matrixView = rot * tra;
            return matrixView;
        }
    }

    public Matrix4x4 VP
    {
        get
        {
            var matrixView = View;

            if (IsOrith)
            {
                var ort = new Matrix4x4();
                ort[0, 0] = 2 / (r - l);
                ort[0, 3] = -(r + l) / (r - l);
                ort[1, 1] = 2 / (t - b);
                ort[1, 3] = -(t + b) / (t - b);
                ort[2, 2] = 2 / (n - f);
                ort[2, 3] = -(n + f) / (n - f);
                ort[3, 3] = 1;

                return ort * matrixView;
            }
            else
            {
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
}