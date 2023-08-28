using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using GriseoRenderer.Foundation;

namespace GriseoRenderer.JobSystem;

public class JobScheduler
{
    private readonly Semaphore _semaphore = new Semaphore(0, int.MaxValue);
    private Thread[] _threads;
    private readonly ConcurrentQueue<Coroutine> _bundles = new();

    public void Init(int threadSize = 16)
    {
        _threads = new Thread[threadSize];
        for (int i = 0; i < threadSize; i++)
        {
            _threads[i] = new Thread(DoJob);
            _threads[i].IsBackground = true;
            _threads[i].Start();
        }
    }

    public static JobHandle Schedule<T>(T jobs, int startIndex, int endIndex, int batchSize,
        JobHandle dependency = null)
        where T : struct, IYieldJobFor
    {
        var instance = Singleton<JobScheduler>.Instance;
        var bundles = instance._bundles;
        var semaphore = instance._semaphore;

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
            var ie = jobs.Execute(i, rightIndex);
            Coroutine coroutine = new Coroutine(ie, handle, dependency);
            bundles.Enqueue(coroutine);
        }

        handle.Init(bundleCount);
        semaphore.Release(bundleCount);
        return handle;
    }

    private void AddWaitJob(Coroutine originJob)
    {
        _bundles.Enqueue(originJob);
        _semaphore.Release(1);
    }

    private void DoJob()
    {
        while (true)
        {
            _semaphore.WaitOne();

            if (_bundles.TryDequeue(out var job))
            {
                while (job.MoveNext())
                {
                    if (job.IsBlocking())
                    {
                        AddWaitJob(job);
                        break;
                    }
                }
            }
        }
    }
}