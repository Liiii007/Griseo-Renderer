using System;

namespace GriseoRenderer.Foundation;

public static class Singleton<T> where T : new()
{
    private static readonly Lazy<T> LazyInstance = new Lazy<T>();

    public static T Instance => LazyInstance.Value;
}