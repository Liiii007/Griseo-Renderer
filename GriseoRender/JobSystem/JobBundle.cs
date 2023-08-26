namespace GriseoRenderer.JobSystem;

public class JobBundle<T> : IJob where T : struct, IJobFor
{
    private int _minIndex;
    private int _maxIndex;
    private JobHandle _handle;

    private T _job;

    public JobBundle(T job, int minIndex, int maxIndex, JobHandle handle)
    {
        _handle = handle;
        _job = job;
        _minIndex = minIndex;
        _maxIndex = maxIndex;
    }

    public void Execute()
    {
        for (int i = _minIndex; i < _maxIndex; i++)
        {
            _job.Execute(i);
        }
        
        _handle.CompleteOne();
    }
}