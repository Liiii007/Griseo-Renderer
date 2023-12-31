﻿using System.Numerics;
using GriseoRenderer.Resources;

namespace GriseoRenderer.Render;

public class RenderObject
{
    public Mesh Mesh;
    public Translation Translation;
    public VertexOut[] VerticesOut;
    public Texture MainTex;

    public RenderObject(Mesh mesh, Texture mainTex = null)
    {
        Mesh = mesh;
        MainTex = mainTex;
        VerticesOut = new VertexOut[Mesh.Vertices.Length];
        Translation = new Translation(new Vector3(0, 0, 0), Quaternion.CreateFromYawPitchRoll(0, 0, 0),
            new Vector3(1, 1, 1));
    }

    public Matrix4x4 M => Translation.GetTransformMatrix();
}