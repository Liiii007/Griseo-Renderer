using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace GriseoRenderer.JobSystem;

public class JobHandle
{
    private readonly object _lock = new object();
    private int _jobSize;
    private bool _isInit;
    private int _completeCount;
    public bool IsComplete { get; private set; }

    public void Init(int size)
    {
        if (_isInit)
        {
            throw new Exception("Already init");
        }

        _isInit = true;
        IsComplete = false;
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
            IsComplete = true;
            Monitor.PulseAll(_lock);
        }

        Monitor.Exit(_lock);
    }
}