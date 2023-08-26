using System;
using System.Collections.Concurrent;
using System.Threading;

namespace GriseoRenderer.JobSystem;

public class JobScheduler
{
    private Semaphore _semaphore = new Semaphore(0, int.MaxValue);
    private Thread[] _threads = new Thread[16];
    private ConcurrentQueue<IJob> _bundles = new ConcurrentQueue<IJob>();

    public void Init()
    {
        for (int i = 0; i < 16; i++)
        {
            _threads[i] = new Thread(DoJob);
            _threads[i].IsBackground = true;
            _threads[i].Start();
        }
    }

    public JobHandle Schedule<T>(T jobs, int startIndex, int endIndex, int batchSize) where T : struct, IJobFor
    {
        if (endIndex <= startIndex)
        {
            throw new ArgumentException("Index error");
        }

        int bundleCount = 0;
        JobHandle handle = new JobHandle();
        for (int i = startIndex; i < endIndex; i += batchSize)
        {
            bundleCount++;
            int rightIndex = Math.Min(endIndex, i + batchSize);
            _bundles.Enqueue(new JobBundle<T>(jobs, i, rightIndex, handle));
        }
        
        handle.Init(bundleCount);
        _semaphore.Release(bundleCount);
        return handle;
    }

    private void DoJob()
    {
        while (true)
        {
            _semaphore.WaitOne();

            if (_bundles.TryDequeue(out var job))
            {
                job.Execute();
                Console.WriteLine($"Job Done");
            }
        }
    }
}