using System.Numerics;

namespace GriseoRender;

public struct Translation
{
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }
    public Vector3 Scale { get; set; }

    public Matrix4x4 TRS { get; private set; }

    public Translation(Vector3 translation, Quaternion rotation, Vector3 scale)
    {
        Position = translation;
        Rotation = rotation;
        Scale = scale;
        
        Rebuild();
    }

    public void Rebuild()
    {
        Matrix4x4 mRot = Matrix4x4.CreateFromQuaternion(Rotation);
        Matrix4x4 mTra = Matrix4x4.Identity;
        Matrix4x4 mSca = Matrix4x4.Identity;

        mTra[0, 3] = Position.X;
        mTra[1, 3] = Position.Y;
        mTra[2, 3] = Position.Z;

        mSca[0, 0] = Scale.X;
        mSca[1, 1] = Scale.Y;
        mSca[2, 2] = Scale.Z;

        TRS = mTra * mSca * mRot;
    }

    public Matrix4x4 GetTransformMatrix()
    {
        return TRS;
    }

    public Vector4 Transform(Vector4 origin)
    {
        var result = Vector4.Transform(origin, TRS);
        return result;
    }
}