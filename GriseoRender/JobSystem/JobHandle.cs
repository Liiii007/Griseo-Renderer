using System;
using System.Threading;

namespace GriseoRenderer.JobSystem;

public class JobHandle
{
    private readonly object _lock = new object();
    private int _jobSize;
    private bool _isInit;
    private int _completeCount;

    public void Init(int size)
    {
        if (_isInit)
        {
            throw new Exception("Already init");
        }

        _isInit = true;
        _jobSize = size;
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
        var value = Interlocked.Increment(ref _completeCount);
        if (value == _jobSize)
        {
            Monitor.Pulse(_lock);
        }
        Monitor.Exit(_lock);
    }
}