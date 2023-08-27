using System;
using System.Collections;
using System.Collections.Generic;
using GriseoRenderer.Foundation;
using GriseoRenderer.JobSystem;

namespace GriseoRenderer.JobSystem;

public class JobDependency
{
    public bool IsComplete = false;

    public IEnumerator Complete()
    {
        while (!IsComplete)
        {
            yield return null;
        }

        yield break;
    }
}

public abstract class JobBase
{
    private List<JobHandle> _handles = new();

    public abstract IEnumerator Execute();
}

public enum JobState
{
    Create,
    Doing,
    Wait,
    Done
}

public class WaitForSubJob : IEnumerator
{
    private JobHandle _handle;

    public WaitForSubJob(JobHandle handle)
    {
        _handle = handle;
    }

    public bool MoveNext()
    {
        return !_handle.IsComplete;
    }

    public void Reset()
    {
        throw new NotImplementedException();
    }

    public object Current => _handle;

    public bool IsComplete => _handle.IsComplete;
}

public struct SubJob : IYieldJobFor
{
    public IEnumerator Execute(int startIndex, int endIndex)
    {
        for (int i = startIndex; i < endIndex; i++)
        {
            Console.WriteLine(i);
        }

        yield break;
    }
}

public struct JobsTest : IYieldJobFor
{
    public IEnumerator Execute(int startIndex, int endIndex)
    {
        var subJob = new SubJob();
        var handle = JobScheduler.Schedule(subJob, 0, 1, 1);
        yield return new WaitForSubJob(handle);
    }
}