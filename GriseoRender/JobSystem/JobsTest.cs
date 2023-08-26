using System;

namespace GriseoRenderer.JobSystem;

public struct JobsTest : IJobFor
{
    public void Execute(int index)
    {
        Console.WriteLine($"Doing job:{index}");
    }
}