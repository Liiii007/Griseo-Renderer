using System;
using System.Threading;

namespace GriseoRenderer.JobSystem;

public class JobHandle
{
    private readonly object _lock = new object();
    private readonly int _jobSize;
    private int _completeCount;

    public JobHandle(int jobSize)
    {
        _jobSize = jobSize;
    }

    public void Complete()
    {
        Monitor.Enter(_lock);

        while (_completeCount < _jobSize)
        {
            Monitor.Wait(_lock);
        }
        
        Monitor.Exit(_lock);
    }

    public void CompleteOne()
    {
        Monitor.Enter(_lock);
        Interlocked.Increment(ref _completeCount);
        Monitor.PulseAll(_lock);
        Monitor.Exit(_lock);
    }
}