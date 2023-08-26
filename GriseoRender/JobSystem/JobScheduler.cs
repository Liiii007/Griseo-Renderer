using System;
using System.Collections.Concurrent;
using System.Threading;

namespace GriseoRenderer.JobSystem;

public class JobScheduler
{
    private Semaphore _semaphore = new Semaphore(0, 65536);
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

    public JobHandle Schedule<T>(T jobs, int startIndex, int endIndex, int bundleSize) where T : struct, IJobFor
    {
        if (endIndex <= startIndex)
        {
            throw new ArgumentException("Index error");
        }

        JobHandle handle = new JobHandle((endIndex - startIndex) / bundleSize + 1);
        int bundleCount = 0;
        for (int i = startIndex; i < endIndex; i += bundleSize)
        {
            bundleCount++;
            int rightIndex = Math.Min(endIndex, i + bundleSize);
            _bundles.Enqueue(new JobBundle<T>(jobs, i, rightIndex, handle));
        }

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